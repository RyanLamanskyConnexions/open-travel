using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Diagnostics.Debug;

namespace Connexions.OpenTravel.UserInterface
{
	/// <summary>
	/// Maintains a web socket connection which represents a user's session.
	/// </summary>
	sealed class Session : IServiceProvider, IDisposable
	{
		#region Session Properties
		/// <summary>
		/// 2-character ISO code that indicates the traveler's nationality.
		/// </summary>
		public string Nationality { get; set; }

		/// <summary>
		/// 2-character ISO code that indicates the traveler's country of residence. 
		/// </summary>
		public string CountryOfResidence { get; set; }

		/// <summary>
		/// Retains arbitrary data for a user's session.
		/// </summary>
		private readonly ConcurrentDictionary<Type, object> data = new ConcurrentDictionary<Type, object>();

		/// <summary>
		/// Gets or adds an item from/to the session local memory data storage.
		/// </summary>
		/// <typeparam name="T">The type of item being retrieved or added.</typeparam>
		/// <param name="key">Identifies the item to add.</param>
		/// <param name="valueFactory">If the value does not currently exist, this is called to provide one to store.</param>
		/// <returns>The stored item.</returns>
		public T GetOrAdd<T>(Type key, Func<Type, T> valueFactory) => (T)this.data.GetOrAdd(key, type => valueFactory(type));
		#endregion

		public T GetService<T>() => (T)this.GetService(typeof(T));

		#region WebSocket Management
		/// <summary>
		/// Thread-safe when properties are not changed so reduce garbage collection pressure by re-using the same instance.
		/// </summary>
		private static readonly JsonSerializer jsonSerializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };

		/// <summary>
		/// Thread-safe so reduce garbage collection pressure by re-using the same instance.
		/// </summary>
		private static readonly UTF8Encoding UTF8 = new UTF8Encoding(false);

		private readonly IServiceProvider services;

		/// <summary>
		/// Restricts access to <see cref="sendMessage"/> to a single concurrent user.
		/// </summary>
		private readonly SemaphoreSlim sendSync = new SemaphoreSlim(1, 1);

		/// <summary>
		/// Indicates that the session has been ended and any pending requests should be aborted.
		/// </summary>
		public CancellationToken CancellationToken { get; }

		private readonly Func<Task<ArraySegment<Byte>>> receiveMessage;
		private readonly Func<ArraySegment<Byte>, Task> sendMessage;
		private Task listener = Task.CompletedTask;

		private readonly ConcurrentDictionary<Task, Int64> operations = new ConcurrentDictionary<Task, Int64>();

		private Session(IServiceProvider services, CancellationToken cancellationToken, Func<Task<ArraySegment<Byte>>> receiveMessage, Func<ArraySegment<Byte>, Task> sendMessage)
		{
			this.services = services ?? throw new ArgumentNullException(nameof(services));
			this.CancellationToken = cancellationToken;
			this.receiveMessage = receiveMessage ?? throw new ArgumentNullException(nameof(receiveMessage));
			this.sendMessage = sendMessage ?? throw new ArgumentNullException(nameof(sendMessage));
		}

		public object GetService(Type serviceType) => this.services.GetService(serviceType);

		async Task Listen()
		{
			var bytes = await receiveMessage();
			if (bytes.Count == 0)
				return;

			var returnedStringFromClient = Encoding.UTF8.GetString(bytes.Array, bytes.Offset, bytes.Count);

			var command = JsonConvert.DeserializeObject<ICommand>(
				returnedStringFromClient,
				new JsonSerializerSettings
				{
					TypeNameHandling = TypeNameHandling.Objects
				}
				);

			var task = command.ExecuteAsync(this);
			this.operations.TryAdd(task, command.Sequence);
		}

		/// <summary>
		/// Sends a message to the client with an unlimited timeout.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <returns>A task that returns true if the message was sent, otherwise false.</returns>
		public Task<Boolean> SendAsync(ICommandMessage message) => SendAsync(message, TimeSpan.FromMilliseconds(-1));

		/// <summary>
		/// Sends a message to the client with the provided timeout.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <param name="timeout">The amount of time to wait for concurrent sends to be completed.</param>
		/// <returns>A task that returns true if the message was sent, otherwise false.</returns>
		public async Task<Boolean> SendAsync(ICommandMessage message, TimeSpan timeout)
		{
			ArraySegment<byte> messageBytes;
			using (var memory = new MemoryStream())
			{
				using (var writer = new StreamWriter(memory, UTF8, 4 * 1024, true))
				{
					jsonSerializer.Serialize(writer, message);
				}

				messageBytes = new ArraySegment<byte>(memory.ToArray());
			}

			if (!await this.sendSync.WaitAsync(timeout, this.CancellationToken))
				return false;

			try
			{
				await this.sendMessage(messageBytes);
				return true;
			}
			finally
			{
				this.sendSync.Release();
			}
		}

		/// <summary>
		/// Retrieves the tasks associated with of all active operations.
		/// </summary>
		/// <returns>Active operations.</returns>
		/// <remarks>The request listener is always the first item.</remarks>
		private IEnumerable<Task> GetOperations()
		{
			if (listener.IsCompleted)
				listener = Listen();

			yield return listener;

			var failedOperations = new List<KeyValuePair<Task, long>>();

			foreach (var kv in this.operations)
			{
				var item = kv.Key;
				if (item.IsCompleted)
				{
					this.operations.TryRemove(item, out var unused);
					Exception x = item.Exception;
					if (x != null)
					{
						if (x is AggregateException aggregate && aggregate.InnerExceptions.Count == 1)
						{
							x = aggregate.InnerExceptions[0];
						}
						GetService<ILoggerFactory>().CreateLogger("WebSocket Operations").LogError(x.ToString());
						failedOperations.Add(new KeyValuePair<Task, long>(this.SendAsync(new CommandMessage { Sequence = kv.Value, ErrorMessage = x.ToString(), RanToCompletion = true }), kv.Value));
					}
					continue;
				}

				yield return item;
			}

			foreach (var item in failedOperations)
				this.operations.TryAdd(item.Key, item.Value);
		}

		#region IDisposable
		/// <summary>
		/// When true, a call to <see cref="IDisposable.Dispose"/> does nothing.
		/// </summary>
		private bool disposed;

		/// <summary>
		/// Releases unmanaged items associated with this instance.
		/// </summary>
		void IDisposable.Dispose()
		{
			if (disposed)
				return;

			this.sendSync.Dispose();
			disposed = true;
		}
		#endregion

		public static async Task WebSocketHandlerAsync(HttpContext context, Func<Task> notUsed)
		{
			Assert(context != null);

			if (!context.WebSockets.IsWebSocketRequest)
				return;

			using (var socket = await context.WebSockets.AcceptWebSocketAsync())
			using (var socketClosedTokenSource = new CancellationTokenSource())
			using (var cancellation = CancellationTokenSource.CreateLinkedTokenSource(socketClosedTokenSource.Token, context.RequestAborted))
			using (var session = new Session(
				context.RequestServices,
				cancellation.Token,
				async () =>
				{
					var buffer = new Byte[16 * 1024];
					var segment = new ArraySegment<Byte>(buffer);
					var received = await socket.ReceiveAsync(segment, cancellation.Token);

					return new ArraySegment<Byte>(buffer, 0, received.Count);
				},
				async segment => await socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellation.Token)
				))
			{
				try
				{
					while (socket.State == WebSocketState.Open)
					{
						await Task.WhenAny(session.GetOperations());
					}
				}
				finally
				{
					socketClosedTokenSource.Cancel();
				}
			} //end using manager and cancellation token sources
		}
		#endregion
	}
}
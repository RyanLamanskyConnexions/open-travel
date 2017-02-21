using Microsoft.AspNetCore.Http;
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
	sealed class Session : IServiceProvider, IDisposable
	{
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
		public readonly CancellationToken CancellationToken;

		private readonly Func<Task<ArraySegment<Byte>>> receiveMessage;
		private readonly Func<ArraySegment<Byte>, Task> sendMessage;
		private Task listener = Task.CompletedTask;

		private readonly ConcurrentDictionary<Task, Task> operations = new ConcurrentDictionary<Task, Task>();

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
			this.operations.TryAdd(task, task);
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
		public IEnumerable<Task> GetOperations()
		{
			if (listener.IsCompleted)
				listener = Listen();

			yield return listener;

			foreach (var item in this.operations.Keys)
			{
				if (item.IsCompleted)
				{
					this.operations.TryRemove(item, out var unused);
					continue;
				}

				yield return item;
			}
		}

		#region IDisposable
		/// <summary>
		/// When true, a call to <see cref="Dispose"/> does nothing.
		/// </summary>
		private bool disposed;

		/// <summary>
		/// Releases unmanaged items associated with this instance.
		/// </summary>
		public void Dispose()
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
	}
}
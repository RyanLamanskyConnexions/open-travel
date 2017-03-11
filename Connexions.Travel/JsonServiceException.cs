using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;

namespace Connexions.Travel
{
	/// <summary>
	/// Encapsulates errors that occur during a call to a JSON service.
	/// </summary>
	[JsonObject(MemberSerialization = MemberSerialization.OptOut)]
	public class JsonServiceException : Exception
	{
		/// <summary>
		/// Creates a new <see cref="JsonSerializationException"/> instance.
		/// </summary>
		public JsonServiceException()
		{
		}

		/// <summary>
		/// Creates a new <see cref="JsonSerializationException"/> instance with the provided message.
		/// </summary>
		/// <param name="message">Populates the <see cref="Exception.Message"/> property.</param>
		public JsonServiceException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Creates a new <see cref="JsonSerializationException"/> instance with the provided message and inner exception.
		/// </summary>
		/// <param name="message">Populates the <see cref="Exception.Message"/> property.</param>
		/// <param name="innerException">Populates the <see cref="Exception.InnerException"/> property.</param>
		public JsonServiceException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Creates a new <see cref="JsonServiceException"/> wrapping the provided <see cref="HttpResponseMessage"/>.
		/// </summary>
		/// <param name="response">The message to be wrapped.</param>
		/// <param name="rawBody">The unparsed body</param>
		public JsonServiceException(HttpResponseMessage response, string rawBody)
			: this(response?.StatusCode ?? default(HttpStatusCode), rawBody)
		{
			if (response == null)
				throw new ArgumentNullException(nameof(response));
		}

		/// <summary>
		/// Creates a new <see cref="JsonServiceException"/> wrapping the provided <see cref="HttpStatusCode"/> and raw content body.
		/// </summary>
		/// <param name="statusCode">The message to be wrapped.</param>
		/// <param name="rawBody">The unparsed body</param>
		public JsonServiceException(HttpStatusCode statusCode, string rawBody)
			: base("Error occurred with JSON service call.")
		{
			StatusCode = statusCode;

			if (rawBody == null)
				return;

			try
			{
				Body = JsonConvert.DeserializeObject(rawBody);
			}
			catch
			{
				try
				{
					Body = XDocument.Parse(rawBody);
				}
				catch
				{
					Body = rawBody;
				}
			}
		}

		/// <summary>
		/// The HTTP status code from the response.
		/// </summary>
		[JsonProperty]
		public HttpStatusCode StatusCode { get; }

		/// <summary>
		/// If provided, the response which will be either an object or a string depending on whether JSON parsing succeeded.
		/// </summary>
		[JsonProperty]
		public object Body { get; }
	}
}
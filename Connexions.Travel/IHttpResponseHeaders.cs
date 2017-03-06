using System.Collections.Generic;
using System.Net;

namespace Connexions.Travel
{
	/// <summary>
	/// Implementing this interface causes the <see cref="ICapiClient"/> implementations to include this information in the return object.
	/// </summary>
	interface IHttpResponseHeaders
	{
		/// <summary>
		/// The HTTP status code returned by the remote service.
		/// </summary>
		HttpStatusCode HttpStatusCode { get; set; }

		/// <summary>
		/// A collection of HTTP header values, keyed by header name.
		/// </summary>
		IDictionary<string, string> Headers { get; set; }
	}
}
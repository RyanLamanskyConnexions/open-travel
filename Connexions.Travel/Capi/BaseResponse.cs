using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

#pragma warning disable IDE1006 // CAPI naming styles follow a different standard than .NET

namespace Connexions.Travel.Capi
{
	class BaseResponse : IHttpResponseHeaders
	{
		/// <summary>
		/// Only populated when an error occurs, a predefined error code.
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string code { get; set; }

		/// <summary>
		/// Only populated when an error occurs, a human-readable message hopefully describing the problem.
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string message { get; set; }

		public class Information
		{
			public string code { get; set; }
			public string message { get; set; }
		}

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Information[] info { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public HttpStatusCode HttpStatusCode { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public IDictionary<string, string> Headers { get; set; }

		/// <summary>
		/// Removes logging info and organizes/trims data for delivery to a client web browser.
		/// </summary>
		public virtual void PrepareForClient()
		{
			this.HttpStatusCode = 0;
			this.Headers = null;
		}
	}
}
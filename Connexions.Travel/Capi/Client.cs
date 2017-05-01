using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connexions.Travel.Capi
{
	class Client : ICapiClient
	{
		private readonly Configuration.IServiceResolver resolver;

		public Client(Configuration.IServiceResolver resolver)
		{
			this.resolver = resolver;
		}

		async Task<T> ICapiClient.PostAsync<T>(string path, object body, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var service = this.resolver.GetServiceForRequest(path);

			using (var message = new HttpRequestMessage(HttpMethod.Post, service.Url + path))
			using (var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip })
			using (var client = new HttpClient(handler))
			{
				message.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
				message.Headers.Add("oski-tenantId", service.TenantId);
				message.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

				using (var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
				{
					if (response.StatusCode == HttpStatusCode.Redirect)
						throw new JsonServiceException(response.StatusCode, null, "Service returned HTTP Redirect unexpectedly" + (response.Headers.TryGetValues("Location", out var values) ? $", suggested URL: {values.FirstOrDefault()}" : "."));

					if (response.Content.Headers.ContentType.MediaType != "application/json")
						throw new JsonServiceException(response, await response.Content.ReadAsStringAsync());

					using (var stream = await response.Content.ReadAsStreamAsync())
					using (var textReader = new System.IO.StreamReader(stream, Encoding.UTF8, true, 1 << 12, true))
					using (var jsonReader = new JsonTextReader(textReader))
					{
						var result = new JsonSerializer().Deserialize<T>(jsonReader);
						if (result is IHttpResponseHeaders httpInfo)
						{
							httpInfo.HttpStatusCode = response.StatusCode;
							httpInfo.Headers = response.Headers.ToDictionary(kv => kv.Key, kv => kv.Value.First(), StringComparer.OrdinalIgnoreCase);
						}
						return result;
					} //end using response stream
				}
			}
		}
	}
}
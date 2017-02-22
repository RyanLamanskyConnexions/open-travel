using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connexions.OpenTravel.UserInterface
{
	class CapiClient : ICapiClient
	{
		/// <summary>
		/// Thread safe as long as settings aren't changed.
		/// </summary>
		static readonly JsonSerializer json = new JsonSerializer();

		async Task<T> ICapiClient.PostAsync<T>(string path, object body, CancellationToken cancellationToken)
		{
			using (var message = new HttpRequestMessage(HttpMethod.Post, Configuration.Capi.Url + path))
			using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip }))
			{
				message.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
				message.Headers.Add("oski-tenantId", Configuration.Capi.TenantId);
				message.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

				using (var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
				{
					if (response.IsSuccessStatusCode == false || response.Content.Headers.ContentType.MediaType != "application/json")
						throw new JsonServiceException(response, await response.Content.ReadAsStringAsync());

					using (var stream = await response.Content.ReadAsStreamAsync())
					using (var textReader = new System.IO.StreamReader(stream, Encoding.UTF8, true, 1 << 12, true))
					using (var jsonReader = new JsonTextReader(textReader))
					{
						var result = json.Deserialize<T>(jsonReader);
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
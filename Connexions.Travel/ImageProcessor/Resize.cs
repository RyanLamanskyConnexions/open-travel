using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Connexions.Travel.ImageProcessor
{
	static class Resize
	{
		internal static void Configure(IApplicationBuilder app)
		{
			app.Run(HandleAsync);
		}

		/// <summary>
		/// The expiration time for both client and server caches.
		/// </summary>
		static readonly TimeSpan ExpireAfter = TimeSpan.FromHours(5);

		/// <summary>
		/// A file cache entry, used for both original and resized images.
		/// </summary>
		sealed class File
		{
			public readonly string ContentType;
			public readonly byte[] Content;
			public readonly DateTimeOffset Created = DateTimeOffset.UtcNow;

			public File(string contentType, byte[] content)
			{
				ContentType = contentType;
				Content = content;
			}

			public Task WriteAsync(HttpContext context)
			{
				if (
					DateTimeOffset.TryParseExact(
						context.Request.Headers["If-Modified-Since"],
						"r",
						CultureInfo.InvariantCulture,
						DateTimeStyles.None,
						out var ifModifiedSince)
					&&
					ifModifiedSince.AddSeconds(1) >= Created //If-Modified-Since is only precise to the second, so add a 1-second buffer.
					)
				{
					context.Response.StatusCode = (int)HttpStatusCode.NotModified; ;
					return Task.CompletedTask;
				}

				context.Response.Headers.Add("Cache-Control", $"public, max-age={ExpireAfter.TotalSeconds:0000000}");
				context.Response.ContentType = ContentType;
				context.Response.ContentLength = Content.Length;
				return context.Response.Body.WriteAsync(Content, 0, Content.Length);
			}
		}

		/// <summary>
		/// Describes how an image should be resized.
		/// </summary>
		enum ResizingMode
		{
			/// <summary>
			/// Reduces the size of the image until it entirely fits within the provided dimensions.
			/// </summary>
			Fit = 0,
			/// <summary>
			/// Centers the image within the dimensions and reduces its size until it matches in one dimension.
			/// </summary>
			Fill = 1
		}

		/// <summary>
		/// <see cref="ResizingMode"/> values keyed by the string form of their name.
		/// </summary>
		static readonly Dictionary<string, ResizingMode> resizingModeByString = Enum
			.GetNames(typeof(ResizingMode))
			.ToDictionary(name => name, name => (ResizingMode)Enum.Parse(typeof(ResizingMode), name), StringComparer.OrdinalIgnoreCase)
			;

		internal static async Task HandleAsync(HttpContext context)
		{
			var queryString = context.Request.Query;
			if (!int.TryParse(queryString["w"], out var maxWidth))
				maxWidth = 100;
			if (!int.TryParse(queryString["h"], out var maxHeight))
				maxHeight = 100;
			var url = queryString["u"];
			if (url.Count == 0)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound; //Bad API call.
				return;
			}
			resizingModeByString.TryGetValue((string)queryString["m"] ?? "", out var mode);

			var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
			var path = context.Request.PathBase + context.Request.QueryString;

			if (cache.Get(path) is File resized)
			{
				await resized.WriteAsync(context);
				return;
			}

			var raw = cache.Get(url) as File;
			if (raw == null)
			{
				try
				{
					using (var client = new HttpClient())
					{
						client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; ServiceUI 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.79 Safari/537.36 Edge/14.14393");

						using (var response = await client.GetAsync(url))
						{
							if (!response.IsSuccessStatusCode)
							{
								context.Response.StatusCode = 404;
								return;
							}

							raw = new File(
								response.Content.Headers.ContentType.MediaType,
								await response.Content.ReadAsByteArrayAsync()
								);
						}
					}
				}
				catch (TaskCanceledException) //client.Timeout can trigger this.
				{
					//An improvement would be to send a timed-out image of the requested size and type.
					context.Response.ContentType = "text/plain";
					await context.Response.WriteAsync("Remote host did not respond in a timely manner.");
					return;
				}

				if (raw == null)
					return; //Some kind of problem with the source.
			}

			cache.Set(url, raw, DateTimeOffset.UtcNow.Add(ExpireAfter));

			if (raw.ContentType == null)
			{
				//An improvement would be to send an error image of the requested size and type.
				context.Response.StatusCode = (int)HttpStatusCode.NotFound; //The content type is unknown.
				return;
			}

			using (var stream = new MemoryStream(raw.Content))
			using (var image = new Bitmap(stream))
			{
				var sourceWidth = image.Width;
				var sourceHeight = image.Height;

				switch (mode)
				{
					default:
					case ResizingMode.Fit:
						{
							if (sourceWidth <= maxWidth && sourceHeight <= maxHeight)
							{
								//No processing needed, send original image.
								await raw.WriteAsync(context);
								return;
							}

							var downRatio = Math.Max((float)sourceWidth / maxWidth, (float)sourceHeight / maxHeight);
							var width = (int)Math.Max(1, sourceWidth / downRatio);
							var height = (int)Math.Max(1, sourceHeight / downRatio);

							var destRect = new Rectangle(0, 0, width, height);
							resized = Render(width, height, image, destRect);
						}
						break; //Fit (default)
					case ResizingMode.Fill:
						{
							var scale = Math.Max(maxWidth / (float)sourceWidth, maxHeight / (float)sourceHeight);
							var destY = (maxHeight - sourceHeight * scale) / 2;
							var destX = (maxWidth - sourceWidth * scale) / 2;
							var width = (int)Math.Round(sourceWidth * scale);
							var height = (int)Math.Round(sourceHeight * scale);

							var destRect = new Rectangle((int)Math.Round(destX), (int)Math.Round(destY), width, height);
							resized = Render(width + (int)Math.Round(2 * destX), height + (int)Math.Round(2 * destY), image, destRect);
						}
						break; //Fill
				}
			} //end using stream/image

			cache.Set(path, resized, DateTimeOffset.UtcNow.Add(ExpireAfter));
			await resized.WriteAsync(context);
		} //end create task

		static readonly ImageCodecInfo jpgEncoder = ImageCodecInfo.GetImageEncoders().First(encoder => encoder.FormatID == ImageFormat.Jpeg.Guid);

		static File Render(int width, int height, Bitmap image, Rectangle destRect)
		{
			using (var destImage = new Bitmap(width, height, image.PixelFormat))
			{
				destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

				using (var graphics = Graphics.FromImage(destImage))
				{
					graphics.CompositingMode = CompositingMode.SourceCopy;
					graphics.CompositingQuality = CompositingQuality.HighQuality;
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

					var sourceRect = new Rectangle(0, 0, image.Width, image.Height);

					graphics.DrawImage(image, destRect, sourceRect, GraphicsUnit.Pixel);
				}

				using (var resizedBytes = new MemoryStream())
				{
					if ((image.PixelFormat & PixelFormat.Alpha) != 0)
					{
						destImage.Save(resizedBytes, ImageFormat.Png);
						return new File("image/png", resizedBytes.ToArray());
					}

					using (var encoderParams = new EncoderParameters(1))
					using (var encoderParam = new EncoderParameter(Encoder.Quality, 95L))
					{
						encoderParams.Param[0] = encoderParam;
						destImage.Save(resizedBytes, jpgEncoder, encoderParams);
						return new File("image/jpeg", resizedBytes.ToArray());
					}
				}
			} //end using destImage
		}
	}
}
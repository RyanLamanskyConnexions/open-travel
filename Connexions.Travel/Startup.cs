using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Threading.Tasks;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Connexions.Travel.Tests")]

namespace Connexions.Travel
{
	/// <summary>
	/// ASP.NET Core startup processes.
	/// </summary>
	public class Startup
	{
		/// <summary>
		/// This method gets called by the runtime. Use this method to add services to the container. 
		/// </summary>
		/// <remarks>For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940 .</remarks>
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal)
				.AddResponseCompression()
				.AddMemoryCache()
				.AddSingleton<Configuration.IServiceResolver, Configuration.DemoServiceResolver>()
				.AddSingleton<ICapiClient, Capi.Client>()
				;
		}

		/// <summary>
		/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline. 
		/// </summary>
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole();

			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();

			app
				.UseResponseCompression()
				.MapWhen(HttpsRedirectCondition, a => a.Run(HttpsRedirectPermanent))
				.UseDefaultFiles()
				.UseStaticFiles()
				.Map("/Session", a => a.UseWebSockets().Use(Session.WebSocketHandlerAsync))
				.Map("/ImageProcessor/Resize", ImageProcessor.Resize.Configure)
				;
		}

		/// <summary>
		/// Determines whether the request should be redirected to HTTPS.
		/// </summary>
		/// <param name="context">Context for the request.</param>
		/// <returns>True if it should be redirected to HTTPS, otherwise false.</returns>
		private static bool HttpsRedirectCondition(Microsoft.AspNetCore.Http.HttpContext context)
			=> context.Request.IsHttps == false
			&& context.Request.Host.HasValue
			&& context.Request.Host.Port.HasValue == false //If a non-default port is used, the HTTPS port is unknown.
			&& context.Request.Host.Host.EndsWith(".azurewebsites.net")
			;

		/// <summary>
		/// Indicates that the desired resource has permanently moved to an HTTPS-based URL with with same host.
		/// </summary>
		/// <param name="context">Context for the request.</param>
		/// <returns><see cref="Task.CompletedTask"/>.</returns>
		private static Task HttpsRedirectPermanent(Microsoft.AspNetCore.Http.HttpContext context)
		{
			var path = "https://" + context.Request.Host.Value + context.Request.Path;
			context.Response.Redirect(path, true);
			return Task.CompletedTask;
		}
	}
}
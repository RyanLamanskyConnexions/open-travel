﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

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
				.UseDefaultFiles()
				.UseStaticFiles()
				.Map("/Session", a => a.UseWebSockets().Use(Session.WebSocketHandlerAsync))
				.Map("/ImageProcessor/Resize", ImageProcessor.Resize.Configure)
				;
		}
	}
}
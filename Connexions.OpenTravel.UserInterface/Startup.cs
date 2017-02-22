using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Connexions.OpenTravel.UserInterface
{
	/// <summary>
	/// ASP.NET Core startup processes.
	/// </summary>
	public class Startup
	{
		/// <summary>
		/// This method gets called by the runtime. Use this method to add services to the container. 
		/// </summary>
		/// <remarks>For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940</remarks>
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<ICapiClient, CapiClient>();
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
				.UseDefaultFiles()
				.UseStaticFiles()
				.Map("/Session", a => a.UseWebSockets().Use(Session.WebSocketHandlerAsync))
				;
		}
	}
}
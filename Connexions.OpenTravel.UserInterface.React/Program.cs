using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Connexions.OpenTravel.UserInterface.React
{
	static class Program
	{
		static void Main()
		{
			new WebHostBuilder()
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseIISIntegration()
				.UseStartup<Startup>()
				.UseApplicationInsights()
				.Build()
				.Run()
				;
		}
	}
}
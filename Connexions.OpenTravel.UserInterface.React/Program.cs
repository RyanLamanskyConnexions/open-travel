using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Connexions.Travel.React
{
	/// <summary>
	/// Contains the main entry point to the program.
	/// </summary>
	static class Program
	{
		/// <summary>
		/// The main entry point to the program.
		/// </summary>
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
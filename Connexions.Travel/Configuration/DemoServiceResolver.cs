namespace Connexions.Travel.Configuration
{
	/// <summary>
	/// Returns demo URLs and credentials to service requests.
	/// </summary>
	class DemoServiceResolver : IServiceResolver
	{
		private static readonly Service Hotel = new Service("https://public-be.stage.oski.io/", "Demo", "101");
		private static readonly Service Car = new Service("https://public-carbe.stage.oski.io/", "Demo", "200");

		Service IServiceResolver.GetServiceForRequest(string path)
		{
			switch (path.Substring(0, path.IndexOf('/')))
			{
				case "hotel": return Hotel;
				case "car": return Car;
			}

			return null;
		}
	}
}
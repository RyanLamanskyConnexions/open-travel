namespace Connexions.Travel.Configuration
{
	/// <summary>
	/// Acquires service information based on provided information.
	/// </summary>
	interface IServiceResolver
    {
		/// <summary>
		/// Acquires service information based on provided parameters.
		/// </summary>
		/// <param name="path">The path portion of the service end point.</param>
		/// <returns>Details about the service to accomodate <paramref name="path"/>.</returns>
		Service GetServiceForRequest(string path);
    }
}
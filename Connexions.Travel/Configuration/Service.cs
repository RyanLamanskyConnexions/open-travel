namespace Connexions.Travel.Configuration
{
	/// <summary>
	/// Contains configuration information for a particular service.
	/// </summary>
	class Service
	{
		/// <summary>
		/// Creates a new <see cref="Service"/> instance with the provided parameters.
		/// </summary>
		/// <param name="url">The base URL to the service.</param>
		/// <param name="tenantId">The tenant ID parameter.</param>
		/// <param name="posId">The POS ID parameter.</param>
		public Service(string url, string tenantId, string posId)
		{
			this.Url = url;
			this.TenantId = tenantId;
			this.PosId = posId;
		}

		/// <summary>
		/// The base URL to the service.
		/// </summary>
		public string Url { get; }

		/// <summary>
		/// The tenant ID parameter.
		/// </summary>
		public string TenantId { get; }

		/// <summary>
		/// The POS ID parameter.
		/// </summary>
		public string PosId { get; }
	}
}
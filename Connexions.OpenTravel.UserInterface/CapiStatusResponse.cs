namespace Connexions.Travel
{
	/// <summary>
	/// The common format shared by the various CAPI status calls.
	/// </summary>
	class CapiStatusResponse : CapiBaseResponse
	{
		/// <summary>
		/// A value of "Complete" indicates completion, otherwise the search is still underway.
		/// </summary>
		public string status;

		/// <summary>
		/// Describes a supplier whose processing has finished.
		/// </summary>
		public class CompletedSupplier
		{
			public string id;
			public string family;
			public string name;
		}

		/// <summary>
		/// The suppliers whose processing has finished.
		/// </summary>
		public CompletedSupplier[] completedSuppliers;
	}
}
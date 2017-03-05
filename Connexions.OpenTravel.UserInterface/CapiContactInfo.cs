namespace Connexions.OpenTravel.UserInterface
{
	class CapiContactInfo
	{
		public CapiPhone[] phones;

		public class BillingAddress
		{
			public string line1;
			public string line2;

			public class CodeName
			{
				public string code;
				public string name;
			}

			public CodeName city;
			public CodeName state;
			public string countryCode;
			public string postalCode;
		}

		public BillingAddress billingAddress;

		public string email;
	}
}
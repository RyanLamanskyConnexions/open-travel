namespace Connexions.Travel.Capi
{
	class ContactInfo
	{
		public Phone[] phones;

		public class Address
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

		public Address address;
		public Address billingAddress;

		public string email;
	}
}
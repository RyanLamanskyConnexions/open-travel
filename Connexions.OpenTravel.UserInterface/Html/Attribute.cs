namespace Connexions.OpenTravel.UserInterface.Html
{
	class Attribute : ISafe
	{
		public Attribute()
		{
		}

		public Attribute(string name)
		{
			Name = name;
		}

		public string Name;

		public bool IsUnsafe => true;

		public string Value;
	}
}
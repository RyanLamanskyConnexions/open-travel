namespace Connexions.Travel.Html
{
	class TextElement : Parsed
	{
		public TextElement()
		{
		}

		public TextElement(string content)
		{
			Content = content;
		}

		public string Content;

		public override bool IsUnsafe => false;

		public override string ToString() => Content;
	}
}
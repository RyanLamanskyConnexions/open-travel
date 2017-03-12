using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Connexions.Travel.Html
{
	class Element : Parsed
	{
		public Element()
		{
		}

		public Element(string name)
		{
			Name = name;
		}

		public Element(string name, IList<Parsed> children)
		{
			Name = name;
			Children = children;
			Attributes = new List<Attribute>();
		}

		/// <summary>
		/// Void elements can never have children per the HTML spec here: https://www.w3.org/TR/html-markup/syntax.html#syntax-elements
		/// </summary>
		private static readonly HashSet<string> voidElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"area",
			"base",
			"br",
			"col",
			"command",
			"embed",
			"hr",
			"img",
			"input",
			"keygen",
			"link",
			"meta",
			"param",
			"source",
			"track",
			"wbr",
		};

		public string Name;

		[JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
		public IList<Parsed> Children;

		public bool ShouldSerializeChildren() => Children?.Count > 0 && voidElements.Contains(Name) == false;

		public IList<Attribute> Attributes;

		public bool ShouldSerializeAttributes() => Attributes?.Count > 0;

		/// <summary>
		/// Element names not on this list have not been evaluated for script or style risks.
		/// </summary>
		private static readonly HashSet<string> whiteList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"a",
			"b",
			"br",
			"div",
			"h1",
			"h2",
			"h3",
			"h4",
			"h5",
			"h6",
			"hr",
			//"img", Not safe because it can be used to add cookies to the user's browser.
			"li",
			"ol",
			"p",
			//"script", Not safe because script can do anything.
			"span",
			"strong",
			//"style", Not safe because CSS has a variety of ways to impact the user.
			"table",
			"tbody",
			"td",
			"tfoot",
			"th",
			"thead",
			"tr",
			"ul",
		};

		public override bool IsUnsafe => whiteList.Contains(Name) == false;

		public override string ToString() => "<" + Name + " />";
	}
}
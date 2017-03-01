using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

namespace Connexions.OpenTravel.UserInterface.Html
{
	[TestClass]
	public class ParserTests
	{
		enum Safety
		{
			Safe = 0,
			Unsafe,
			Mixed
		}

		static Parsed[] Parse(string characters, int expectedLength, Safety safety = Safety.Safe)
		{
			var options = ParserOptions.None;
			if (safety != Safety.Safe)
				options |= ParserOptions.PreserveUnsafe;

			var result = Parser.Parse(characters, options);

			Assert.IsNotNull(result);

			var array = result.ToArray();
			if (characters?.Length != 0)
			{
				Assert.AreEqual(expectedLength, array.Length, "Parse of " + nameof(characters) + " does not match " + nameof(expectedLength) + " of " + expectedLength.ToString() + ".");

				foreach (var item in array)
				{
					Assert.IsNotNull(item);
					Assert.IsNotNull(item.ToString());

					switch (safety)
					{
						case Safety.Mixed:
						case Safety.Safe:
							Assert.IsFalse(item.IsUnsafe, $"Expected safety is {safety} but parsed item is unsafe.");
							break;
					}
				}
			}

			return array;
		}

		[TestMethod]
		public void Html_Parser_NullInput()
		{
			var result = Parser.Parse(null);
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.ToArray().Length);
		}

		[TestMethod]
		public void Html_Parser_EmptyString()
		{
			Parse("", 0);
		}

		[TestMethod]
		public void Html_Parser_PlainText()
		{
			const string input = "Test";
			var result = Parse(input, 1);
			Assert.IsInstanceOfType(result[0], typeof(TextElement));
			Assert.AreEqual(((TextElement)result[0]).Content, input);
		}

		[TestMethod]
		public void Html_Parser_RootEmptyElement()
		{
			var result = Parse("<br />", 1);
			Assert.IsInstanceOfType(result[0], typeof(Element));
			Assert.AreEqual(((Element)result[0]).Name, "br");
		}

		[TestMethod]
		public void Html_Parser_RootElementWithText()
		{
			var result = Parse("<b>Test</b>", 1);
			Assert.IsInstanceOfType(result[0], typeof(Element));
			Assert.AreEqual(((Element)result[0]).Name, "b");
		}

		[TestMethod]
		public void Html_Parser_TwoRootElements()
		{
			var result = Parse("<hr /><br />", 2);
			Assert.IsInstanceOfType(result[0], typeof(Element));
			Assert.IsInstanceOfType(result[1], typeof(Element));
			Assert.AreEqual(((Element)result[0]).Name, "hr");
			Assert.AreEqual(((Element)result[1]).Name, "br");
		}

		[TestMethod]
		public void Html_Parser_TwoRootElementsFirstWithText()
		{
			var result = Parse("<b>Test</b><br />", 2);
			Assert.IsInstanceOfType(result[0], typeof(Element));
			Assert.IsInstanceOfType(result[1], typeof(Element));
			Assert.AreEqual(((Element)result[0]).Name, "b");
			Assert.AreEqual(((Element)result[1]).Name, "br");
		}

		[TestMethod]
		public void Html_Parser_RootElementsAndRootText()
		{
			var result = Parse("Text<br />", 2);
			Assert.IsInstanceOfType(result[0], typeof(TextElement));
			Assert.IsInstanceOfType(result[1], typeof(Element));
			Assert.AreEqual(((TextElement)result[0]).Content, "Text");
			Assert.AreEqual(((Element)result[1]).Name, "br");
		}

		[TestMethod]
		public void Html_Parser_NestedElement()
		{
			var result = Parse(@"<h1><a></a></h1>", 1);
			Assert.IsInstanceOfType(result[0], typeof(Element));
			var element = (Element)result[0];
			Assert.AreEqual(element.Name, "h1");
			element = element.Children.FirstOrDefault() as Element;
			Assert.IsNotNull(element);
			Assert.AreEqual(element.Name, "a");
		}

		[TestMethod]
		public void Html_Parser_NestedElementWithAttribute()
		{
			var result = Parse(@"<h1><a href=""/""></a></h1>", 1);
			Assert.IsInstanceOfType(result[0], typeof(Element));
			var element = (Element)result[0];
			Assert.AreEqual(element.Name, "h1");
			element = element.Children.FirstOrDefault() as Element;
			Assert.IsNotNull(element);
			Assert.AreEqual(element.Name, "a");
			var attribute = element.Attributes.FirstOrDefault();
			Assert.IsNotNull(attribute);
			Assert.AreEqual("href", attribute.Name);
			Assert.AreEqual("/", attribute.Value);
		}

		[TestMethod]
		public void Html_Parser_DeeplyNestedElementsWithAttributes()
		{
			var result = Parse(@"<h1><a href=""/""><span class=""Travel"">Travel</span><span class=""Light"">Light</span></a></h1>", 1);
			Assert.IsInstanceOfType(result[0], typeof(Element));
			var element = (Element)result[0];
			Assert.AreEqual(element.Name, "h1");
			element = element.Children.FirstOrDefault() as Element;
			Assert.IsNotNull(element);
			Assert.AreEqual(element.Name, "a");
			var attribute = element.Attributes.FirstOrDefault();
			Assert.IsNotNull(attribute);
			Assert.AreEqual("href", attribute.Name);
			Assert.AreEqual("/", attribute.Value);
		}

		[TestMethod]
		public void Html_Parser_MinimizedAttribute()
		{
			var result = Parse(@"<input checked />", 1, Safety.Unsafe);
			Assert.IsInstanceOfType(result[0], typeof(Element));
			var element = (Element)result[0];
			Assert.AreEqual(element.Name, "input");
			var attribute = element.Attributes.FirstOrDefault();
			Assert.IsNotNull(attribute);
			Assert.AreEqual("checked", attribute.Name);
			Assert.IsNull(attribute.Value);
		}

		[TestMethod]
		public void Html_Parser_VoidElement()
		{
			var result = Parse(@"<br>", 1);
			Assert.IsInstanceOfType(result[0], typeof(Element));
			var element = (Element)result[0];
			Assert.AreEqual(element.Name, "br");
		}

		[TestMethod]
		public void Html_Parser_RealHotelDescription()
		{
			var result = Parse("<strong>1 bed or 2 beds</strong><br /> 215 sq feet (20 sq meters)<br /><br /><b>Entertainment</b> - Free WiFi and wired Internet access, 32-inch flat-screen TV <br /><b>Food & Drink</b> - Refrigerator, microwave, and coffee/tea maker<br /><b>Bathroom</b> - Private bathroom, shower/tub combination, free toiletries, and a hair dryer<br /><b>Practical</b> - Free local calls, safe, and iron/ironing board; free cribs/infant beds available on request<br /><b>Comfort</b> - Air conditioning and daily housekeeping<br /><b>Need to Know</b> - No rollaway/extra beds available<br />Connecting/adjoining rooms can be requested, subject to availability <br />", 25);
			Assert.IsInstanceOfType(result[0], typeof(Element));
			Assert.IsInstanceOfType(result[1], typeof(Element));
			Assert.AreEqual(((Element)result[0]).Name, "strong");
			Assert.AreEqual(((Element)result[1]).Name, "br");
		}

		[TestMethod]
		public void Html_Parser_SafeByDefault()
		{
			Assert.IsFalse(Parser.Parse("<script></script>").Any());
			Assert.IsFalse(Parser.Parse("<style></style>").Any());
			Assert.IsFalse(Parser.Parse("<img></img>").Any());

			var result = Parse("<p onclick=\"alert()\" />", 1);
			Assert.IsInstanceOfType(result[0], typeof(Element));
			var element = (Element)result[0];
			Assert.AreEqual(element.Name, "p");
			Assert.IsFalse(element.Attributes.Any());
		}

		/// <summary>
		/// Basic sanity test that JSON serialization works.
		/// </summary>
		[TestMethod]
		public void Html_Parser_JsonSerialization()
		{
			var serialized = JsonConvert.SerializeObject(Parse("<a href=\"Url\"><br />Text</a>", 1));
			Assert.IsNotNull(serialized);
		}
	}
}
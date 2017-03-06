using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connexions.Travel.Html
{
	[Flags]
	enum ParserOptions
	{
		/// <summary>
		/// Default options.
		/// </summary>
		None = 0,
		/// <summary>
		/// Elements and attributes where <see cref="ISafe.IsUnsafe"/> is true are included.  This can expose browser-based consumers to a variety of attacks.
		/// </summary>
		PreserveUnsafe = 1 << 1,
	}

	/// <summary>
	/// A very simple HTML parser that converts raw HTML into a series of objects.  Designed to try to make sense of malformed markup, like a real browser would.
	/// </summary>
	static class Parser
	{
		enum State
		{
			Text,
			LessThan,
			ElementName,
			PreAttribute,
			SelfClosingElement,
			ElementClosing,
			AttributeName,
			AttributeEquals,
			AttributeOpenQuote,
			AttributeUnquoted,
		}

		public static IEnumerable<Parsed> Parse(IEnumerable<char> characters)
		{
			return Parse(characters, ParserOptions.None);
		}

		public static IEnumerable<Parsed> Parse(IEnumerable<char> characters, ParserOptions options)
		{
			var enumerable = ParseUnsafe(characters);

			if ((options & ParserOptions.PreserveUnsafe) == 0)
			{
				enumerable = enumerable
					.Where(parsed => parsed.IsUnsafe == false)
					.Select(parsed =>
					{
						var element = parsed as Element;
						if (element == null)
							return parsed;

						if (element.Attributes != null && element.Attributes.Any(att => att.IsUnsafe))
							element.Attributes = element.Attributes.Where(att => att.IsUnsafe == false).ToArray();

						return element;
					});
			}

			return enumerable;
		}

		static IEnumerable<Parsed> ParseUnsafe(IEnumerable<char> characters)
		{
			if (characters == null)
				yield break;

			var state = State.Text;
			var text = new StringBuilder();
			var parents = new Stack<Element>();
			Element parent, child;
			Attribute att = null;

			foreach (var character in characters)
			{
				switch (state)
				{
					case State.Text:
						switch (character)
						{
							case '<':
								state = State.LessThan;
								continue;
							default:
								text.Append(character);
								continue;
						}
					case State.LessThan:
						if (parents.Count != 0 && character == '/')
						{
							((List<Parsed>)parents.Peek().Children).Add(new TextElement(text.ToString()));
							text.Clear();
							state = State.ElementClosing;
							continue;
						}

						if (char.IsLetter(character) == false)
						{
							//Malformed markup.
							text
								.Append('<')
								.Append(character)
								;
							state = State.Text;
							continue;
						}

						if (text.Length != 0)
						{
							var textElement = new TextElement(text.ToString());
							if (parents.Count != 0)
								((List<Parsed>)parents.Peek().Children).Add(textElement);
							else
								yield return textElement;
							text.Clear();
						}

						text.Append(character);
						state = State.ElementName;
						continue;
					case State.ElementName:
						switch (character)
						{
							case '\t':
							case '\r':
							case '\n':
							case ' ':
								child = new Element(text.ToString(), new List<Parsed>());
								if (parents.Count != 0)
									((List<Parsed>)parents.Peek().Children).Add(child);
								parents.Push(child);
								text.Clear();
								state = State.PreAttribute;
								continue;
							case '>':
								child = new Element(text.ToString(), new List<Parsed>());
								if (parents.Count != 0)
									((List<Parsed>)parents.Peek().Children).Add(child);
								parents.Push(child);
								text.Clear();
								state = State.Text;
								continue;
							case '/':
								state = State.SelfClosingElement;
								continue;
							default:
								text.Append(character);
								continue;
						}
					case State.PreAttribute:
						switch (character)
						{
							case '>':
								state = State.Text;
								continue;
							case '\t':
							case '\r':
							case '\n':
							case ' ':
								continue;
							case '/':
								state = State.SelfClosingElement;
								continue;
						}

						text.Append(character);
						state = State.AttributeName;
						continue;
					case State.SelfClosingElement:
						if (character != '>')
						{
							//Malformed markup.
							text.Append(character);
						}

						if (parents.Count >= 1)
						{
							parent = parents.Pop();
							if (parents.Count == 0)
								yield return parent;
						}
						state = State.Text;
						continue;
					case State.ElementClosing:
						if (character != '>')
							continue; //Malformed markup.

						if (parents.Count >= 1)
						{
							parent = parents.Pop();
							if (parents.Count == 0)
								yield return parent;
						}
						state = State.Text;
						continue;
					case State.AttributeName:
						switch (character)
						{
							case '=':
								att = new Attribute(text.ToString());
								text.Clear();
								state = State.AttributeEquals;
								continue;
							case '\t':
							case '\r':
							case '\n':
							case ' ':
								((List<Attribute>)parents.Peek().Attributes).Add(new Attribute(text.ToString()));
								text.Clear();
								state = State.PreAttribute;
								continue;
							case '/':
								((List<Attribute>)parents.Peek().Attributes).Add(new Attribute(text.ToString()));
								text.Clear();
								state = State.SelfClosingElement;
								continue;
							case '>':
								((List<Attribute>)parents.Peek().Attributes).Add(new Attribute(text.ToString()));
								text.Clear();
								state = State.Text;
								continue;
						}

						text.Append(character);
						continue;
					case State.AttributeEquals:
						if (character != '"')
						{
							text.Append(character);
							state = State.AttributeUnquoted;
							continue;
						}

						state = State.AttributeOpenQuote;
						continue;
					case State.AttributeOpenQuote:
						if (character != '"')
						{
							text.Append(character);
							continue;
						}

						att.Value = text.ToString();
						text.Clear();
						((List<Attribute>)parents.Peek().Attributes).Add(att);
						att = null;

						state = State.PreAttribute;
						continue;
				}
			}

			if (parents.Count != 0)
			{
				if (text.Length != 0)
					((List<Parsed>)parents.Peek().Children).Add(new TextElement(text.ToString()));

				while (parents.Count != 1)
					parents.Pop();

				yield return parents.Pop();
			}
			else if (text.Length != 0)
				yield return new TextElement(text.ToString());
		}
	}
}
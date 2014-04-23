/*
 * Copyright 2010, 2011, 2012 mapsforge.org
 *
 * This program is free software: you can redistribute it and/or modify it under the
 * terms of the GNU Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY
 * WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
 * PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with
 * this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System.IO;
using System.Collections.Generic;
using System;
using System.Xml.Linq;
using System.Xml;
using RenderTheme.RenderInstructions;
namespace RenderTheme.Rule
{
	public class RenderThemeHandler
	{
		private enum Element
		{
			RenderTheme,
			RenderingInstruction,
			Rule
		}

		private const string ElementNameRule = "rule";
		private const string UnexpectedElement = "unexpected element: ";

		public static Theme GetRenderTheme ( XmlRenderTheme xmlRenderTheme )
		{
			RenderThemeHandler renderThemeHandler = new RenderThemeHandler(xmlRenderTheme.RelativePathPrefix);

			using(Stream stream = xmlRenderTheme.OpenStream()) {
				XDocument xmlReader = XDocument.Load(stream);
				renderThemeHandler.ScanElementsTree(xmlReader.Root);
				return renderThemeHandler.renderTheme;
			} 
		}

		private void ScanElementsTree ( XElement element )
		{

			StartElement(
				element.Name.NamespaceName,
				element.Name.LocalName,
				element.Name.LocalName,
				element.Attributes());

			foreach (var subElement in element.Elements()) {
				ScanElementsTree(subElement);
			}

			EndElement(
				element.Name.NamespaceName,
				element.Name.LocalName,
				element.Name.LocalName
				);

			EndDocument();
		}

		private Rule currentRule;
		private readonly Stack<Element> elementStack = new Stack<Element>();
		private int level;
		private readonly string relativePathPrefix;
		private Theme renderTheme;
		private readonly Stack<Rule> ruleStack = new Stack<Rule>();

		private RenderThemeHandler ( string relativePathPrefix )
		{
			this.relativePathPrefix = relativePathPrefix;
		}

		public void EndDocument ()
		{
			if (this.renderTheme == null) {
				throw new NullReferenceException("missing element: renderTheme");
			}

			this.renderTheme.Levels = this.level;
			this.renderTheme.Complete();
		}

		public void EndElement ( string uri, string localName, string qName )
		{
			this.elementStack.Pop();

			if (ElementNameRule.Equals(qName)) {
				this.ruleStack.Pop();
				if (this.ruleStack.Count == 0) {
					this.renderTheme.AddRule(this.currentRule);
				} else {
					this.currentRule = this.ruleStack.Peek();
				}
			}
		}

		public void Error ( XmlException exception )
		{
			System.Diagnostics.Debug.WriteLine(exception);
		}

		public void StartElement ( string uri, string localName, string qName, IEnumerable<XAttribute> attributes )
		{
			try {
				if ("rendertheme".Equals(qName)) {
					CheckState(qName, Element.RenderTheme);
					this.renderTheme = new RenderThemeBuilder(qName, attributes).build();
				} else if (ElementNameRule.Equals(qName)) {
					CheckState(qName, Element.Rule);
					Rule rule = new RuleBuilder(qName, attributes, this.ruleStack).build();
					if (this.ruleStack.Count != 0) {
						this.currentRule.AddSubRule(rule);
					}
					this.currentRule = rule;
					this.ruleStack.Push(this.currentRule);
				} else if ("area".Equals(qName)) {
					CheckState(qName, Element.RenderingInstruction);
					Area area = new AreaBuilder(qName, attributes, this.level++, this.relativePathPrefix).build();
					this.currentRule.AddRenderingInstruction(area);
				} else if ("caption".Equals(qName)) {
					CheckState(qName, Element.RenderingInstruction);
					Caption caption = new CaptionBuilder(qName, attributes).build();
					this.currentRule.AddRenderingInstruction(caption);
				} else if ("circle".Equals(qName)) {
					CheckState(qName, Element.RenderingInstruction);
					Circle circle = new CircleBuilder(qName, attributes, this.level++).build();
					this.currentRule.AddRenderingInstruction(circle);
				} else if ("line".Equals(qName)) {
					CheckState(qName, Element.RenderingInstruction);
					Line line = new LineBuilder(qName, attributes, this.level++, this.relativePathPrefix).build();
					this.currentRule.AddRenderingInstruction(line);
				} else if ("lineSymbol".Equals(qName)) {
					CheckState(qName, Element.RenderingInstruction);
					LineSymbol lineSymbol = new LineSymbolBuilder(qName, attributes,
							this.relativePathPrefix).build();
					this.currentRule.AddRenderingInstruction(lineSymbol);
				} else if ("pathText".Equals(qName)) {
					CheckState(qName, Element.RenderingInstruction);
					PathText pathText = new PathTextBuilder(qName, attributes).build();
					this.currentRule.AddRenderingInstruction(pathText);
				} else if ("symbol".Equals(qName)) {
					CheckState(qName, Element.RenderingInstruction);
					Symbol symbol = new SymbolBuilder(qName, attributes, this.relativePathPrefix).build();
					this.currentRule.AddRenderingInstruction(symbol);
				} else {
					throw new XmlException("unknown element: " + qName);
				}
			} catch (IOException e) {
				throw new XmlException(null, e);
			}
		}

		public void Warning ( XmlException exception )
		{
			System.Diagnostics.Debug.WriteLine(exception);
		}

		private void CheckElement ( string elementName, Element element )
		{
			switch (element) {
				case Element.RenderTheme:
					if (this.elementStack.Count != 0) {
						throw new XmlException(UnexpectedElement + elementName);
					}
					return;

				case Element.Rule:
					Element parentElement = this.elementStack.Peek();
					if (parentElement != Element.RenderTheme && parentElement != Element.Rule) {
						throw new XmlException(UnexpectedElement + elementName);
					}
					return;

				case Element.RenderingInstruction:
					if (this.elementStack.Peek() != Element.Rule) {
						throw new XmlException(UnexpectedElement + elementName);
					}
					return;
			}

			throw new XmlException("unknown enum value: " + element);
		}

		private void CheckState ( string elementName, Element element )
		{
			CheckElement(elementName, element);
			this.elementStack.Push(element);
		}
	}
}
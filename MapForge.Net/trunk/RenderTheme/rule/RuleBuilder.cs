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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
namespace RenderTheme.Rule
{

	/**
	 * A builder for {@link Rule} instances.
	 */
	public class RuleBuilder
	{
		private const string ClosedKey = "closed";
		private const string ElementKey = "e";
		private const string KeysKey = "k";
		private const string ValuesKey = "v";
		private static readonly char[] SplitPattern = { '|' };
		private const string ZoomMaxKey = "zoom-max";
		private const string ZoomMinKey = "zoom-min";

		private static readonly Func<RuleBuilder, IRuleTest>[] RuleTestBuilders = {
			ZoomTest.Build,
			KeyValueTest.Build,
			KeyNoValueTest.Build ,
			ClosedWayTest.Build,
			ClassTester.Build
		};

		public List<IRuleTest> RuleTests { get; set; }

		public string Closed { get; private set; }
		public string ElementClass { get; private set; }
		public string Keys { get; private set; }
		public string Values { get; private set; }
		private readonly Stack<Rule> ruleStack;


		public string[] keyList;
		public string[] valueList;
		public byte zoomMax;
		public byte zoomMin;

		public RuleBuilder ( string elementName, IEnumerable<XAttribute> attributes, Stack<Rule> ruleStack )
		{
			this.ruleStack = ruleStack;

			this.Closed = null;
			this.zoomMin = 0;
			this.zoomMax = Byte.MaxValue;

			extractValues(elementName, attributes);
		}

		/**
		 * @return a new {@code Rule} instance.
		 */
		public Rule build ()
		{
			return new Rule(this);
		}

		private void extractValues ( string elementName, IEnumerable<XAttribute> attributes )
		{
			foreach (XAttribute attribute in attributes) {
				string name = attribute.Name.LocalName;
				string value = attribute.Value;

				if (ElementKey.Equals(name)) {
					this.ElementClass = value.ToLower();
				} else if (KeysKey.Equals(name)) {
					this.Keys = value;
				} else if (ValuesKey.Equals(name)) {
					this.Values = value;
				} else if (ClosedKey.Equals(name)) {
					this.Closed = value;
				} else if (ZoomMinKey.Equals(name)) {
					this.zoomMin = XmlUtils.parseNonNegativeByte(name, value);
				} else if (ZoomMaxKey.Equals(name)) {
					this.zoomMax = XmlUtils.parseNonNegativeByte(name, value);
				} else {
					throw XmlUtils.createSAXException(elementName, name, value);
				}
			}

			validate(elementName);

			this.keyList = this.Keys.Split(SplitPattern);
			this.valueList = this.Values.Split(SplitPattern);

			RuleTests = new List<IRuleTest>();
			foreach (var builder in RuleTestBuilders) {
				//tente de créer un test
				var ruleTest = builder(this);
				//si aucun test n'a été créé, passe au suivant
				if (ruleTest == null) continue;

				//vérifie si le test n'est pas couvert par un test d'une rêgle englobante
				bool isCovered = false;
				foreach (var rule in ruleStack) {
					if (rule.RuleTests.Any(ruleTest.IsCovered)) {
						isCovered = true;
					}
					if (isCovered) break;
				}
				//si oui, n'ajoute pas le test
				if (isCovered) continue;
				//si non, ajoute le test
				RuleTests.Add(ruleTest);
			}
		}

		private void validate ( string elementName )
		{
			XmlUtils.checkMandatoryAttribute(elementName, ElementKey, this.ElementClass);
			XmlUtils.checkMandatoryAttribute(elementName, KeysKey, this.Keys);
			XmlUtils.checkMandatoryAttribute(elementName, ValuesKey, this.Values);

			if (this.zoomMin > this.zoomMax) {
				throw new XmlException('\'' + ZoomMinKey + "' > '" + ZoomMaxKey + "': " + this.zoomMin + ' ' + this.zoomMax);
			}
		}


	}
}
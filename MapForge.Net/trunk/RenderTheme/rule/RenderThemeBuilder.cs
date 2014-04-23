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
using System.Windows.Media;
using System.Collections.Generic;
using System.Xml.Linq;
using MapCore;
using System.Xml;
namespace RenderTheme.Rule
{

	/**
	 * A builder for {@link RenderTheme} instances.
	 */
	public class RenderThemeBuilder
	{
		private const string BaseStrokeWidth = "base-stroke-width";
		private const string BaseTextSize = "base-text-size";
		private const string MapBackground = "map-background";
		private const int RenderThemeVersion = 2;
		private const string Version = "version";
		private const string Xmlns = "xmlns";
		private const string XmlnsXsi = "xsi";
		private const string XsiSchemalocation = "schemaLocation";

		private int version;
		public float baseStrokeWidth;
		public float baseTextSize;
		public Color mapBackground;

		public RenderThemeBuilder ( string elementName, IEnumerable<XAttribute> attributes )
		{
			this.baseStrokeWidth = 1;
			this.baseTextSize = 1;
			this.mapBackground = Colors.White;

			extractValues(elementName, attributes);
		}

		/**
		 * @return a new {@code RenderTheme} instance.
		 */
		public Theme build ()
		{
			return new Theme(this);
		}

		private void extractValues ( string elementName, IEnumerable<XAttribute> attributes )
		{
			foreach (XAttribute attribute in attributes) {
				string name = attribute.Name.LocalName;
				string value = attribute.Value;
				string ns = attribute.Name.Namespace.NamespaceName;

				if (ns == "http://www.w3.org/2000/xmlns/") {
					continue;
				} else if (Xmlns.Equals(name)) {
					continue;
				} else if (XmlnsXsi.Equals(name)) {
					continue;
				} else if (XsiSchemalocation.Equals(name)) {
					continue;
				} else if (Version.Equals(name)) {
					this.version = XmlUtils.parseNonNegativeInteger(name, value);
				} else if (MapBackground.Equals(name)) {
					this.mapBackground = Utils.ParseColor(value); ;
				} else if (BaseStrokeWidth.Equals(name)) {
					this.baseStrokeWidth = XmlUtils.parseNonNegativeFloat(name, value);
				} else if (BaseTextSize.Equals(name)) {
					this.baseTextSize = XmlUtils.parseNonNegativeFloat(name, value);
				} else {
					throw XmlUtils.createSAXException(elementName, name, value);
				}
			}

			validate(elementName);
		}

		private void validate ( string elementName )
		{
			XmlUtils.checkMandatoryAttribute(elementName, Version, this.version);

			if (this.version != RenderThemeVersion) {
				throw new XmlException("unsupported render theme version: " + this.version);
			}
		}
	}
}
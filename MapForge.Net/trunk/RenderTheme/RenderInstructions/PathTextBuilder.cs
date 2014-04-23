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
using RenderTheme.Graphics;
using System.Windows.Media;
using System.Collections.Generic;
using MapCore;
using System;
using System.Xml.Linq;
namespace RenderTheme.RenderInstructions
{

	/**
	 * A builder for {@link PathText} instances.
	 */
	public class PathTextBuilder
	{
		static string FILL = "fill";
		static string FONT_FAMILY = "font-family";
		static string FONT_SIZE = "font-size";
		static string FONT_STYLE = "font-style";
		static string K = "k";
		static string STROKE = "stroke";
		static string STROKE_WIDTH = "stroke-width";

		public Brush fill;
		public float fontSize;
		public Pen stroke;
		public TextKey textKey;

		public PathTextBuilder ( string elementName, IEnumerable<XAttribute> attributes )
		{

			this.fill = Brushes.Black;

			this.stroke = new Pen(Brushes.Black, 1);

			extractValues(elementName, attributes);
		}

		/**
		 * @return a new {@code PathText} instance.
		 */
		public PathText build ()
		{
			return new PathText(this);
		}

		private void extractValues ( string elementName, IEnumerable<XAttribute> attributes )
		{
			//RenderTheme.Graphics.FontFamily fontFamily = RenderTheme.Graphics.FontFamily.DEFAULT;
			//FontStyle fontStyle = FontStyle.NORMAL;

			foreach (XAttribute attribute in attributes) {
				string name = attribute.Name.LocalName;
				string value = attribute.Value;

				if (K.Equals(name)) {
					this.textKey = TextKey.getInstance(value);
				} else if (FONT_FAMILY.Equals(name)) {
					//fontFamily = RenderTheme.Graphics.FontFamily.Parse(value);
				} else if (FONT_STYLE.Equals(name)) {
					//fontStyle = (FontStyle)Enum.Parse(typeof(FontStyle), value, true);
				} else if (FONT_SIZE.Equals(name)) {
					this.fontSize = XmlUtils.parseNonNegativeFloat(name, value);
				} else if (FILL.Equals(name)) {
					this.fill = new SolidColorBrush(Utils.ParseColor(value));
				} else if (STROKE.Equals(name)) {
					this.stroke.Brush = new SolidColorBrush(Utils.ParseColor(value));
				} else if (STROKE_WIDTH.Equals(name)) {
					this.stroke.Thickness = XmlUtils.parseNonNegativeFloat(name, value);
				} else {
					throw XmlUtils.createSAXException(elementName, name, value);
				}
			}

			//this.fill.FontFamily = fontFamily;
			//this.fill.FontStyle = fontStyle;
			//this.stroke.FontFamily = fontFamily;
			//this.stroke.FontStyle = fontStyle;

			XmlUtils.checkMandatoryAttribute(elementName, K, this.textKey);
		}
	}
}
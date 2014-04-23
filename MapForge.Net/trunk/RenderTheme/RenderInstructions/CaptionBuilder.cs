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
using System.Collections.Generic;
using System.Xml.Linq;
using System.Windows.Media;
using System;
using MapCore;
using System.Windows;
namespace RenderTheme.RenderInstructions
{

	/**
	 * A builder for {@link Caption} instances.
	 */
	public class CaptionBuilder
	{
		static string DY = "dy";
		static string FILL = "fill";
		static string FONT_FAMILY = "font-family";
		static string FONT_SIZE = "font-size";
		static string FONT_STYLE = "font-style";
		static string K = "k";
		static string STROKE = "stroke";
		static string STROKE_WIDTH = "stroke-width";

		public float dy;
		public Brush fill;
		public float fontSize;
		public HorizontalAlignment horizontalAlignment;
		public VerticalAlignment verticalAlignment;
		public Typeface typeface;
		public Pen stroke;
		public TextKey textKey;

		public CaptionBuilder ( string elementName, IEnumerable<XAttribute> attributes )
		{
			this.fill = Brushes.Black;
			this.stroke = new Pen(Brushes.Black, 1);
			horizontalAlignment = HorizontalAlignment.Left;
			verticalAlignment = VerticalAlignment.Bottom;
			typeface = RenderTheme.Graphics.FontFamily.DEFAULT;

			extractValues(elementName, attributes);
		}

		/**
		 * @return a new {@code Caption} instance.
		 */
		public Caption build ()
		{
			return new Caption(this);
		}

		private void extractValues ( string elementName, IEnumerable<XAttribute> attributes )
		{
			foreach (XAttribute attribute in attributes) {
				string name = attribute.Name.LocalName;
				string value = attribute.Value;
				if (K.Equals(name)) {
					this.textKey = TextKey.getInstance(value);
				} else if (DY.Equals(name)) {
					this.dy = float.Parse(value);
				} else if (FONT_FAMILY.Equals(name)) {
					typeface = RenderTheme.Graphics.FontFamily.Parse(value);
				} else if (FONT_STYLE.Equals(name)) {
					//typeface. fontStyle = (FontStyle)Enum.Parse(typeof(FontStyle), value, true);
				} else if (FONT_SIZE.Equals(name)) {
					this.fontSize = XmlUtils.parseNonNegativeFloat(name, value);
				} else if (FILL.Equals(name)) {
					this.fill =  new SolidColorBrush(Utils.ParseColor(value));
				} else if (STROKE.Equals(name)) {
					this.stroke.Brush = new SolidColorBrush(Utils.ParseColor(value));
				} else if (STROKE_WIDTH.Equals(name)) {
					this.stroke.Thickness = XmlUtils.parseNonNegativeFloat(name, value);
				} else {
					throw XmlUtils.createSAXException(elementName, name, value);
				}
			}

			XmlUtils.checkMandatoryAttribute(elementName, K, this.textKey);
		}
	}
}
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
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using MapCore;
namespace RenderTheme.RenderInstructions
{

	/**
	 * A builder for {@link Area} instances.
	 */
	public class AreaBuilder
	{
		static string FILL = "fill";
		static string SRC = "src";
		static string STROKE = "stroke";
		static string STROKE_WIDTH = "stroke-width";

		public Brush fill;
		public int level;
		public Pen stroke;
		public float strokeWidth;

		public AreaBuilder ( string elementName, IEnumerable<XAttribute> attributes, int level,
				string relativePathPrefix )
		{
			this.level = level;

			this.fill = Brushes.Black;

			this.stroke = new Pen();
			this.stroke.Brush = Brushes.Transparent;
			this.stroke.DashStyle = DashStyles.Solid;
			this.stroke.StartLineCap = PenLineCap.Round;
			this.stroke.EndLineCap = PenLineCap.Round;
			this.stroke.DashCap = PenLineCap.Round;

			extractValues(elementName, attributes, relativePathPrefix);
		}

		/**
		 * @return a new {@code Area} instance.
		 */
		public Area build ()
		{
			return new Area(this);
		}

		private void extractValues ( string elementName, IEnumerable<XAttribute> attributes,
				string relativePathPrefix )
		{
			foreach (XAttribute attribute in attributes) {
				string name = attribute.Name.LocalName;
				string value = attribute.Value;

				if (SRC.Equals(name)) {
					this.fill = new ImageBrush(XmlUtils.createBitmap(relativePathPrefix, value));
				} else if (FILL.Equals(name)) {
					this.fill = new SolidColorBrush(Utils.ParseColor(value));
				} else if (STROKE.Equals(name)) {
					this.stroke.Brush = new SolidColorBrush(Utils.ParseColor(value));
				} else if (STROKE_WIDTH.Equals(name)) {
					this.strokeWidth = XmlUtils.parseNonNegativeFloat(name, value);
				} else {
					throw XmlUtils.createSAXException(elementName, name, value);
				}
			}
		}
	}
}

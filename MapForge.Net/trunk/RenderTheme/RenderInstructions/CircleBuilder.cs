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
namespace RenderTheme.RenderInstructions
{

	/**
	 * A builder for {@link Circle} instances.
	 */
	public class CircleBuilder
	{
		static string FILL = "fill";
		static string RADIUS = "radius";
		static string SCALE_RADIUS = "scale-radius";
		static string STROKE = "stroke";
		static string STROKE_WIDTH = "stroke-width";

		public Brush fill;
		public int level;
		public float radius;
		public bool scaleRadius;
		public Pen stroke;
		public float strokeWidth;

		public CircleBuilder ( string elementName, IEnumerable<XAttribute> attributes, int level )
		{
			this.level = level;

			this.fill = Brushes.Transparent;

			this.stroke = new Pen(Brushes.Black, 1);

			extractValues(elementName, attributes);
		}

		/**
		 * @return a new {@code Circle} instance.
		 */
		public Circle build ()
		{
			return new Circle(this);
		}

		private void extractValues ( string elementName, IEnumerable<XAttribute> attributes )
		{
			foreach (XAttribute attribute in attributes) {
				string name = attribute.Name.LocalName;
				string value = attribute.Value;

				if (RADIUS.Equals(name)) {
					this.radius = XmlUtils.parseNonNegativeFloat(name, value);
				} else if (SCALE_RADIUS.Equals(name)) {
					this.scaleRadius = bool.Parse(value);
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

			XmlUtils.checkMandatoryAttribute(elementName, RADIUS, this.radius);
		}
	}
}
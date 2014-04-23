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
	 * A builder for {@link Line} instances.
	 */
	public class LineBuilder
	{
		static string SRC = "src";
		static string STROKE = "stroke";
		static string STROKE_DASHARRAY = "stroke-dasharray";
		static string STROKE_LINECAP = "stroke-linecap";
		static string STROKE_WIDTH = "stroke-width";

		private static float[] parseFloatArray ( string name, string dashString )
		{
			string[] dashEntries = dashString.Split(',');
			float[] dashIntervals = new float[dashEntries.Length];
			for (int i = 0; i < dashEntries.Length; ++i) {
				dashIntervals[i] = XmlUtils.parseNonNegativeFloat(name, dashEntries[i]);
			}
			return dashIntervals;
		}

		public int level;
		public Pen stroke;
		public float strokeWidth;

		public LineBuilder ( string elementName, IEnumerable<XAttribute> attributes, int level,
				string relativePathPrefix )
		{
			this.level = level;

			this.stroke = new Pen();
			this.stroke.Brush = Brushes.Black;
			this.stroke.DashStyle = DashStyles.Solid;
			this.stroke.StartLineCap = PenLineCap.Round;
			this.stroke.EndLineCap = PenLineCap.Round;
			this.stroke.DashCap = PenLineCap.Round;

			extractValues(elementName, attributes, relativePathPrefix);
		}

		/**
		 * @return a new {@code Line} instance.
		 */
		public Line build ()
		{
			return new Line(this);
		}

		private void extractValues ( string elementName, IEnumerable<XAttribute> attributes,
				string relativePathPrefix )
		{
			foreach (XAttribute attribute in attributes) {
				string name = attribute.Name.LocalName;
				string value = attribute.Value;

				if (SRC.Equals(name)) {
					//this.stroke.BitmapShader = XmlUtils.createBitmap(relativePathPrefix, value);
				} else if (STROKE.Equals(name)) {
					this.stroke.Brush = new SolidColorBrush(Utils.ParseColor(value));
				} else if (STROKE_WIDTH.Equals(name)) {
					this.strokeWidth = XmlUtils.parseNonNegativeFloat(name, value);
				} else if (STROKE_DASHARRAY.Equals(name)) {
					//this.stroke.DashPathEffect = parseFloatArray(name, value);
				} else if (STROKE_LINECAP.Equals(name)) {
					if (string.Compare(value, "BUTT",StringComparison.InvariantCultureIgnoreCase) == 0) value = "Flat";
					var cap = (PenLineCap)Enum.Parse(typeof(PenLineCap), value, true);
					this.stroke.StartLineCap = cap;
					this.stroke.EndLineCap = cap;
					this.stroke.DashCap = cap;
				} else {
					throw XmlUtils.createSAXException(elementName, name, value);
				}
			}
		}
	}
}
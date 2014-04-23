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
using System.Windows;
using RenderTheme.Graphics;
namespace MapsUtilities.MapGenerator.DatabaseRenderer
{

	public class pointTextContainer
	{
		public Rect boundary;
		public Paint paintBack;
		public Paint paintFront;
		public SymbolContainer symbol;
		public string text;
		public double x;
		public double y;

		/**
		 * Create a new Point container, that holds the x-y coordinates of a point, a text variable and one paint objects.
		 * 
		 * @param text
		 *            the text of the point.
		 * @param x
		 *            the x coordinate of the point.
		 * @param y
		 *            the y coordinate of the point.
		 * @param paintFront
		 *            the paintFront for the point.
		 */
		public PointTextContainer ( string text, double x, double y, Paint paintFront )
		{
			this.text = text;
			this.x = x;
			this.y = y;
			this.paintFront = paintFront;
			this.paintBack = null;

			this.boundary = new Rect(0, 0, paintFront.getTextWidth(text), paintFront.getTextHeight(text));
		}

		/**
		 * Create a new Point container, that holds the x-y coordinates of a point, a text variable and two paint objects.
		 * 
		 * @param text
		 *            the text of the point.
		 * @param x
		 *            the x coordinate of the point.
		 * @param y
		 *            the y coordinate of the point.
		 * @param paintFront
		 *            the paintFront for the point.
		 * @param paintBack
		 *            the paintBack for the point.
		 */
		public PointTextContainer ( string text, double x, double y, Paint paintFront, Paint paintBack )
		{
			this.text = text;
			this.x = x;
			this.y = y;
			this.paintFront = paintFront;
			this.paintBack = paintBack;

			if (paintBack != null) {
				paintBack.getTextHeight(text);
				paintBack.getTextWidth(text);
				this.boundary = new Rect(0, 0, paintBack.getTextWidth(text), paintBack.getTextHeight(text));
			} else {
				this.boundary = new Rect(0, 0, paintFront.getTextWidth(text), paintFront.getTextHeight(text));
			}
		}

		/**
		 * Create a new Point container, that holds the x-y coordinates of a point, a text variable, two paint objects, and
		 * a reference on a symbol, if the text is connected with a NODE.
		 * 
		 * @param text
		 *            the text of the point.
		 * @param x
		 *            the x coordinate of the point.
		 * @param y
		 *            the y coordinate of the point.
		 * @param paintFront
		 *            the paintFront for the point.
		 * @param paintBack
		 *            the paintBack for the point.
		 * @param symbol
		 *            the connected Symbol.
		 */
		public PointTextContainer ( string text, double x, double y, Paint paintFront, Paint paintBack, SymbolContainer symbol )
		{
			this.text = text;
			this.x = x;
			this.y = y;
			this.paintFront = paintFront;
			this.paintBack = paintBack;
			this.symbol = symbol;

			if (paintBack != null) {
				this.boundary = new Rect(0, 0, paintBack.getTextWidth(text), paintBack.getTextHeight(text));
			} else {
				this.boundary = new Rect(0, 0, paintFront.getTextWidth(text), paintFront.getTextHeight(text));
			}
		}
	}
}
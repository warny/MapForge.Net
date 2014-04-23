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
using MapCore.Util;
using System;
using MapCore.Model;
using RenderTheme.Graphics;
using System.Windows.Controls;
using P=System.Windows.Controls.Primitives;
using S = System.Windows.Shapes;
using System.Windows.Media;


namespace MapsUtilities.Overlay
{

	/**
	 * A {@code Circle} consists of a center {@link GeoPoint} and a non-negative radius in meters.
	 * <p>
	 * A {@code Circle} holds two {@link Paint} objects to allow for different outline and filling. These paints define
	 * drawing parameters such as color, stroke width, pattern and transparency. {@link Paint#setAntiAlias Anti-aliasing}
	 * should be enabled to minimize visual distortions and to improve the overall drawing quality.
	 */
	public class Circle : OverlayItem
	{
		private static void checkRadius ( float radius )
		{
			if (radius < 0) {
				throw new ArgumentException("radius must not be negative: " + radius, "radius");
			}
		}

		private static double metersToPixels ( double latitude, float meters, byte zoom )
		{
			double groundResolution = MercatorProjection.calculateGroundResolution(latitude, zoom);
			return meters / groundResolution;
		}

		private GeoPoint GeoPoint;
		private Paint paintFill;
		private Paint paintStroke;
		private float radius;

		/**
		 * @param GeoPoint
		 *            the initial center point of this circle (may be null).
		 * @param radius
		 *            the initial non-negative radius of this circle in meters.
		 * @param paintFill
		 *            the initial {@code Paint} used to fill this circle (may be null).
		 * @param paintStroke
		 *            the initial {@code Paint} used to stroke this circle (may be null).
		 * @throws ArgumentException
		 *             if the given {@code radius} is negative.
		 */
		public Circle ( GeoPoint GeoPoint, float radius, Paint paintFill, Paint paintStroke )
		{
			checkRadius(radius);
			this.GeoPoint = GeoPoint;
			this.radius = radius;
			this.paintFill = paintFill;
			this.paintStroke = paintStroke;
		}

		//@Override
		public bool draw ( BoundingBox boundingBox, byte zoomLevel, Canvas canvas, MapPoint canvasPosition )
		{
			if (this.GeoPoint == null || (this.paintStroke == null && this.paintFill == null)) {
				return false;
			}

			double latitude = this.GeoPoint.Latitude;
			double longitude = this.GeoPoint.Longitude;
			float pixelX = (float)(MercatorProjection.longitudeToPixelX(longitude, zoomLevel) - canvasPosition.X);
			float pixelY = (float)(MercatorProjection.latitudeToPixelY(latitude, zoomLevel) - canvasPosition.Y);
			float radiusInPixel = (float)metersToPixels(latitude, this.radius, zoomLevel);

			var shape = new S.Ellipse();
			shape.Width = radiusInPixel * 2;
			shape.Height = radiusInPixel * 2;

			OverlayUtils.SetShapeFormat(this.paintStroke, this.paintFill, shape);

			canvas.Children.Add(shape);
			Canvas.SetTop(shape, pixelY - radius);
			Canvas.SetLeft(shape, pixelX - radius);

			return true;
		}

		/**
		 * @return the center point of this circle (may be null).
		 */
		public GeoPoint getGeoPoint ()
		{
			return this.GeoPoint;
		}

		/**
		 * @return the {@code Paint} used to fill this circle (may be null).
		 */
		public Paint getPaintFill ()
		{
			return this.paintFill;
		}

		/**
		 * @return the {@code Paint} used to stroke this circle (may be null).
		 */
		public Paint getPaintStroke ()
		{
			return this.paintStroke;
		}

		/**
		 * @return the non-negative radius of this circle in meters.
		 */
		public float getRadius ()
		{
			return this.radius;
		}

		/**
		 * @param GeoPoint
		 *            the new center point of this circle (may be null).
		 */
		public void setGeoPoint ( GeoPoint GeoPoint )
		{
			this.GeoPoint = GeoPoint;
		}

		/**
		 * @param paintFill
		 *            the new {@code Paint} used to fill this circle (may be null).
		 */
		public void setPaintFill ( Paint paintFill )
		{
			this.paintFill = paintFill;
		}

		/**
		 * @param paintStroke
		 *            the new {@code Paint} used to stroke this circle (may be null).
		 */
		public void setPaintStroke ( Paint paintStroke )
		{
			this.paintStroke = paintStroke;
		}

		/**
		 * @param radius
		 *            the new non-negative radius of this circle in meters.
		 * @throws ArgumentException
		 *             if the given {@code radius} is negative.
		 */
		public void setRadius ( float radius )
		{
			checkRadius(radius);
			this.radius = radius;
		}
	}
}
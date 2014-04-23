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
using MapCore.Model;
using MapCore.Util;
using System.Windows;
using System.Windows.Controls;
using P = System.Windows.Controls.Primitives;
using S = System.Windows.Shapes;
using System.Windows.Shapes;
using System.Windows.Media;
using System.IO;

namespace MapsUtilities.Overlay
{

	/**
	 * A {@code Marker} draws a {@link Shape} at a given geographical position.
	 */
	public class Marker : OverlayItem
	{
		/**
		 * Sets the bounds of the given Shape so that (0,0) is the center of its bounding box.
		 * 
		 * @param Shape
		 *            the Shape whose bounds should be set.
		 * @return the given Shape with set bounds.
		 */
		public static Shape boundCenter ( Shape Shape )
		{
			double intrinsicWidth = Shape.Width;
			double intrinsicHeight = Shape.Height;
			var translation = new TranslateTransform();
			translation.X = intrinsicWidth / -2;
			translation.Y = intrinsicHeight / -2;
			Shape.RenderTransform = translation;
			return Shape;
		}

		/**
		 * Sets the bounds of the given Shape so that (0,0) is the center of its bottom row.
		 * 
		 * @param Shape
		 *            the Shape whose bounds should be set.
		 * @return the given Shape with set bounds.
		 */
		public static Shape boundCenterBottom ( Shape Shape )
		{
			double intrinsicWidth = Shape.Width;
			double intrinsicHeight = Shape.Height;
			var translation = new TranslateTransform();
			translation.X = intrinsicWidth / -2;
			translation.Y = -intrinsicHeight;
			Shape.RenderTransform = translation;
			return Shape;
		}

		private static bool intersect ( Canvas canvas, double left, double top, double right, double bottom )
		{
			return right >= 0 && left <= canvas.Width && bottom >= 0 && top <= canvas.Height;
		}

		private Shape Shape;
		private GeoPoint GeoPoint;

		/**
		 * @param GeoPoint
		 *            the initial geographical coordinates of this marker (may be null).
		 * @param Shape
		 *            the initial {@code Shape} of this marker (may be null).
		 */
		public Marker ( GeoPoint GeoPoint, Shape Shape )
		{
			this.GeoPoint = GeoPoint;
			this.Shape = Shape;
		}

		//@Override
		public bool draw ( BoundingBox boundingBox, byte zoomLevel, Canvas canvas, MapCore.Model.MapPoint canvasPosition )
		{
			if (this.GeoPoint == null || this.Shape == null) {
				return false;
			}

			double latitude = this.GeoPoint.Latitude;
			double longitude = this.GeoPoint.Longitude;
			double pixelX = (MercatorProjection.longitudeToPixelX(longitude, zoomLevel) - canvasPosition.X);
			double pixelY = (MercatorProjection.latitudeToPixelY(latitude, zoomLevel) - canvasPosition.Y);


			Rect drawableBounds = this.Shape.GetPosition();
			double left = pixelX + drawableBounds.Left;
			double top = pixelY + drawableBounds.Top;
			double right = pixelX + drawableBounds.Right ;
			double bottom = pixelY + drawableBounds.Bottom;

			if (!intersect(canvas, left, top, right, bottom)) {
				return false;
			}
			var clone = this.Shape.Clone();

			canvas.Children.Add(clone);
			Canvas.SetTop(clone, pixelX);
			Canvas.SetLeft(clone, pixelY);
			return true;
		}

		/**
		 * @return the {@code Shape} of this marker (may be null).
		 */
		public Shape getDrawable ()
		{
			return this.Shape;
		}

		/**
		 * @return the geographical coordinates of this marker (may be null).
		 */
		public GeoPoint getGeoPoint ()
		{
			return this.GeoPoint;
		}

		/**
		 * @param Shape
		 *            the new {@code Shape} of this marker (may be null).
		 */
		public void setDrawable ( Shape Shape )
		{
			this.Shape = Shape;
		}

		/**
		 * @param GeoPoint
		 *            the new geographical coordinates of this marker (may be null).
		 */
		public void setGeoPoint ( GeoPoint GeoPoint )
		{
			this.GeoPoint = GeoPoint;
		}
	}
}
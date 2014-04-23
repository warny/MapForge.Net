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
using System.Collections.Generic;
using System.Windows.Shapes;
using MapCore.Util;
using System.Windows.Media;
namespace MapsUtilities.Overlay
{

	/**
	 * A {@code PolygonalChain} is a connected series of line segments specified by a list of {@link GeoPoint GeoPoints}.
	 */
	public class PolygonalChain
	{
		private List<GeoPoint> GeoPoints;

		/**
		 * @param GeoPoints
		 *            the initial GeoPoints of this polygonal chain (may be null).
		 */
		public PolygonalChain ( IEnumerable<GeoPoint> GeoPoints )
		{
			if (GeoPoints == null) {
				this.GeoPoints = new List<GeoPoint>();
			} else {
				this.GeoPoints = new List<GeoPoint>(GeoPoints);
			}
		}

		/**
		 * @return a lock (thread-safe) list of all GeoPoints of this polygonal chain. Manual synchronization on
		 *         this list is necessary when iterating over it.
		 */
		public List<GeoPoint> getGeoPoints ()
		{
			lock (this.GeoPoints) {
				return this.GeoPoints;
			}
		}

		/**
		 * @return true if the first and the last GeoPoint of this polygonal chain are equal, false otherwise.
		 */
		public bool isClosed ()
		{
			lock (this.GeoPoints) {
				int numberOfGeoPoints = this.GeoPoints.Count;
				if (numberOfGeoPoints < 2) {
					return false;
				}

				GeoPoint GeoPointFirst = this.GeoPoints[0];
				GeoPoint GeoPointLast = this.GeoPoints[numberOfGeoPoints - 1];
				return GeoPointFirst.Equals(GeoPointLast);
			}
		}

		/**
		 * @param zoomLevel
		 *            the zoom level at which this {@code PolygonalChain} should draw itself.
		 * @param canvasPosition
		 *            the top-left pixel position of the canvas on the world map at the given zoom level.
		 * @param closeAutomatically
		 *            whether the generated path should always be closed.
		 * @return a {@code Path} representing this {@code PolygonalChain} (may be null).
		 */
		public Path draw ( byte zoomLevel, MapPoint canvasPosition, bool closeAutomatically )
		{
			lock (this.GeoPoints) {
				int numberOfGeoPoints = this.GeoPoints.Count;
				if (numberOfGeoPoints < 2) {
					return null;
				}

				Path path = new Path();
				PathGeometry geometry = new PathGeometry();
				PathFigure figure = new PathFigure();
				for (int i = 0; i < numberOfGeoPoints; ++i) {
					GeoPoint GeoPoint = this.GeoPoints[i];
					double latitude = GeoPoint.Latitude;
					double longitude = GeoPoint.Longitude;
					float pixelX = (float)(MercatorProjection.longitudeToPixelX(longitude, zoomLevel) - canvasPosition.X);
					float pixelY = (float)(MercatorProjection.latitudeToPixelY(latitude, zoomLevel) - canvasPosition.Y);

					if (i == 0) {
						figure.StartPoint = new Point(pixelX, pixelY);
					} else {
						var segment = new LineSegment();
						segment.point = new Point(pixelX, pixelY);
						figure.Segments.Add(segment);
					}
				}

				if (closeAutomatically && !isClosed()) {
					var segment = new LineSegment();
					segment.point = figure.StartPoint;
					figure.Segments.Add(segment);
				}
				return path;
			}
		}
	}
}
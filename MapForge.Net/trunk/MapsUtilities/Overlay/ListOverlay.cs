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
using System.Collections.Generic;
using MapCore.Util;
using MapCore.Model;
using System.Windows.Controls;
namespace MapsUtilities.Overlay
{

	/**
	 * A thread-safe {@link Overlay} implementation to display a list of {@link OverlayItem OverlayItems}.
	 */
	public class ListOverlay : Overlay
	{
		private List<OverlayItem> overlayItems = new List<OverlayItem>();

		//@Override
		public void draw ( BoundingBox boundingBox, byte zoomLevel, Canvas canvas )
		{
			double canvasPixelLeft = MercatorProjection.longitudeToPixelX(boundingBox.MinLongitude, zoomLevel);
			double canvasPixelTop = MercatorProjection.latitudeToPixelY(boundingBox.MaxLatitude, zoomLevel);
			MapPoint canvasPosition = new MapPoint(canvasPixelLeft, canvasPixelTop);

			lock (this.overlayItems) {
				foreach (OverlayItem overlayItem in overlayItems) {
					overlayItem.draw(boundingBox, zoomLevel, canvas, canvasPosition);
				}
			}
		}

		/**
		 * @return a lock (thread-safe) list of all {@link OverlayItem OverlayItems} on this {@code ListOverlay}.
		 *         Manual synchronization on this list is necessary when iterating over it.
		 */
		public List<OverlayItem> getOverlayItems ()
		{
			return this.overlayItems;
		}
	}
}
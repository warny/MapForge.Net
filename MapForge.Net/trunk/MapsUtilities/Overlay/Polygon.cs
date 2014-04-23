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
using MapCore.Model;
using System.Linq;
using System.Windows.Shapes;
using System.Windows.Controls;
namespace MapsUtilities.Overlay
{

	/**
	 * A {@code Polygon} draws a list of {@link PolygonalChain PolygonalChains}. As a polygon represents a closed area, any
	 * open {@code PolygonalChain} will automatically be closed during the draw process.
	 * <p>
	 * A {@code Polygon} holds two {@link Paint} objects to allow for different outline and filling. These paints define
	 * drawing parameters such as color, stroke width, pattern and transparency. {@link Paint#setAntiAlias Anti-aliasing}
	 * should be enabled to minimize visual distortions and to improve the overall drawing quality.
	 */
	public class Polygon : OverlayItem
	{
		private Paint paintFill;
		private Paint paintStroke;
		private List<PolygonalChain> polygonalChains;

		/**
		 * @param polygonalChains
		 *            the initial polygonal chains on this polygon (may be null).
		 * @param paintFill
		 *            the initial {@code Paint} used to fill this polygon (may be null).
		 * @param paintStroke
		 *            the initial {@code Paint} used to stroke this polygon (may be null).
		 */
		public Polygon ( IEnumerable<PolygonalChain> polygonalChains, Paint paintFill, Paint paintStroke )
		{
			if (polygonalChains == null) {
				this.polygonalChains = new List<PolygonalChain>();
			} else {
				this.polygonalChains = new List<PolygonalChain>(polygonalChains);
			}
			this.paintFill = paintFill;
			this.paintStroke = paintStroke;
		}

		//@Override
		public bool draw ( BoundingBox boundingBox, byte zoomLevel, Canvas canvas, MapPoint canvasPosition )
		{
			lock (this.polygonalChains) {
				if (!this.polygonalChains.Any() || (this.paintStroke == null && this.paintFill == null)) {
					return false;
				}


				for (int i = 0; i < this.polygonalChains.Count; ++i) {
					PolygonalChain polygonalChain = this.polygonalChains[i];
					Path closedPath = polygonalChain.draw(zoomLevel, canvasPosition, true);
					//closedPath.setFillType(FillType.EVEN_ODD);
					if (closedPath != null) {
						OverlayUtils.SetShapeFormat(this.paintStroke, this.paintFill, closedPath);
						canvas.Children.Add(closedPath);
					}
				}

				return true;
			}
		}

		/**
		 * @return the {@code Paint} used to fill this polygon (may be null).
		 */
		public Paint getPaintFill ()
		{
			return this.paintFill;
		}

		/**
		 * @return the {@code Paint} used to stroke this polygon (may be null).
		 */
		public Paint getPaintStroke ()
		{
			return this.paintStroke;
		}

		/**
		 * @return a lock (thread-safe) list of all polygonal chains on this polygon. Manual synchronization on this
		 *         list is necessary when iterating over it.
		 */
		public List<PolygonalChain> getPolygonalChains ()
		{
			lock (this.polygonalChains) {
				return this.polygonalChains;
			}
		}

		/**
		 * @param paintFill
		 *            the new {@code Paint} used to fill this polygon (may be null).
		 */
		public void setPaintFill ( Paint paintFill )
		{
			this.paintFill = paintFill;
		}

		/**
		 * @param paintStroke
		 *            the new {@code Paint} used to stroke this polygon (may be null).
		 */
		public void setPaintStroke ( Paint paintStroke )
		{
			this.paintStroke = paintStroke;
		}
	}
}
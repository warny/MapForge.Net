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
using MapCore.Model;
using System.Windows.Controls;
using System.Windows.Shapes;
namespace MapsUtilities.Overlay {

/**
 * A {@code Polyline} draws a {@link PolygonalChain}.
 * <p>
 * A {@code Polyline} holds a {@link Paint} object which defines drawing parameters such as color, stroke width, pattern
 * and transparency. {@link Paint#setAntiAlias Anti-aliasing} should be enabled to minimize visual distortions and to
 * improve the overall drawing quality.
 */
public class Polyline : OverlayItem {
	private Paint paintStroke;
	private PolygonalChain polygonalChain;

	/**
	 * @param polygonalChain
	 *            the initial polygonal chain of this polyline (may be null).
	 * @param paintStroke
	 *            the initial {@code Paint} used to stroke this polyline (may be null).
	 */
	public Polyline(PolygonalChain polygonalChain, Paint paintStroke) {
		this.polygonalChain = polygonalChain;
		this.paintStroke = paintStroke;
	}

	//@Override
	public  bool draw(BoundingBox boundingBox, byte zoomLevel, Canvas canvas, MapPoint canvasPosition) {
		if (this.polygonalChain == null || this.paintStroke == null) {
			return false;
		}

		Path path = this.polygonalChain.draw(zoomLevel, canvasPosition, false);
		if (path == null) {
			return false;
		}
		OverlayUtils.SetShapeFormat(paintStroke, null, path);

		canvas.Children.Add(path);
		return true;
	}

	/**
	 * @return the {@code Paint} used to stroke this polyline (may be null).
	 */
	public  Paint getPaintStroke() {
		return this.paintStroke;
	}

	/**
	 * @return the polygonal chain of this polyline (may be null).
	 */
	public  PolygonalChain getPolygonalChain() {
		return this.polygonalChain;
	}

	/**
	 * @param paintStroke
	 *            the new {@code Paint} used to stroke this polyline (may be null).
	 */
	public  void setPaintStroke(Paint paintStroke) {
		this.paintStroke = paintStroke;
	}

	/**
	 * @param polygonalChain
	 *            the new polygonal chain of this polyline (may be null).
	 */
	public  void setPolygonalChain(PolygonalChain polygonalChain) {
		this.polygonalChain = polygonalChain;
	}
}
}
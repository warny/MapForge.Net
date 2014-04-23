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
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using MapCore.Model;
using System.Windows.Media;
using System.Windows;
namespace RenderTheme.RenderInstructions {

/**
 * Represents an icon along a polyline on the map.
 */
public class LineSymbol : RenderInstruction {
	private bool alignCenter;
	private ImageSource BitmapImage;
	private bool repeat;

	public LineSymbol(LineSymbolBuilder lineSymbolBuilder) {
		this.alignCenter = lineSymbolBuilder.alignCenter;
		this.BitmapImage = lineSymbolBuilder.BitmapImage;
		this.repeat = lineSymbolBuilder.repeat;
	}

	public void Dispose() {
		//this.BitmapImage.Dispose();
	}

	public void RenderNode ( DrawingContext drawingContext, MapElement element )
	{
		// do nothing
	}

	public void RenderWay ( DrawingContext drawingContext, MapElement element )
	{
		Size size = new Size(BitmapImage.Width, BitmapImage.Height);

		foreach (var way in element.WayGeometry.Figures) {
			Point point = new Point(way.StartPoint.X - size.Width / 2, way.StartPoint.Y - size.Height / 2);
			drawingContext.DrawImage(this.BitmapImage, new System.Windows.Rect(point, size));

			foreach (var segment in way.Segments) {
				//point = new Point(segment..X - size.Width / 2, segment.StartPoint.Y - size.Height / 2);
				//drawingContext.DrawImage(this.BitmapImage, new System.Windows.Rect(point, size));
			}
		}
	}

	public void ScaleStrokeWidth(float scaleFactor) {
		// do nothing
	}

	public void ScaleTextSize(float scaleFactor) {
		// do nothing
	}
}
}
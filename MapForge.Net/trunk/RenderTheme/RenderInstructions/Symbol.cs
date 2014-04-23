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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MapCore.Model;

namespace RenderTheme.RenderInstructions
{

	/**
	 * Represents an icon on the map.
	 */
	public class Symbol : RenderInstruction
	{
		private ImageSource BitmapImage;

		public Symbol ( SymbolBuilder symbolBuilder )
		{
			this.BitmapImage = symbolBuilder.BitmapImage;
		}

		public void Dispose ()
		{
			//this.BitmapImage.Dispose();
		}

		public void RenderWay ( DrawingContext drawingContext, MapElement element )
		{
			Render(drawingContext, element);
		}

		public void RenderNode ( DrawingContext drawingContext, MapElement element )
		{
			Render(drawingContext, element);
		}

		private void Render ( DrawingContext drawingContext, MapElement element )
		{
			if (element.Position == null) return;
			Size size = new Size(BitmapImage.Width, BitmapImage.Height);
			Point point = new Point(element.Position.Value.X - size.Width / 2, element.Position.Value.Y - size.Height / 2);
			drawingContext.DrawImage(this.BitmapImage, new System.Windows.Rect(point, size));
		}

		public void ScaleStrokeWidth ( float scaleFactor )
		{
			// do nothing
		}

		public void ScaleTextSize ( float scaleFactor )
		{
			// do nothing
		}
	}
}

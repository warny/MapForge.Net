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
using System.Windows.Media;
namespace RenderTheme.RenderInstructions
{

	/**
	 * Represents a text label on the map.
	 */
	public class Caption : RenderInstruction
	{
		private float dy;
		private Brush fill;
		private float fontSize;
		private Pen stroke;
		private TextKey textKey;

		public Caption ( CaptionBuilder captionBuilder )
		{
			this.dy = captionBuilder.dy;
			this.fill = captionBuilder.fill;
			this.fontSize = captionBuilder.fontSize;
			this.stroke = captionBuilder.stroke;
			this.textKey = captionBuilder.textKey;
		}

		public void Dispose ()
		{
			// do nothing
		}

		public void RenderNode ( DrawingContext drawingContext, MapElement element )
		{
			string caption = this.textKey.getValue(element.Tags);
			if (caption == null) {
				return;
			}
			FormattedText text = new FormattedText(caption, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), fontSize, Brushes.Black); 
			
		}

		public void RenderWay ( DrawingContext drawingContext, MapElement element )
		{
			string caption = this.textKey.getValue(element.Tags);
			if (caption == null) {
				return;
			}
			//renderCallback.renderAreaCaption(caption, this.dy, this.fill, this.stroke);
		}

		public void ScaleStrokeWidth ( float scaleFactor )
		{
			// do nothing
		}

		public void ScaleTextSize ( float scaleFactor )
		{
			//this.fill.TextSize = this.fontSize * scaleFactor;
			//this.stroke.TextSize = this.fontSize * scaleFactor;
		}
	}
}
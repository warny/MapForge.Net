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
using System.Windows.Media;
using MapCore.Model;
using RenderTheme.Graphics;
namespace RenderTheme.RenderInstructions
{

	/**
	 * Represents a round area on the map.
	 */
	public class Circle : RenderInstruction
	{
		private Brush fill;
		private int level;
		private float radius;
		private float renderRadius;
		private bool scaleRadius;
		private Pen stroke;
		private float strokeWidth;

		public Circle ( CircleBuilder circleBuilder )
		{
			this.fill = circleBuilder.fill;
			this.level = circleBuilder.level;
			this.radius = circleBuilder.radius;
			this.scaleRadius = circleBuilder.scaleRadius;
			this.stroke = circleBuilder.stroke;
			this.strokeWidth = circleBuilder.strokeWidth;

			if (!this.scaleRadius) {
				this.renderRadius = this.radius;
				if (this.stroke != null) {
					this.stroke.Thickness = this.strokeWidth;
				}
			}
		}

		public void Dispose ()
		{
			// do nothing
		}

		public void RenderNode ( DrawingContext drawingContext, MapElement element )
		{
			if (element.Position == null) return;
			drawingContext.DrawEllipse(fill, stroke, element.Position.Value, radius, radius);
		}

		public void RenderWay ( DrawingContext drawingContext, MapElement element )
		{
			// do nothing
		}

		public void ScaleStrokeWidth ( float scaleFactor )
		{
			if (this.scaleRadius) {
				this.renderRadius = this.radius * scaleFactor;
				if (this.stroke != null) {
					this.stroke.Thickness = this.strokeWidth * scaleFactor;
				}
			}
		}

		public void ScaleTextSize ( float scaleFactor )
		{
			// do nothing
		}
	}
}
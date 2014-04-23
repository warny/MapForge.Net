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

using System.Linq;
using RenderTheme.Graphics;
using System.Collections.Generic;
using MapCore.Model;
using System.Windows.Media;
namespace RenderTheme.RenderInstructions
{

	/**
	 * Represents a closed polygon on the map.
	 */
	public class Area : RenderInstruction
	{
		protected Brush fill;
		protected int level;
		protected Pen stroke;
		protected float strokeWidth;

		protected Area () { }

		public Area ( AreaBuilder areaBuilder )
		{
			this.fill = areaBuilder.fill;
			this.level = areaBuilder.level;
			this.stroke = areaBuilder.stroke;
			this.strokeWidth = areaBuilder.strokeWidth;
		}

		public void RenderNode ( DrawingContext drawingContext, MapElement element )
		{
			// do nothing
		}

		public void RenderWay ( DrawingContext drawingContext, MapElement element )
		{
			drawingContext.DrawGeometry(this.fill, this.stroke, element.WayGeometry);
		}

		public void ScaleStrokeWidth ( float scaleFactor )
		{
			if (this.stroke != null) {
				this.stroke.Thickness = this.strokeWidth * scaleFactor;
			}
		}

		public void ScaleTextSize ( float scaleFactor )
		{
			// do nothing
		}

		public override bool Equals(object obj)
		{
			var other = obj as Area;
			if (other == null) return false;

			return this.GetHashCode() == obj.GetHashCode();
		}

		public override int GetHashCode ()
		{
			unchecked {
				return new [] {
					this.stroke.GetHashCode(),
					this.fill.GetHashCode(),
					this.level.GetHashCode()
				}.Aggregate(7, ( accumulator, variable ) => accumulator + 31 * variable);
			}
		}
	}						    
}
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
using System;
using System.Collections.Generic;
using System.Windows.Media;
using MapCore.Model;

namespace RenderTheme.RenderInstructions
{

	/**
	 * A RenderInstruction is a basic graphical primitive to draw a map.
	 */
	public interface RenderInstruction
	{
		/**
		 * @param renderCallback
		 *            a reference to the receiver of all render callbacks.
		 * @param tags
		 *            the tags of the node.
		 */
		void RenderWay ( DrawingContext drawingContext, MapElement element );

		/**
		 * @param renderCallback
		 *            a reference to the receiver of all render callbacks.
		 * @param tags
		 *            the tags of the way.
		 */
		void RenderNode ( DrawingContext drawingContext, MapElement element );

		/**
		 * Scales the stroke width of this RenderInstruction by the given factor.
		 * 
		 * @param scaleFactor
		 *            the factor by which the stroke width should be scaled.
		 */
		void ScaleStrokeWidth ( float scaleFactor );

		/**
		 * Scales the text size of this RenderInstruction by the given factor.
		 * 
		 * @param scaleFactor
		 *            the factor by which the text size should be scaled.
		 */
		void ScaleTextSize ( float scaleFactor );
	}
}
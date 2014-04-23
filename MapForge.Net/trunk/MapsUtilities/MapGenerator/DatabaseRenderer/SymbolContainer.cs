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
using System.Windows.Media.Imaging;
namespace MapsUtilities.MapGenerator.DatabaseRenderer
{

	public class SymbolContainer
	{
		public bool alignCenter;
		public MapPoint point;
		public float rotation;
		public BitmapImage symbol;

		public SymbolContainer ( BitmapImage symbol, MapPoint point )
			: this(symbol, point, false, 0)
		{
		}

		public SymbolContainer ( BitmapImage symbol, MapPoint point, bool alignCenter, float rotation )
		{
			this.symbol = symbol;
			this.point = point;
			this.alignCenter = alignCenter;
			this.rotation = rotation;
		}
	}

}
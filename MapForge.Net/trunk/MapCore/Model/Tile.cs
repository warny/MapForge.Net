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
using System.Runtime.Serialization;
using System.Text;
using System.Linq;

namespace MapCore.Model
{

	/**
	 * A tile represents a rectangular part of the world map. All tiles can be identified by their X and Y number together
	 * with their zoom level. The actual area that a tile covers on a map depends on the underlying map projection.
	 */
	public class Tile : IComparable<Tile> {
		/**
		 * Width and height of a map tile in pixel.
		 */
		public int TileSize { get; private set; }

		private const long serialVersionUID = 1L;

		/**
		 * The X number of this tile.
		 */
		public long TileX { get; set; }

		/**
		 * The Y number of this tile.
		 */
		public long TileY { get; set; }

		/**
		 * The zoom level of this tile.
		 */
		public byte ZoomFactor { get; set; }

		/**
		 * @param tileX
		 *            the X number of the tile.
		 * @param tileY
		 *            the Y number of the tile.
		 * @param zoomLevel
		 *            the zoom level of the tile.
		 */
		public Tile ( long tileX, long tileY, byte zoomFactor, int tileSize )
		{
			this.TileX = tileX;
			this.TileY = tileY;
			this.ZoomFactor = zoomFactor;
			this.TileSize = tileSize;
		}

		public override bool Equals ( Object obj )
		{
			if (this == obj) {
				return true;
			} else if (!(obj is Tile)) {
				return false;
			}
			Tile other = (Tile)obj;
			if (this.TileX != other.TileX) {
				return false;
			} else if (this.TileY != other.TileY) {
				return false;
			} else if (this.ZoomFactor != other.ZoomFactor) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// Upper left point of this tile
		/// </summary>
		public MapPoint MapPoint1
		{
			get
			{
				return new MapPoint(
					this.TileX * TileSize,
					this.TileY * TileSize,
					ZoomFactor,
					TileSize
				);
			}
		}

		/// <summary>
		/// Lowerright point of this tile
		/// </summary>
		public MapPoint MapPoint2
		{
			get
			{
				return new MapPoint(
					(this.TileX + 1) * TileSize,
					(this.TileY + 1) * TileSize,
					ZoomFactor,
					TileSize
				);
			}
		}

		/// <summary>
		/// Test if a point is contained
		/// </summary>
		/// <param name="mappoint">point to test</param>
		/// <returns>True if contained</returns>
		public bool Contains ( MapPoint mappoint )
		{
			if (mappoint.X < this.TileX * TileSize || mappoint.X > (this.TileX + 1) * TileSize) return false;
			if (mappoint.Y < this.TileY * TileSize || mappoint.Y > (this.TileY + 1) * TileSize) return false;
			return true;
		}

		public override int GetHashCode ()
		{
			int result = 7;
			result = 31 * result + (int)(this.TileX ^ (this.TileX >> 32));
			result = 31 * result + (int)(this.TileY ^ (this.TileY >> 32));
			result = 31 * result + this.ZoomFactor;
			return result;
		}

		public override string ToString ()
		{
			return string.Format("tileX={0}, tileY={1}, zoomLevel={2}", this.TileX,this.TileY,this.ZoomFactor);
		}

		#region IComparable<Tile> Membres

		public int CompareTo ( Tile other )
		{
			return (new int[] {
				this.TileX.CompareTo(other.TileX),
				this.TileY.CompareTo(other.TileY),
				this.ZoomFactor.CompareTo(other.ZoomFactor),
				this.TileSize.CompareTo(other.TileSize)
			}).FirstOrDefault(c=>c!=0);
		}

		#endregion
	}
}
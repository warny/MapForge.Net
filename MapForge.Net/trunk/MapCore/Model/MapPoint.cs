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

/**
 * A point represents an immutable pair of double coordinates.
 */
namespace MapCore.Model
{

	public class MapPoint : IComparable<MapPoint> /*, ISerializable*/ {
		private const long serialVersionUID = 1L;

		/// <summary>
		/// The x coordinate of this point.
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// The y coordinate of this point.
		/// </summary>
		public double Y { get; set; }

		/// <summary>
		/// Zoom factor 
		/// </summary>
		public byte ZoomFactor { get; private set; }

		/// <summary>
		/// Size of tiles
		/// </summary>
		public int TileSize { get; private set; }

		/// <summary>
		/// Build a new Mappoint
		/// </summary>
		/// <param name="x">the x coordinate of this point.</param>
		/// <param name="y">the y coordinate of this point.</param>
		public MapPoint ( double x, double y, byte zoomFactor, int tileSize )
		{
			this.X = x;
			this.Y = y;
			this.ZoomFactor = zoomFactor;
			this.TileSize = tileSize;
		}

		public MapPoint ChangeZoomFactor ( byte zoomFactor )
		{
			if (zoomFactor == this.ZoomFactor) {
				return this;
			} else if (zoomFactor > this.ZoomFactor) {
				long multiplier = 1 << (zoomFactor - this.ZoomFactor);
				return new MapPoint(this.X * multiplier, this.Y * multiplier, zoomFactor, this.TileSize);
			} else {
				long divider = 1 << (this.ZoomFactor - zoomFactor);
				return new MapPoint(this.X / divider, this.Y / divider, zoomFactor, this.TileSize);
			}
		}

		public int CompareTo ( MapPoint point )
		{
			point = point.ChangeZoomFactor(this.ZoomFactor);
			if (this.X > point.X) {
				return 1;
			} else if (this.X < point.X) {
				return -1;
			} else if (this.Y > point.Y) {
				return 1;
			} else if (this.Y < point.Y) {
				return -1;
			}
			return 0;
		}


		public override bool Equals ( Object obj )
		{
			if (this == obj) {
				return true;
			} else if (!(obj is MapPoint)) {
				return false;
			}
			MapPoint other = (MapPoint)obj;
			if (BitConverter.DoubleToInt64Bits(this.X) != BitConverter.DoubleToInt64Bits(other.X)) {
				return false;
			} else if (BitConverter.DoubleToInt64Bits(this.Y) != BitConverter.DoubleToInt64Bits(other.Y)) {
				return false;
			}
			return true;
		}

		public override int GetHashCode ()
		{
			const int prime = 31;
			int result = 1;
			long temp;
			temp = BitConverter.DoubleToInt64Bits(this.X);
			result = prime * result + (int)(temp ^ (temp >> 32));
			temp = BitConverter.DoubleToInt64Bits(this.Y);
			result = prime * result + (int)(temp ^ (temp >> 32));
			return result;
		}

		public override string ToString ()
		{
			return string.Format("x={0}, y={1}", this.X, this.Y);
		}

		public static implicit operator System.Windows.Point ( MapPoint point )
		{
			return new System.Windows.Point(point.X /* point.TileSize*/, point.Y /* point.TileSize*/);
		}

	}
}

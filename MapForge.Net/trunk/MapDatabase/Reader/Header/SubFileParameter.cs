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
using MapCore.Projections;
using MapCore.Util;

namespace MapDB.Reader.Header
{
	/**
	 * Holds all parameters of a sub-file.
	 */
	public class SubFileParameter
	{
		/**
		 * Number of bytes a single index entry consists of.
		 */
		public const byte BytesPerIndexEntry = 5;

		/**
		 * Base zoom level of the sub-file, which equals to one block.
		 */
		public byte BaseZoomLevel { get; private set; }

		/**
		 * Vertical amount of blocks in the grid.
		 */
		public long BlocksHeight { get; private set; }

		/**
		 * Horizontal amount of blocks in the grid.
		 */
		public long BlocksWidth { get; private set; }

		/**
		 * Y number of the tile at the bottom boundary in the grid.
		 */
		public long BoundaryTileBottom { get; private set; }

		/**
		 * X number of the tile at the left boundary in the grid.
		 */
		public long BoundaryTileLeft { get; private set; }

		/**
		 * X number of the tile at the right boundary in the grid.
		 */
		public long BoundaryTileRight { get; private set; }

		/**
		 * Y number of the tile at the top boundary in the grid.
		 */
		public long BoundaryTileTop { get; private set; }

		/**
		 * Absolute end address of the index in the enclosing file.
		 */
		public long IndexEndAddress { get; private set; }

		/**
		 * Absolute start address of the index in the enclosing file.
		 */
		public long IndexStartAddress { get; private set; }

		/**
		 * Total number of blocks in the grid.
		 */
		public long NumberOfBlocks { get; private set; }

		/**
		 * Absolute start address of the sub-file in the enclosing file.
		 */
		public long StartAddress { get; private set; }

		/**
		 * Size of the sub-file in bytes.
		 */
		public long SubFileSize { get; private set; }

		/**
		 * Maximum zoom level for which the block entries tables are made.
		 */
		public byte ZoomLevelMax { get; private set; }

		/**
		 * Minimum zoom level for which the block entries tables are made.
		 */
		public byte ZoomLevelMin { get; private set; }

		public SubFileParameter ( SubFileParameterBuilder subFileParameterBuilder, RepresentationConverter projection )
		{
			this.StartAddress = subFileParameterBuilder.startAddress;
			this.IndexStartAddress = subFileParameterBuilder.indexStartAddress;
			this.SubFileSize = subFileParameterBuilder.subFileSize;
			this.BaseZoomLevel = subFileParameterBuilder.baseZoomLevel;
			this.ZoomLevelMin = subFileParameterBuilder.zoomLevelMin;
			this.ZoomLevelMax = subFileParameterBuilder.zoomLevelMax;

			// calculate the XY numbers of the boundary tiles in this sub-file
			var tile1 = projection.GeoPointToTile(subFileParameterBuilder.boundingBox.point1, this.BaseZoomLevel);
			var tile2 = projection.GeoPointToTile(subFileParameterBuilder.boundingBox.point2, this.BaseZoomLevel);
			this.BoundaryTileLeft = tile1.TileX;
			this.BoundaryTileBottom = tile1.TileY;
			this.BoundaryTileRight = tile2.TileX;
			this.BoundaryTileTop = tile2.TileY;

			// calculate the horizontal and vertical amount of blocks in this sub-file
			this.BlocksWidth = this.BoundaryTileRight - this.BoundaryTileLeft + 1;
			this.BlocksHeight = this.BoundaryTileBottom - this.BoundaryTileTop + 1;

			// calculate the total amount of blocks in this sub-file
			this.NumberOfBlocks = this.BlocksWidth * this.BlocksHeight;

			this.IndexEndAddress = this.IndexStartAddress + this.NumberOfBlocks * BytesPerIndexEntry;
		}

		public override bool Equals ( Object obj )
		{
			if (this == obj) {
				return true;
			} else if (!(obj is SubFileParameter)) {
				return false;
			}
			SubFileParameter other = (SubFileParameter)obj;
			if (this.StartAddress != other.StartAddress) {
				return false;
			} else if (this.SubFileSize != other.SubFileSize) {
				return false;
			} else if (this.BaseZoomLevel != other.BaseZoomLevel) {
				return false;
			}
			return true;
		}

		public override int GetHashCode ()
		{
			int result = 7;
			result = 31 * result + (int)(this.StartAddress ^ (this.StartAddress >> 32));
			result = 31 * result + (int)(this.SubFileSize ^ (this.SubFileSize >> 32));
			result = 31 * result + this.BaseZoomLevel;
			return result;
		}

	}
}
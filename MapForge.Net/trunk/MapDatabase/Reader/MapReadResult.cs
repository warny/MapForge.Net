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
using MapCore;
using MapCore.Model;
namespace MapDB.Reader
{

	/**
	 * An immutable container for the data returned by the {@link MapDatabase}.
	 */
	public class MapReadResult : IMapReadResult
	{
		/**
		 * True if the read area is completely covered by water, false otherwise.
		 */
		public bool IsWater { get; private set; }

		/**
		 * The read nodes.
		 */
		public List<Node> Nodes { get; private set; }

		/**
		 * The read ways.
		 */
		public List<Way> Ways { get; private set; }

		public MapReadResult ( MapReadResultBuilder mapReadResultBuilder )
		{
			this.Nodes = mapReadResultBuilder.nodes;
			this.Ways = mapReadResultBuilder.ways;
			this.IsWater = mapReadResultBuilder.isWater;
		}
	}
}
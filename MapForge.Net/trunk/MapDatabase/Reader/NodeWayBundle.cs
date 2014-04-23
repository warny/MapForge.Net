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
using MapCore.Model;
using MapDB.Reader;

namespace MapDB.Reader
{
	public class NodeWayBundle
	{
		public Tile Tile { get; private set; }
		public List<Node> Nodes { get; private set; }
		public List<Way> Ways { get; private set; }

		public NodeWayBundle (Tile tile, List<Node> nodes, List<Way> ways )
		{
			this.Tile = tile;
			this.Nodes = nodes;
			this.Ways = ways;
		}
	}
}
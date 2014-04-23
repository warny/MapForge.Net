using System;
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
using MapCore.Model;
using MapCore.Projections;
namespace MapDB.Reader.Header
{

	public class MapFileInfoBuilder
	{
		public RepresentationConverter Projection;
		public BoundingBox boundingBox;
		public long fileSize;
		public int fileVersion;
		public DateTime mapDate;
		public byte numberOfSubFiles;
		public OptionalFields optionalFields;
		public Tag[] nodeTags;
		public string projectionName;
		public int tilePixelSize;
		public Tag[] wayTags;

		public MapFileInfo build ()
		{
			return new MapFileInfo(this);
		}
	}
}
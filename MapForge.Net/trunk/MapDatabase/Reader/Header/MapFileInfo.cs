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

	/**
	 * Contains the immutable metadata of a map file.
	 * 
	 * @see MapDatabase#getMapFileInfo()
	 */
	public class MapFileInfo
	{
		/// <summary>
		/// Transformation de représentation de carte
		/// </summary>
		public RepresentationConverter Projection { get; private set; }

		/**
		 * The bounding box of the map file.
		 */
		public BoundingBox BoundingBox { get; private set; }

		/**
		 * The comment field of the map file (may be null).
		 */
		public string Comment { get; private set; }

		/**
		 * The created by field of the map file (may be null).
		 */
		public string CreatedBy { get; private set; }

		/**
		 * True if the map file includes debug information, false otherwise.
		 */
		public bool DebugFile { get; private set; }

		/**
		 * The size of the map file, measured in bytes.
		 */
		public long FileSize { get; private set; }

		/**
		 * The file version number of the map file.
		 */
		public int FileVersion { get; private set; }

		/**
		 * The preferred language for names as defined in ISO 3166-1 (may be null).
		 */
		public string LanguagePreference { get; private set; }

		/**
		 * The date of the map data in milliseconds since January 1, 1970.
		 */
		public DateTime MapDate { get; private set; }

		/**
		 * The number of sub-files in the map file.
		 */
		public byte NumberOfSubFiles { get; private set; }

		/**
		 * The NODE tags.
		 */
		public Tag[] NodeTags { get; private set; }

		/**
		 * The way tags.
		 */
		public Tag[] WayTags { get; private set; }

		/**
		 * The name of the projection used in the map file.
		 */
		public string ProjectionName { get; private set; }

		/**
		 * The map start position from the file header (may be null).
		 */
		public GeoPoint StartPosition { get; private set; }

		/**
		 * The map start zoom level from the file header (may be null).
		 */
		public byte StartZoomLevel { get; private set; }

		/**
		 * The size of the tiles in pixels.
		 */
		public int TilePixelSize { get; private set; }

		public MapFileInfo ( MapFileInfoBuilder mapFileInfoBuilder )
		{
			this.Comment = mapFileInfoBuilder.optionalFields.comment;
			this.CreatedBy = mapFileInfoBuilder.optionalFields.createdBy;
			this.DebugFile = mapFileInfoBuilder.optionalFields.isDebugFile;
			this.FileSize = mapFileInfoBuilder.fileSize;
			this.FileVersion = mapFileInfoBuilder.fileVersion;
			this.LanguagePreference = mapFileInfoBuilder.optionalFields.languagePreference;
			this.BoundingBox = mapFileInfoBuilder.boundingBox;
			this.MapDate = mapFileInfoBuilder.mapDate;
			this.NumberOfSubFiles = mapFileInfoBuilder.numberOfSubFiles;
			this.NodeTags = mapFileInfoBuilder.nodeTags;
			this.ProjectionName = mapFileInfoBuilder.projectionName;
			this.StartPosition = mapFileInfoBuilder.optionalFields.startPosition;
			this.StartZoomLevel = mapFileInfoBuilder.optionalFields.startZoomLevel;
			this.TilePixelSize = mapFileInfoBuilder.tilePixelSize;
			this.WayTags = mapFileInfoBuilder.wayTags;

			this.Projection = new RepresentationConverter(Projections.GetProjection(ProjectionName), TilePixelSize); 
		}

	}
}
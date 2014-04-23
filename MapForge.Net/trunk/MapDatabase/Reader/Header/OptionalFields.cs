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
using MapCore.Model;
namespace MapDB.Reader.Header
{

	//import org.mapsforge.core.model.CoordinatesUtil;
	//import org.mapsforge.core.model.GeoPoint;
	//import org.mapsforge.map.reader.ReadBuffer;

	public class OptionalFields
	{
		/**
		 * Bitmask for the comment field in the file header.
		 */
		private const int HEADER_BITMASK_COMMENT = 0x08;

		/**
		 * Bitmask for the created by field in the file header.
		 */
		private const int HEADER_BITMASK_CREATED_BY = 0x04;

		/**
		 * Bitmask for the debug flag in the file header.
		 */
		private const int HEADER_BITMASK_DEBUG = 0x80;

		/**
		 * Bitmask for the language preference field in the file header.
		 */
		private const int HEADER_BITMASK_LANGUAGE_PREFERENCE = 0x10;

		/**
		 * Bitmask for the start position field in the file header.
		 */
		private const int HEADER_BITMASK_START_POSITION = 0x40;

		/**
		 * Bitmask for the start zoom level field in the file header.
		 */
		private const int HEADER_BITMASK_START_ZOOM_LEVEL = 0x20;

		/**
		 * The length of the language preference string.
		 */
		private const int LANGUAGE_PREFERENCE_LENGTH = 2;

		/**
		 * Maximum valid start zoom level.
		 */
		private const int START_ZOOM_LEVEL_MAX = 22;

		public static void readOptionalFields ( BufferStream readBuffer, MapFileInfoBuilder mapFileInfoBuilder )
		{
			OptionalFields optionalFields = new OptionalFields((byte)readBuffer.ReadByte());
			mapFileInfoBuilder.optionalFields = optionalFields;

			optionalFields.ReadOptionalFields(readBuffer);
		}

		public string comment;
		public string createdBy;
		public bool hasComment;
		public bool hasCreatedBy;
		public bool hasLanguagePreference;
		public bool hasStartPosition;
		public bool hasStartZoomLevel;
		public bool isDebugFile;
		public string languagePreference;
		public GeoPoint startPosition;
		public byte startZoomLevel;

		private OptionalFields ( byte flags )
		{
			this.isDebugFile = (flags & HEADER_BITMASK_DEBUG) != 0;
			this.hasStartPosition = (flags & HEADER_BITMASK_START_POSITION) != 0;
			this.hasStartZoomLevel = (flags & HEADER_BITMASK_START_ZOOM_LEVEL) != 0;
			this.hasLanguagePreference = (flags & HEADER_BITMASK_LANGUAGE_PREFERENCE) != 0;
			this.hasComment = (flags & HEADER_BITMASK_COMMENT) != 0;
			this.hasCreatedBy = (flags & HEADER_BITMASK_CREATED_BY) != 0;
		}

		private void ReadLanguagePreference ( BufferStream readBuffer )
		{
			if (this.hasLanguagePreference) {
				string countryCode = readBuffer.ReadUTF8Encodedstring();
				if (countryCode.Length != LANGUAGE_PREFERENCE_LENGTH) {
					throw new System.IO.InvalidDataException("invalid language preference: " + countryCode);
				}
				this.languagePreference = countryCode;
			}
		}

		private void ReadMapStartPosition ( BufferStream readBuffer )
		{
			if (this.hasStartPosition) {
				double mapStartLatitude = CoordinatesUtil.microdegreesToDegrees(readBuffer.ReadInt());
				double mapStartLongitude = CoordinatesUtil.microdegreesToDegrees(readBuffer.ReadInt());
				try {
					this.startPosition = new GeoPoint(mapStartLatitude, mapStartLongitude);
				} catch (ArgumentException e) {
					throw new System.IO.InvalidDataException(e.Message);
				}
			}
		}

		private void ReadMapStartZoomLevel ( BufferStream readBuffer )
		{
			if (this.hasStartZoomLevel) {
				// get and check the start zoom level (1 byte)
				byte mapStartZoomLevel = (byte)readBuffer.ReadByte();
				if (mapStartZoomLevel < 0 || mapStartZoomLevel > START_ZOOM_LEVEL_MAX) {
					throw new System.IO.InvalidDataException("invalid map start zoom level: " + mapStartZoomLevel);
				}

				this.startZoomLevel = mapStartZoomLevel;
			}
		}

		private void ReadOptionalFields ( BufferStream readBuffer )
		{
			ReadMapStartPosition(readBuffer);
			ReadMapStartZoomLevel(readBuffer);
			ReadLanguagePreference(readBuffer);
			if (this.hasComment) {
				this.comment = readBuffer.ReadUTF8Encodedstring();
			}

			if (this.hasCreatedBy) {
				this.createdBy = readBuffer.ReadUTF8Encodedstring();
			}
		}
	}
}
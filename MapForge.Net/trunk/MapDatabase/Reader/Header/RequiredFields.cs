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
using System;
namespace MapDB.Reader.Header
{
	internal static class RequiredFields
	{
		/**
		 * Magic byte at the beginning of a valid binary map file.
		 */
		private const string FileHeaderDeclaration = "mapsforge binary OSM";

		/**
		 * Maximum size of the file header in bytes.
		 */
		private const int MaxHeaderSize = 1000000;

		/**
		 * Minimum size of the file header in bytes.
		 */
		private const int MinHeaderSize = 70;

		/**
		 * Version of the map file format which is supported by this implementation.
		 */
		private const int SupportedFileVersion = 3;

		public static void ReadBoundingBox ( BufferStream readBuffer, MapFileInfoBuilder mapFileInfoBuilder )
		{
			double minLatitude = CoordinatesUtil.microdegreesToDegrees(readBuffer.ReadInt());
			double minLongitude = CoordinatesUtil.microdegreesToDegrees(readBuffer.ReadInt());
			double maxLatitude = CoordinatesUtil.microdegreesToDegrees(readBuffer.ReadInt());
			double maxLongitude = CoordinatesUtil.microdegreesToDegrees(readBuffer.ReadInt());

			try {
				mapFileInfoBuilder.boundingBox = new BoundingBox(minLatitude, minLongitude, maxLatitude, maxLongitude);
			} catch (ArgumentException e) {
				throw new System.IO.InvalidDataException(e.Message, e);
			}
		}

		public static void ReadFileSize ( BufferStream readBuffer, long fileSize, MapFileInfoBuilder mapFileInfoBuilder )
		{
			// get and check the file size (8 bytes)
			long headerFileSize = readBuffer.ReadLong();
			if (headerFileSize != fileSize) {
				throw new System.IO.InvalidDataException("invalid file size: " + headerFileSize);
			}
			mapFileInfoBuilder.fileSize = fileSize;
		}

		public static void ReadFileVersion ( BufferStream readBuffer, MapFileInfoBuilder mapFileInfoBuilder )
		{
			// get and check the file version (4 bytes)
			int fileVersion = readBuffer.ReadInt();
			if (fileVersion != SupportedFileVersion) {
				throw new System.IO.InvalidDataException("unsupported file version: " + fileVersion);
			}
			mapFileInfoBuilder.fileVersion = fileVersion;
		}

		public static void ReadMagicbyte ( BufferStream readBuffer )
		{
			// read the the magic byte and the file header size into the buffer
			int magicbyteLength = FileHeaderDeclaration.Length;
			if (!readBuffer.ReadFromFile(magicbyteLength + 4)) {
				throw new System.IO.InvalidDataException("reading magic byte has failed");
			}

			// get and check the magic byte
			string magicbyte = readBuffer.ReadUTF8Encodedstring(magicbyteLength);
			if (FileHeaderDeclaration != magicbyte) {
				throw new System.IO.InvalidDataException("invalid magic byte: " + magicbyte);
			}
		}

		public static void ReadMapDate ( BufferStream readBuffer, MapFileInfoBuilder mapFileInfoBuilder )
		{
			// get and check the the map date (8 bytes)
			DateTime mapDate = readBuffer.ReadDateTime();
			// is the map date before 2010-01-10 ?
			if (mapDate < new DateTime(2008,1,1)) {
				throw new System.IO.InvalidDataException("invalid map date: " + mapDate);
			}
			mapFileInfoBuilder.mapDate = mapDate;
		}

		public static void ReadProjectionName ( BufferStream readBuffer, MapFileInfoBuilder mapFileInfoBuilder )
		{
			// get and check the projection name
			string projectionName = readBuffer.ReadUTF8Encodedstring();
			mapFileInfoBuilder.projectionName = projectionName;
		}

		public static void ReadRemainingHeader ( BufferStream readBuffer )
		{
			// get and check the size of the remaining file header (4 bytes)
			int remainingHeaderSize = readBuffer.ReadInt();
			if (remainingHeaderSize < MinHeaderSize || remainingHeaderSize > MaxHeaderSize) {
				throw new System.IO.InvalidDataException("invalid remaining header size: " + remainingHeaderSize);
			}

			// read the header data into the buffer
			if (!readBuffer.ReadFromFile(remainingHeaderSize)) {
				throw new System.IO.InvalidDataException("reading header data has failed: " + remainingHeaderSize);
			}
		}

		public static void ReadTilePixelSize ( BufferStream readBuffer, MapFileInfoBuilder mapFileInfoBuilder )
		{
			// get and check the tile pixel size (2 bytes)
			int tilePixelSize = readBuffer.Readshort();
			if (tilePixelSize != 256) {
				throw new System.IO.InvalidDataException("unsupported tile pixel size: " + tilePixelSize);
			}
			mapFileInfoBuilder.tilePixelSize = tilePixelSize;
		}

		public static void ReadNodeTags ( BufferStream readBuffer, MapFileInfoBuilder mapFileInfoBuilder )
		{
			ReadTags(readBuffer, mapFileInfoBuilder, out mapFileInfoBuilder.nodeTags);
		}

		public static void ReadWayTags ( BufferStream readBuffer, MapFileInfoBuilder mapFileInfoBuilder )
		{
			ReadTags(readBuffer, mapFileInfoBuilder, out mapFileInfoBuilder.wayTags);
		}

		private static void ReadTags ( BufferStream readBuffer, MapFileInfoBuilder mapFileInfoBuilder, out Tag[] tags )
		{
			// get and check the number of way tags (2 bytes)
			int numberOfTags = readBuffer.Readshort();
			if (numberOfTags < 0) {
				throw new System.IO.InvalidDataException("invalid number of tags: " + numberOfTags);
			}

			tags = new Tag[numberOfTags];

			for (int currentTagId = 0; currentTagId < numberOfTags; ++currentTagId) {
				// get and check the way tag
				string tag = readBuffer.ReadUTF8Encodedstring();
				if (tag == null) {
					tags = null;
					throw new System.IO.InvalidDataException("tag must not be null: " + currentTagId);
				}
				tags[currentTagId] = new Tag(tag);
			}
			mapFileInfoBuilder.wayTags = tags;
		}

	}
}
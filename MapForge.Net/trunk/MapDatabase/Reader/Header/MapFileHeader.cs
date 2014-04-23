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
using MapCore.Projections;
namespace MapDB.Reader.Header
{

	/**
	 * Reads and validates the header data from a binary map file.
	 */
	public class MapFileHeader
	{
		/**
		 * Maximum valid base zoom level of a sub-file.
		 */
		private const int BASE_ZOOM_LEVEL_MAX = 20;

		/**
		 * Minimum size of the file header in bytes.
		 */
		private const int HEADER_SIZE_MIN = 70;

		/**
		 * Length of the debug signature at the beginning of the index.
		 */
		private const byte SIGNATURE_LENGTH_INDEX = 16;

		/**
		 * A single whitespace character.
		 */
		private const char SPACE = ' ';

		private MapFileInfo mapFileInfo;
		private SubFileParameter[] subFileParameters;
		private byte zoomLevelMaximum;
		private byte zoomLevelMinimum;

		/**
		 * @return a MapFileInfo containing the header data.
		 */
		public MapFileInfo MapFileInfo { get { return this.mapFileInfo; } }

		/**
		 * @param zoomLevel
		 *            the originally requested zoom level.
		 * @return the closest possible zoom level which is covered by a sub-file.
		 */
		public byte getQueryZoomLevel ( byte zoomLevel )
		{
			if (zoomLevel > this.zoomLevelMaximum) {
				return this.zoomLevelMaximum;
			} else if (zoomLevel < this.zoomLevelMinimum) {
				return this.zoomLevelMinimum;
			}
			return zoomLevel;
		}

		/**
		 * @param queryZoomLevel
		 *            the zoom level for which the sub-file parameters are needed.
		 * @return the sub-file parameters for the given zoom level.
		 */
		public SubFileParameter getSubFileParameter ( int queryZoomLevel )
		{
			return this.subFileParameters[queryZoomLevel];
		}

		/**
		 * Reads and validates the header block from the map file.
		 * 
		 * @param readBuffer
		 *            the ReadBuffer for the file data.
		 * @param fileSize
		 *            the size of the map file in bytes.
		 * @return a FileOpenResult containing an error message in case of a failure.
		 * @throws IOException
		 *             if an error occurs while reading the file.
		 */
		public void ReadHeader ( BufferStream readBuffer, long fileSize )
		{
			RequiredFields.ReadMagicbyte(readBuffer);
			RequiredFields.ReadRemainingHeader(readBuffer);
			MapFileInfoBuilder mapFileInfoBuilder = new MapFileInfoBuilder();
			RequiredFields.ReadFileVersion(readBuffer, mapFileInfoBuilder);
			RequiredFields.ReadFileSize(readBuffer, fileSize, mapFileInfoBuilder);
			RequiredFields.ReadMapDate(readBuffer, mapFileInfoBuilder);
			RequiredFields.ReadBoundingBox(readBuffer, mapFileInfoBuilder);
			RequiredFields.ReadTilePixelSize(readBuffer, mapFileInfoBuilder);
			RequiredFields.ReadProjectionName(readBuffer, mapFileInfoBuilder);
			mapFileInfoBuilder.Projection = new RepresentationConverter(Projections.GetProjection(mapFileInfoBuilder.projectionName), mapFileInfoBuilder.tilePixelSize);
			OptionalFields.readOptionalFields(readBuffer, mapFileInfoBuilder);
			RequiredFields.ReadNodeTags(readBuffer, mapFileInfoBuilder);
			RequiredFields.ReadWayTags(readBuffer, mapFileInfoBuilder);
			ReadSubFileParameters(readBuffer, fileSize, mapFileInfoBuilder);
			this.mapFileInfo = mapFileInfoBuilder.build();
		}

		private void ReadSubFileParameters ( BufferStream readBuffer, long fileSize, MapFileInfoBuilder mapFileInfoBuilder )
		{
			// get and check the number of sub-files (1 byte)
			byte numberOfSubFiles = (byte)readBuffer.ReadByte();
			if (numberOfSubFiles < 1) {
				throw new System.IO.InvalidDataException("invalid number of sub-files: " + numberOfSubFiles);
			}
			mapFileInfoBuilder.numberOfSubFiles = numberOfSubFiles;

			SubFileParameter[] tempSubFileParameters = new SubFileParameter[numberOfSubFiles];
			this.zoomLevelMinimum = byte.MaxValue;
			this.zoomLevelMaximum = byte.MinValue;

			// get and check the information for each sub-file
			for (byte currentSubFile = 0; currentSubFile < numberOfSubFiles; ++currentSubFile) {
				SubFileParameterBuilder subFileParameterBuilder = new SubFileParameterBuilder();

				// get and check the base zoom level (1 byte)
				byte baseZoomLevel = (byte)readBuffer.ReadByte();
				if (baseZoomLevel < 0 || baseZoomLevel > BASE_ZOOM_LEVEL_MAX) {
					throw new System.IO.InvalidDataException("invalid base zooom level: " + baseZoomLevel);
				}
				subFileParameterBuilder.baseZoomLevel = baseZoomLevel;

				// get and check the minimum zoom level (1 byte)
				byte zoomLevelMin = (byte)readBuffer.ReadByte();
				if (zoomLevelMin < 0 || zoomLevelMin > 22) {
					throw new System.IO.InvalidDataException("invalid minimum zoom level: " + zoomLevelMin);
				}
				subFileParameterBuilder.zoomLevelMin = zoomLevelMin;

				// get and check the maximum zoom level (1 byte)
				byte zoomLevelMax = (byte)readBuffer.ReadByte();
				if (zoomLevelMax < 0 || zoomLevelMax > 22) {
					throw new System.IO.InvalidDataException("invalid maximum zoom level: " + zoomLevelMax);
				}
				subFileParameterBuilder.zoomLevelMax = zoomLevelMax;

				// check for valid zoom level range
				if (zoomLevelMin > zoomLevelMax) {
					throw new System.IO.InvalidDataException("invalid zoom level range: " + zoomLevelMin + SPACE + zoomLevelMax);
				}

				// get and check the start address of the sub-file (8 bytes)
				long startAddress = readBuffer.ReadLong();
				if (startAddress < HEADER_SIZE_MIN || startAddress >= fileSize) {
					throw new System.IO.InvalidDataException("invalid start address: " + startAddress);
				}
				subFileParameterBuilder.startAddress = startAddress;

				long indexStartAddress = startAddress;
				if (mapFileInfoBuilder.optionalFields.isDebugFile) {
					// the sub-file has an index signature before the index
					indexStartAddress += SIGNATURE_LENGTH_INDEX;
				}
				subFileParameterBuilder.indexStartAddress = indexStartAddress;

				// get and check the size of the sub-file (8 bytes)
				long subFileSize = readBuffer.ReadLong();
				if (subFileSize < 1) {
					throw new System.IO.InvalidDataException("invalid sub-file size: " + subFileSize);
				}
				subFileParameterBuilder.subFileSize = subFileSize;

				subFileParameterBuilder.boundingBox = mapFileInfoBuilder.boundingBox;

				// add the current sub-file to the list of sub-files
				tempSubFileParameters[currentSubFile] = subFileParameterBuilder.build(mapFileInfoBuilder.Projection);

				updateZoomLevelInformation(tempSubFileParameters[currentSubFile]);
			}

			// create and fill the lookup table for the sub-files
			this.subFileParameters = new SubFileParameter[this.zoomLevelMaximum + 1];
			for (int currentMapFile = 0; currentMapFile < numberOfSubFiles; ++currentMapFile) {
				SubFileParameter subFileParameter = tempSubFileParameters[currentMapFile];
				for (byte zoomLevel = subFileParameter.ZoomLevelMin; zoomLevel <= subFileParameter.ZoomLevelMax; ++zoomLevel) {
					this.subFileParameters[zoomLevel] = subFileParameter;
				}
			}
		}

		private void updateZoomLevelInformation ( SubFileParameter subFileParameter )
		{
			// update the global minimum and maximum zoom level information
			if (this.zoomLevelMinimum > subFileParameter.ZoomLevelMin) {
				this.zoomLevelMinimum = subFileParameter.ZoomLevelMin;
			}
			if (this.zoomLevelMaximum < subFileParameter.ZoomLevelMax) {
				this.zoomLevelMaximum = subFileParameter.ZoomLevelMax;
			}
		}
	}
}
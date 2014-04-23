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
using System.Collections.Generic;
using System.IO;
using MapCore;
using MapCore.Model;
using MapCore.Projections;
using MapCore.Util;
using MapDB.Reader.Header;
using MapForgeDb.Reader;

namespace MapDB.Reader
{
	/**
	 * A class for reading binary map files.
	 * <p>
	 * This class is not thread-safe. Each thread should use its own instance.
	 * 
	 * @see <a href="https://code.google.com/p/mapsforge/wiki/SpecificationBinaryMapFile">Specification</a>
	 */
	public class MapDatabase : IDisposable, IVectorMapProvider 
	{
		public RepresentationConverter Projection { get { return MapFileInfo.Projection; } }

		/**
		 * Bitmask to extract the block offset from an index entry.
		 */
		private const long BITMASK_INDEX_OFFSET = 0x7FFFFFFFFFL;

		/**
		 * Bitmask to extract the water information from an index entry.
		 */
		private const long BITMASK_INDEX_WATER = 0x8000000000L;

		/**
		 * Debug message prefix for the block signature.
		 */
		private const string DEBUG_SIGNATURE_BLOCK = "block signature: ";

		/**
		 * Debug message prefix for the NODE signature.
		 */
		private const string DEBUG_SIGNATURE_NODE = "Node signature: ";

		/**
		 * Debug message prefix for the way signature.
		 */
		private const string DEBUG_SIGNATURE_WAY = "way signature: ";

		/**
		 * Amount of cache blocks that the index cache should store.
		 */
		private const int INDEX_CACHE_SIZE = 64;

		/**
		 * Error message for an invalid first way offset.
		 */
		private const string INVALID_FIRST_WAY_OFFSET = "invalid first way offset: ";

		/**
		 * Maximum way nodes sequence length which is considered as valid.
		 */
		private const int MAXIMUM_WAY_NODES_SEQUENCE_LENGTH = 8192;

		/**
		 * Maximum number of map objects in the zoom table which is considered as valid.
		 */
		private const int MAXIMUM_ZOOM_TABLE_OBJECTS = 65536;

		/**
		 * Bitmask for the optional NODE feature "elevation".
		 */
		private const int NODE_FEATURE_ELEVATION = 0x20;

		/**
		 * Bitmask for the optional NODE feature "house number".
		 */
		private const int NODE_FEATURE_HOUSE_NUMBER = 0x40;

		/**
		 * Bitmask for the optional NODE feature "name".
		 */
		private const int NODE_FEATURE_NAME = 0x80;

		/**
		 * Bitmask for the NODE layer.
		 */
		private const int NODE_LAYER_BITMASK = 0xf0;

		/**
		 * Bit shift for calculating the NODE layer.
		 */
		private const int NODE_LAYER_SHIFT = 4;

		/**
		 * Bitmask for the number of NODE tags.
		 */
		private const int NODE_NUMBER_OF_TAGS_BITMASK = 0x0f;

		private const string READ_ONLY_MODE = "r";

		/**
		 * Length of the debug signature at the beginning of each block.
		 */
		private const byte SIGNATURE_LENGTH_BLOCK = 32;

		/**
		 * Length of the debug signature at the beginning of each NODE.
		 */
		private const byte SIGNATURE_LENGTH_NODE = 32;

		/**
		 * Length of the debug signature at the beginning of each way.
		 */
		private const byte SIGNATURE_LENGTH_WAY = 32;

		/**
		 * The key of the elevation OpenStreetMap tag.
		 */
		private const string TAG_KEY_ELE = "ele";

		/**
		 * The key of the house number OpenStreetMap tag.
		 */
		private const string TAG_KEY_HOUSE_NUMBER = "addr:housenumber";

		/**
		 * The key of the name OpenStreetMap tag.
		 */
		private const string TAG_KEY_NAME = "name";

		/**
		 * The key of the reference OpenStreetMap tag.
		 */
		private const string TAG_KEY_REF = "ref";

		/**
		 * Bitmask for the optional way data blocks byte.
		 */
		private const int WAY_FEATURE_DATA_BLOCKS_byte = 0x08;

		/**
		 * Bitmask for the optional way double delta encoding.
		 */
		private const int WAY_FEATURE_DOUBLE_DELTA_ENCODING = 0x04;

		/**
		 * Bitmask for the optional way feature "house number".
		 */
		private const int WAY_FEATURE_HOUSE_NUMBER = 0x40;

		/**
		 * Bitmask for the optional way feature "label position".
		 */
		private const int WAY_FEATURE_LABEL_POSITION = 0x10;

		/**
		 * Bitmask for the optional way feature "name".
		 */
		private const int WAY_FEATURE_NAME = 0x80;

		/**
		 * Bitmask for the optional way feature "reference".
		 */
		private const int WAY_FEATURE_REF = 0x20;

		/**
		 * Bitmask for the way layer.
		 */
		private const int WAY_LAYER_BITMASK = 0xf0;

		/**
		 * Bit shift for calculating the way layer.
		 */
		private const int WAY_LAYER_SHIFT = 4;

		/**
		 * Bitmask for the number of way tags.
		 */
		private const int WAY_NUMBER_OF_TAGS_BITMASK = 0x0f;

		private IndexCache databaseIndexCache;
		private long fileSize;
		private Stream inputFile;
		private MapFileHeader mapFileHeader;
		private BufferStream readBuffer;
		private string signatureBlock;
		private string signatureNode;
		private string signatureWay;

		private Tile tile;
		private GeoPoint tilePosition;

		public MapDatabase ()
		{
		}

		public MapDatabase ( string filename ) : this (new FileInfo(filename))
		{
		}
		public MapDatabase ( FileInfo fileinfo )
		{
			this.OpenFile(fileinfo);
		}

		/**
		 * Closes the map file and destroys all internal caches. Has no effect if no map file is currently opened.
		 */
		public void CloseFile ()
		{
			this.mapFileHeader = null;

			if (this.databaseIndexCache != null) {
				this.databaseIndexCache = null;
			}

			if (this.inputFile != null) {
				this.inputFile.Close();
				this.inputFile = null;
			}

			this.readBuffer = null;
		}

		/**
		 * @return the metadata for the current map file.
		 * @throws InvalidOperationException
		 *             if no map is currently opened.
		 */
		public MapFileInfo MapFileInfo
		{
			get
			{
				if (this.mapFileHeader == null) {
					throw new InvalidOperationException("no map file is currently opened");
				}
				return this.mapFileHeader.MapFileInfo;
			}
		}

		/**
		 * @return true if a map file is currently opened, false otherwise.
		 */
		public bool HasOpenFile { get { return this.inputFile != null; } }

		public void OpenFile ( string filename )
		{
			OpenFile(new FileInfo(filename));
		}

		/**
		 * Opens the given map file, reads its header data and validates them.
		 * 
		 * @param mapFile
		 *            the map file.
		 * @return a FileOpenResult containing an error message in case of a failure.
		 * @throws ArgumentException
		 *             if the given map file is null.
		 */
		public void OpenFile ( FileInfo mapFile )
		{
			try {
				if (mapFile == null) {
					throw new ArgumentException("mapFile must not be null", "mapFile");
				}

				// make sure to close any previously opened file first
				CloseFile();

				// check if the file exists and is readable
				if (!mapFile.Exists) {
					throw new System.IO.InvalidDataException("file does not exist: " + mapFile);
				}

				// open the file in read only mode
				this.inputFile = mapFile.OpenRead();
				this.fileSize = this.inputFile.Length;

				this.readBuffer = new BufferStream(this.inputFile);
				this.mapFileHeader = new MapFileHeader();
				this.mapFileHeader.ReadHeader(this.readBuffer, this.fileSize);

			} catch (IOException e) {
				// make sure that the file is closed
				CloseFile();
				throw new System.IO.InvalidDataException(e.Message);
			}
		}

		public IMapReadResult ReadMapData ( GeoPoint GeoPoint, byte zoomLevel )
		{
			Tile tile = Projection.GeoPointToTile(GeoPoint, zoomLevel);
			return ReadMapData(tile);
		}

		/**
		 * Reads all map data for the area covered by the given tile at the tile zoom level.
		 * 
		 * @param tile
		 *            defines area and zoom level of read map data.
		 * @return the read map data.
		 */
		public IMapReadResult ReadMapData ( Tile tile )
		{
			try {
				prepareExecution();
				QueryParameters queryParameters = new QueryParameters();
				queryParameters.queryZoomLevel = this.mapFileHeader.getQueryZoomLevel(tile.ZoomFactor);

				// get and check the sub-file for the query zoom level
				SubFileParameter subFileParameter = this.mapFileHeader.getSubFileParameter(queryParameters.queryZoomLevel);
				if (subFileParameter == null) {
					return null;
				}

				QueryCalculations.calculateBaseTiles(queryParameters, tile, subFileParameter);
				QueryCalculations.calculateBlocks(queryParameters, subFileParameter);

				return processBlocks(queryParameters, subFileParameter);
			} catch (IOException e) {
				return null;
			}
		}

		private GeoPointList decodeWayNodesDoubleDelta ( int numberOfWayNodes )
		{
			GeoPointList waySegment = new GeoPointList(numberOfWayNodes);

			// get the first way node latitude offset (VBE-S)
			double wayNodeLatitude = this.tilePosition.Latitude
					+ CoordinatesUtil.microdegreesToDegrees(this.readBuffer.ReadSignedInt());

			// get the first way node longitude offset (VBE-S)
			double wayNodeLongitude = this.tilePosition.Longitude
					+ CoordinatesUtil.microdegreesToDegrees(this.readBuffer.ReadSignedInt());

			if (wayNodeLatitude > 90) wayNodeLatitude = 90;
			if (wayNodeLatitude < -90) wayNodeLatitude = -90;
			// store the first way node
			waySegment.Add(new GeoPoint(wayNodeLatitude, wayNodeLongitude));

			double previousSingleDeltaLatitude = 0;
			double previousSingleDeltaLongitude = 0;

			for (int wayNodesIndex = 1; wayNodesIndex < numberOfWayNodes; ++wayNodesIndex) {
				// get the way node latitude double-delta offset (VBE-S)
				double doubleDeltaLatitude = CoordinatesUtil.microdegreesToDegrees(this.readBuffer.ReadSignedInt());

				// get the way node longitude double-delta offset (VBE-S)
				double doubleDeltaLongitude = CoordinatesUtil.microdegreesToDegrees(this.readBuffer.ReadSignedInt());

				double singleDeltaLatitude = doubleDeltaLatitude + previousSingleDeltaLatitude;
				double singleDeltaLongitude = doubleDeltaLongitude + previousSingleDeltaLongitude;

				wayNodeLatitude = wayNodeLatitude + singleDeltaLatitude;
				wayNodeLongitude = wayNodeLongitude + singleDeltaLongitude;

				if (wayNodeLatitude > 90) wayNodeLatitude = 90;
				if (wayNodeLatitude < -90) wayNodeLatitude = -90;
				waySegment.Add(new GeoPoint(wayNodeLatitude, wayNodeLongitude));

				previousSingleDeltaLatitude = singleDeltaLatitude;
				previousSingleDeltaLongitude = singleDeltaLongitude;
			}
			return waySegment;
		}

		private GeoPointList DecodeWayNodesSingleDelta ( int numberOfWayNodes )
		{
			GeoPointList waySegment = new GeoPointList(numberOfWayNodes);
			
			// get the first way node latitude single-delta offset (VBE-S)
			double wayNodeLatitude = this.tilePosition.Latitude
					+ CoordinatesUtil.microdegreesToDegrees(this.readBuffer.ReadSignedInt());

			// get the first way node longitude single-delta offset (VBE-S)
			double wayNodeLongitude = this.tilePosition.Longitude
					+ CoordinatesUtil.microdegreesToDegrees(this.readBuffer.ReadSignedInt());

			// store the first way node
			if (wayNodeLatitude > 90) wayNodeLatitude = 90;
			if (wayNodeLatitude < -90) wayNodeLatitude = -90;
			if (wayNodeLongitude < -180) wayNodeLongitude = -180;
			if (wayNodeLongitude > 180) wayNodeLongitude = 180;
			waySegment.Add(new GeoPoint(wayNodeLatitude, wayNodeLongitude));

			for (int wayNodesIndex = 1; wayNodesIndex < numberOfWayNodes; ++wayNodesIndex) {
				// get the way node latitude offset (VBE-S)
				wayNodeLatitude = wayNodeLatitude + CoordinatesUtil.microdegreesToDegrees(this.readBuffer.ReadSignedInt());

				// get the way node longitude offset (VBE-S)
				wayNodeLongitude = wayNodeLongitude
						+ CoordinatesUtil.microdegreesToDegrees(this.readBuffer.ReadSignedInt());
				if (wayNodeLatitude > 90) wayNodeLatitude = 90;
				if (wayNodeLatitude < -90) wayNodeLatitude = -90;

				waySegment.Add(new GeoPoint(wayNodeLatitude, wayNodeLongitude));
			}
			return waySegment;
		}

		private void prepareExecution ()
		{
			if (this.databaseIndexCache == null) {
				this.databaseIndexCache = new IndexCache(this.inputFile, INDEX_CACHE_SIZE);
			}
		}

		private NodeWayBundle ProcessBlock ( QueryParameters queryParameters, SubFileParameter subFileParameter )
		{
			if (!processBlockSignature()) {
				return null;
			}

			ZoomTable zoomTable = readZoomTable(subFileParameter);
			if (zoomTable == null) {
				return null;
			}
			int zoomTableRow = queryParameters.queryZoomLevel - subFileParameter.ZoomLevelMin;
			int nodesOnQueryZoomLevel = zoomTable[zoomTableRow].NodesCount;
			int waysOnQueryZoomLevel = zoomTable[zoomTableRow].WaysCount;

			// get the relative offset to the first stored way in the block
			long firstWayOffset = this.readBuffer.ReadUnsignedInt();
			if (firstWayOffset < 0) {
				return null;
			}

			// add the current buffer position to the relative first way offset
			firstWayOffset += this.readBuffer.Position;
			if (firstWayOffset > this.readBuffer.Length) {
				return null;
			}

			List<Node> nodes = processNodes(nodesOnQueryZoomLevel);
			if (nodes == null) {
				return null;
			}

			// finished reading nodes, check if the current buffer position is valid
			if (this.readBuffer.Position > firstWayOffset) {
				return null;
			}

			// move the pointer to the first way
			this.readBuffer.Position = firstWayOffset;

			List<Way> ways = processWays(queryParameters, waysOnQueryZoomLevel);
			if (ways == null) {
				return null;
			}

			return new NodeWayBundle(this.tile, nodes, ways);
		}

		private MapReadResult processBlocks ( QueryParameters queryParameters, SubFileParameter subFileParameter )
		{
			bool queryIsWater = true;
			bool queryReadWaterInfo = false;

			MapReadResultBuilder mapReadResultBuilder = new MapReadResultBuilder();

			// read and process all blocks from top to bottom and from left to right
			for (long row = queryParameters.fromBlockY; row <= queryParameters.toBlockY; ++row) {
				for (long column = queryParameters.fromBlockX; column <= queryParameters.toBlockX; ++column) {
					// calculate the actual block number of the needed block in the file
					long blockNumber = row * subFileParameter.BlocksWidth + column;

					// get the current index entry
					long currentBlockIndexEntry = this.databaseIndexCache.getIndexEntry(subFileParameter, blockNumber);

					// check if the current query would still return a water tile
					if (queryIsWater) {
						// check the water flag of the current block in its index entry
						queryIsWater &= (currentBlockIndexEntry & BITMASK_INDEX_WATER) != 0;
						queryReadWaterInfo = true;
					}

					// get and check the current block pointer
					long currentBlockpointer = currentBlockIndexEntry & BITMASK_INDEX_OFFSET;
					if (currentBlockpointer < 1 || currentBlockpointer > subFileParameter.SubFileSize) {
						System.Diagnostics.Debug.WriteLine("invalid current block pointer: " + currentBlockpointer);
						System.Diagnostics.Debug.WriteLine("subFileSize: " + subFileParameter.SubFileSize);
						return null;
					}

					long nextBlockpointer;
					// check if the current block is the last block in the file
					if (blockNumber + 1 == subFileParameter.NumberOfBlocks) {
						// set the next block pointer to the end of the file
						nextBlockpointer = subFileParameter.SubFileSize;
					} else {
						// get and check the next block pointer
						nextBlockpointer = this.databaseIndexCache.getIndexEntry(subFileParameter, blockNumber + 1)
								& BITMASK_INDEX_OFFSET;
						if (nextBlockpointer > subFileParameter.SubFileSize) {
							System.Diagnostics.Debug.WriteLine("invalid next block pointer: " + nextBlockpointer);
							System.Diagnostics.Debug.WriteLine("sub-file size: " + subFileParameter.SubFileSize);
							return null;
						}
					}

					// calculate the size of the current block
					int currentBlockSize = (int)(nextBlockpointer - currentBlockpointer);
					if (currentBlockSize < 0) {
						System.Diagnostics.Debug.WriteLine("current block size must not be negative: " + currentBlockSize);
						return null;
					} else if (currentBlockSize == 0) {
						// the current block is empty, continue with the next block
						continue;
					} else if (currentBlockSize > BufferStream.MAXIMUM_BUFFER_SIZE) {
						// the current block is too large, continue with the next block
						System.Diagnostics.Debug.WriteLine("current block size too large: " + currentBlockSize);
						continue;
					} else if (currentBlockpointer + currentBlockSize > this.fileSize) {
						System.Diagnostics.Debug.WriteLine("current block largher than file size: " + currentBlockSize);
						return null;
					}

					// seek to the current block in the map file
					this.inputFile.Seek(subFileParameter.StartAddress + currentBlockpointer, SeekOrigin.Begin);

					// read the current block into the buffer
					if (!this.readBuffer.ReadFromFile(currentBlockSize)) {
						// skip the current block
						System.Diagnostics.Debug.WriteLine("reading current block has failed: " + currentBlockSize);
						return null;
					}

					// calculate the top-left coordinates of the underlying tile
					this.tile = new Tile(
						subFileParameter.BoundaryTileLeft + column,
						subFileParameter.BoundaryTileTop + row,
						subFileParameter.BaseZoomLevel, 
						Projection.TileSize
					);
					this.tilePosition = Projection.MappointToGeoPoint(tile.MapPoint1);

					try {
						NodeWayBundle nodeWayBundle = ProcessBlock(queryParameters, subFileParameter);
						mapReadResultBuilder.Add(nodeWayBundle);
					} catch (IndexOutOfRangeException e) {
						System.Diagnostics.Debug.WriteLine(e.Message);
					}
				}
			}

			// the query is finished, was the water flag set for all blocks?
			if (queryIsWater && queryReadWaterInfo) {
				mapReadResultBuilder.isWater = true;
			}

			return mapReadResultBuilder.build();
		}

		/**
		 * Processes the block signature, if present.
		 * 
		 * @return true if the block signature could be processed successfully, false otherwise.
		 */
		private bool processBlockSignature ()
		{
			if (this.mapFileHeader.MapFileInfo.DebugFile) {
				// get and check the block signature
				this.signatureBlock = this.readBuffer.ReadUTF8Encodedstring(SIGNATURE_LENGTH_BLOCK);
				if (!this.signatureBlock.StartsWith("###TileStart")) {
					System.Diagnostics.Debug.WriteLine("invalid block signature: " + this.signatureBlock);
					return false;
				}
			}
			return true;
		}

		private List<Node> processNodes ( int numberOfNodes )
		{
			List<Node> nodes = new List<Node>();
			Tag[] NODETags = this.mapFileHeader.MapFileInfo.NodeTags;

			for (int elementCounter = numberOfNodes; elementCounter != 0; --elementCounter) {
				if (this.mapFileHeader.MapFileInfo.DebugFile) {
					// get and check the NODE signature
					this.signatureNode = this.readBuffer.ReadUTF8Encodedstring(SIGNATURE_LENGTH_NODE);
					if (!this.signatureNode.StartsWith("***nodestart")) {
						System.Diagnostics.Debug.WriteLine("invalid NODE signature: " + this.signatureNode);
						System.Diagnostics.Debug.WriteLine(DEBUG_SIGNATURE_BLOCK + this.signatureBlock);
						return null;
					}
				}

				// get the NODE latitude offset (VBE-S)
				double latitude = this.tilePosition.Latitude
						+ CoordinatesUtil.microdegreesToDegrees(this.readBuffer.ReadSignedInt());

				// get the NODE longitude offset (VBE-S)
				double longitude = this.tilePosition.Longitude
						+ CoordinatesUtil.microdegreesToDegrees(this.readBuffer.ReadSignedInt());

				// get the special byte which encodes multiple flags
				byte specialbyte = (byte)this.readBuffer.ReadByte();

				// bit 1-4 represent the layer
				byte layer = (byte)((specialbyte & NODE_LAYER_BITMASK) >> NODE_LAYER_SHIFT);
				// bit 5-8 represent the number of tag IDs
				byte numberOfTags = (byte)(specialbyte & NODE_NUMBER_OF_TAGS_BITMASK);

				List<Tag> tags = new List<Tag>();

				// get the tag IDs (VBE-U)
				for (byte tagIndex = numberOfTags; tagIndex != 0; --tagIndex) {
					int tagId = this.readBuffer.ReadUnsignedInt();
					if (tagId < 0 || tagId >= NODETags.Length) {
						System.Diagnostics.Debug.WriteLine("invalid node tag ID: " + tagId);
						if (this.mapFileHeader.MapFileInfo.DebugFile) {
							System.Diagnostics.Debug.WriteLine(DEBUG_SIGNATURE_NODE + this.signatureNode);
							System.Diagnostics.Debug.WriteLine(DEBUG_SIGNATURE_BLOCK + this.signatureBlock);
						}
						return null;
					}
					tags.Add(NODETags[tagId]);
				}

				// get the feature bitmask (1 byte)
				byte featurebyte = (byte)this.readBuffer.ReadByte();

				// bit 1-3 enable optional features
				bool featureName = (featurebyte & NODE_FEATURE_NAME) != 0;
				bool featureHouseNumber = (featurebyte & NODE_FEATURE_HOUSE_NUMBER) != 0;
				bool featureElevation = (featurebyte & NODE_FEATURE_ELEVATION) != 0;

				// check if the NODE has a name
				if (featureName) {
					tags.Add(new Tag(TAG_KEY_NAME, this.readBuffer.ReadUTF8Encodedstring()));
				}

				// check if the NODE has a house number
				if (featureHouseNumber) {
					tags.Add(new Tag(TAG_KEY_HOUSE_NUMBER, this.readBuffer.ReadUTF8Encodedstring()));
				}

				// check if the NODE has an elevation
				if (featureElevation) {
					tags.Add(new Tag(TAG_KEY_ELE, this.readBuffer.ReadSignedInt().ToString()));
				}

				if (latitude <90 && latitude > -90) 
					nodes.Add(new Node(layer, tags, new GeoPoint(latitude, longitude)));
			}

			return nodes;
		}

		private GeoPointList2 processWayDataBlock ( bool doubleDeltaEncoding )
		{
			// get and check the number of way coordinate blocks (VBE-U)
			int numberOfWayCoordinateBlocks = this.readBuffer.ReadUnsignedInt();
			if (numberOfWayCoordinateBlocks < 1 || numberOfWayCoordinateBlocks > short.MaxValue) {
				System.Diagnostics.Debug.WriteLine("invalid number of way coordinate blocks: " + numberOfWayCoordinateBlocks);
				return null;
			}

			// create the array which will store the different way coordinate blocks
			GeoPointList2 wayCoordinates = new GeoPointList2(numberOfWayCoordinateBlocks);

			// read the way coordinate blocks
			for (int coordinateBlock = 0; coordinateBlock < numberOfWayCoordinateBlocks; coordinateBlock++) {
				// get and check the number of way nodes (VBE-U)
				int numberOfWayNodes = this.readBuffer.ReadUnsignedInt();
				if (numberOfWayNodes < 2 || numberOfWayNodes > MAXIMUM_WAY_NODES_SEQUENCE_LENGTH) {
					System.Diagnostics.Debug.WriteLine("invalid number of way nodes: " + numberOfWayNodes);
					return null;
				}

				// create the array which will store the current way segment
				GeoPointList waySegment;

				if (doubleDeltaEncoding) {
					waySegment = decodeWayNodesDoubleDelta(numberOfWayNodes);
				} else {
					waySegment = DecodeWayNodesSingleDelta(numberOfWayNodes);
				}

				wayCoordinates.Add(waySegment);
			}

			return wayCoordinates;
		}

		private List<Way> processWays ( QueryParameters queryParameters, int numberOfWays )
		{
			List<Way> ways = new List<Way>();
			Tag[] wayTags = this.mapFileHeader.MapFileInfo.WayTags;

			for (int elementCounter = numberOfWays; elementCounter != 0; --elementCounter) {
				if (this.mapFileHeader.MapFileInfo.DebugFile) {
					// get and check the way signature
					this.signatureWay = this.readBuffer.ReadUTF8Encodedstring(SIGNATURE_LENGTH_WAY);
					if (!this.signatureWay.StartsWith("---WayStart")) {
						System.Diagnostics.Debug.WriteLine("invalid way signature: " + this.signatureWay);
						System.Diagnostics.Debug.WriteLine(DEBUG_SIGNATURE_BLOCK + this.signatureBlock);
						return null;
					}
				}

				// get the size of the way (VBE-U)
				int wayDataSize = this.readBuffer.ReadUnsignedInt();
				if (wayDataSize < 0) {
					System.Diagnostics.Debug.WriteLine("invalid way data size: " + wayDataSize);
					if (this.mapFileHeader.MapFileInfo.DebugFile) {
						System.Diagnostics.Debug.WriteLine(DEBUG_SIGNATURE_BLOCK + this.signatureBlock);
					}
					return null;
				}

				if (queryParameters.useTileBitmask) {
					// get the way tile bitmask (2 bytes)
					int tileBitmask = this.readBuffer.Readshort();
					// check if the way is inside the requested tile
					if ((queryParameters.queryTileBitmask & tileBitmask) == 0) {
						// skip the rest of the way and continue with the next way
						this.readBuffer.SkipBytes(wayDataSize - 2);
						continue;
					}
				} else {
					// ignore the way tile bitmask (2 bytes)
					this.readBuffer.SkipBytes(2);
				}

				// get the special byte which encodes multiple flags
				byte specialbyte = (byte)this.readBuffer.ReadByte();

				// bit 1-4 represent the layer
				byte layer = (byte)((specialbyte & WAY_LAYER_BITMASK) >> WAY_LAYER_SHIFT);
				// bit 5-8 represent the number of tag IDs
				byte numberOfTags = (byte)(specialbyte & WAY_NUMBER_OF_TAGS_BITMASK);

				List<Tag> tags = new List<Tag>();

				for (byte tagIndex = numberOfTags; tagIndex != 0; --tagIndex) {
					int tagId = this.readBuffer.ReadUnsignedInt();
					if (tagId < 0 || tagId >= wayTags.Length) {
						System.Diagnostics.Debug.WriteLine("invalid way tag ID: " + tagId);
						return null;
					}
					tags.Add(wayTags[tagId]);
				}

				// get the feature bitmask (1 byte)
				byte featurebyte = (byte)this.readBuffer.ReadByte();

				// bit 1-6 enable optional features
				bool featureName = (featurebyte & WAY_FEATURE_NAME) != 0;
				bool featureHouseNumber = (featurebyte & WAY_FEATURE_HOUSE_NUMBER) != 0;
				bool featureRef = (featurebyte & WAY_FEATURE_REF) != 0;
				bool featureLabelPosition = (featurebyte & WAY_FEATURE_LABEL_POSITION) != 0;
				bool featureWayDataBlocksbyte = (featurebyte & WAY_FEATURE_DATA_BLOCKS_byte) != 0;
				bool featureWayDoubleDeltaEncoding = (featurebyte & WAY_FEATURE_DOUBLE_DELTA_ENCODING) != 0;

				// check if the way has a name
				if (featureName) {
					tags.Add(new Tag(TAG_KEY_NAME, this.readBuffer.ReadUTF8Encodedstring()));
				}

				// check if the way has a house number
				if (featureHouseNumber) {
					tags.Add(new Tag(TAG_KEY_HOUSE_NUMBER, this.readBuffer.ReadUTF8Encodedstring()));
				}

				// check if the way has a reference
				if (featureRef) {
					tags.Add(new Tag(TAG_KEY_REF, this.readBuffer.ReadUTF8Encodedstring()));
				}

				GeoPoint labelPosition = readOptionalLabelPosition(featureLabelPosition);

				int wayDataBlocks = readOptionalWayDataBlocksbyte(featureWayDataBlocksbyte);
				if (wayDataBlocks < 1) {
					System.Diagnostics.Debug.WriteLine("invalid number of way data blocks: " + wayDataBlocks);
					return null;
				}

				for (int wayDataBlock = 0; wayDataBlock < wayDataBlocks; ++wayDataBlock) {
					GeoPointList2 wayNodes = processWayDataBlock(featureWayDoubleDeltaEncoding);
					if (wayNodes == null) {
						return null;
					}

					ways.Add(new Way(layer, tags, wayNodes, labelPosition));
				}
			}

			return ways;
		}

		private GeoPoint readOptionalLabelPosition ( bool featureLabelPosition )
		{
			if (featureLabelPosition) {
				// get the label position latitude offset (VBE-S)
				double latitude = this.tilePosition.Latitude
						+ CoordinatesUtil.microdegreesToDegrees(this.readBuffer.ReadSignedInt());

				// get the label position longitude offset (VBE-S)
				double longitude = this.tilePosition.Longitude
						+ CoordinatesUtil.microdegreesToDegrees(this.readBuffer.ReadSignedInt());

				return new GeoPoint(latitude, longitude);
			}

			return null;
		}

		private int readOptionalWayDataBlocksbyte ( bool featureWayDataBlocksbyte )
		{
			if (featureWayDataBlocksbyte) {
				// get and check the number of way data blocks (VBE-U)
				return this.readBuffer.ReadUnsignedInt();
			}
			// only one way data block exists
			return 1;
		}

		private ZoomTable readZoomTable ( SubFileParameter subFileParameter )
		{
			int rows = subFileParameter.ZoomLevelMax - subFileParameter.ZoomLevelMin + 1;
			ZoomTable zoomTable = new ZoomTable(rows);

			int cumulatedNumberOfnodes = 0;
			int cumulatedNumberOfWays = 0;

			for (int row = 0; row < rows; ++row) {
				cumulatedNumberOfnodes += this.readBuffer.ReadUnsignedInt();
				cumulatedNumberOfWays += this.readBuffer.ReadUnsignedInt();

				if (cumulatedNumberOfnodes < 0 || cumulatedNumberOfnodes > MAXIMUM_ZOOM_TABLE_OBJECTS) {
					System.Diagnostics.Debug.WriteLine("invalid cumulated number of nodes in row " + row + ' ' + cumulatedNumberOfnodes);
					if (this.mapFileHeader.MapFileInfo.DebugFile) {
						System.Diagnostics.Debug.WriteLine(DEBUG_SIGNATURE_BLOCK + this.signatureBlock);
					}
					return null;
				} else if (cumulatedNumberOfWays < 0 || cumulatedNumberOfWays > MAXIMUM_ZOOM_TABLE_OBJECTS) {
					System.Diagnostics.Debug.WriteLine("invalid cumulated number of ways in row " + row + ' ' + cumulatedNumberOfWays);
					if (this.mapFileHeader.MapFileInfo.DebugFile) {
						System.Diagnostics.Debug.WriteLine(DEBUG_SIGNATURE_BLOCK + this.signatureBlock);
					}
					return null;
				}

				zoomTable[row] = new ZoomTable.ZoomTableEntry(cumulatedNumberOfnodes, cumulatedNumberOfWays);
			}

			return zoomTable;
		}

		#region IDisposable Membres

		public void Dispose ()
		{
			this.CloseFile();
		}

		#endregion

		public int numberOfNodes { get; set; }
	}
}
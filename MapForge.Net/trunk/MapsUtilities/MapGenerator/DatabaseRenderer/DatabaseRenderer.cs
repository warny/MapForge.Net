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
using RenderTheme.Graphics;
using System.Collections.Generic;
using MapDB.Reader;
using MapDB.Reader.Header;
using MapsUtilities.MapGenerator.DatabaseRenderer;
using MapCore.Util;
using System;
using System.IO;
using RenderTheme;
using RenderTheme.Rule;
using System.Xml;
using System.Windows.Media;
using MapsUtilities.MapGenerator;
using System.Windows.Media.Imaging;

/**
 * A DatabaseRenderer renders map tiles by reading from a {@link MapDatabase}.
 */	

namespace MapsUtilitiess.MapGenerator.DatabaseRenderer {
public class DatabaseRenderer : RenderCallback {
	private const byte DEFAULT_START_ZOOM_LEVEL = 12;
	private const byte LAYERS = 11;
	private static readonly Paint PAINT_WATER_TILE_HIGHTLIGHT = AndroidGraphics.INSTANCE.getPaint();
	private const double STROKE_INCREASE = 1.5;
	private const byte STROKE_MIN_ZOOM_LEVEL = 12;
	private static readonly Tag TAG_NATURAL_WATER = new Tag("natural", "water");
	private static readonly MapPoint[][] WATER_TILE_COORDINATES = getTilePixelCoordinates();
	private const byte ZOOM_MAX = 22;

	private static XmlRenderTheme getRenderTheme(XmlRenderTheme jobTheme) {
		try {
			return RenderThemeHandler.GetRenderTheme(AndroidGraphics.INSTANCE, jobTheme);
		} catch (XmlException e) {
			System.Diagnostics.Debug.WriteLine(e);
		} catch (IOException e) {
			System.Diagnostics.Debug.WriteLine(e);
		}
		return null;
	}

	private static MapPoint[][] getTilePixelCoordinates() {
		MapPoint point1 = new MapPoint(0, 0);
		MapPoint point2 = new MapPoint(Tile.TileSize, 0);
		MapPoint point3 = new MapPoint(Tile.TileSize, Tile.TileSize);
		MapPoint point4 = new MapPoint(0, Tile.TileSize);
		return new MapPoint[][] { new [] { point1, point2, point3, point4, point1 } };
	}

	private static byte getValidLayer(byte layer) {
		if (layer < 0) {
			return 0;
		} else if (layer >= LAYERS) {
			return LAYERS - 1;
		} else {
			return layer;
		}
	}

	private List<pointTextContainer> areaLabels;
	private CanvasRasterer canvasRasterer;
	private MapPoint[][] coordinates;
	private Tile currentTile;
	private List<List<ShapePaintContainer>> drawingLayers;
	private LabelPlacement labelPlacement;
	private MapDatabase mapDatabase;
	private List<pointTextContainer> nodes;
	private List<SymbolContainer> pointSymbols;
	private MapPoint NODEPosition;
	private XmlRenderTheme previousJobTheme;
	private float previousTextScale;
	private byte previousZoomLevel;
	private XmlRenderTheme renderTheme;
	private ShapeContainer shapeContainer;
	private List<WayTextContainer> wayNames;
	private List<List<List<ShapePaintContainer>>> ways;
	private List<SymbolContainer> waySymbols;

	/**
	 * Constructs a new DatabaseRenderer.
	 * 
	 * @param mapDatabase
	 *            the MapDatabase from which the map data will be read.
	 */
	public DatabaseRenderer(MapDatabase mapDatabase) {
		this.mapDatabase = mapDatabase;
		this.canvasRasterer = new CanvasRasterer();
		this.labelPlacement = new LabelPlacement();

		this.ways = new List<List<List<ShapePaintContainer>>>(LAYERS);
		this.wayNames = new List<WayTextContainer>(64);
		this.nodes = new List<pointTextContainer>(64);
		this.areaLabels = new List<pointTextContainer>(64);
		this.waySymbols = new List<SymbolContainer>(64);
		this.Pointsymbols = new List<SymbolContainer>(64);

		PAINT_WATER_TILE_HIGHTLIGHT.Style = Style.FILL;
		PAINT_WATER_TILE_HIGHTLIGHT.Color = Colors.Cyan;
	}

	/**
	 * Called when a job needs to be executed.
	 * 
	 * @param mapGeneratorJob
	 *            the job that should be executed.
	 * @param BitmapImage
	 *            the BitmapImage for the generated map tile.
	 * @return true if the job was executed successfully, false otherwise.
	 */
	public bool executeJob(MapGeneratorJob mapGeneratorJob, BitmapImage BitmapImage) {
		this.currentTile = mapGeneratorJob.tile;
		System.Windows.Media.Imaging.WriteableBitmap bmp;


		XmlRenderTheme jobTheme = mapGeneratorJob.jobParameters.jobTheme;
		if (!jobTheme.Equals(this.previousJobTheme)) {
			if (this.renderTheme != null) {
				this.renderTheme.Dispose();
			}
			this.renderTheme = getRenderTheme(jobTheme);
			if (this.renderTheme == null) {
				this.previousJobTheme = null;
				return false;
			}
			createWayLists();
			this.previousJobTheme = jobTheme;
			this.previousZoomLevel = byte.MinValue;
			// invalidate the previousTextScale so that textScale from jobParameters will
			// be applied next time
			this.previousTextScale = -1;
		}

		byte zoomLevel = this.currentTile.ZoomFactor;
		if (zoomLevel != this.previousZoomLevel) {
			setScaleStrokeWidth(zoomLevel);
			this.previousZoomLevel = zoomLevel;
		}

		float textScale = mapGeneratorJob.jobParameters.textScale;
		if (textScale != this.previousTextScale) {
			this.renderTheme.scaleTextSize(textScale);
			this.previousTextScale = textScale;
		}

		if (this.mapDatabase != null) {
			MapReadResult mapReadResult = this.mapDatabase.ReadMapData(this.currentTile);
			processReadMapData(mapReadResult);
		}

		this.nodes = this.labelPlacement.placeLabels(this.nodes, this.Pointsymbols, this.areaLabels, this.currentTile);

		this.canvasRasterer.setCanvasBitmap(BitmapImage);
		this.canvasRasterer.fill(this.renderTheme.getMapBackground());
		this.canvasRasterer.drawWays(this.ways);
		this.canvasRasterer.drawSymbols(this.waySymbols);
		this.canvasRasterer.drawSymbols(this.Pointsymbols);
		this.canvasRasterer.drawWayNames(this.wayNames);
		this.canvasRasterer.drawNodes(this.nodes);
		this.canvasRasterer.drawNodes(this.areaLabels);

		if (mapGeneratorJob.debugSettings.drawTileFrames) {
			this.canvasRasterer.drawTileFrame();
		}

		if (mapGeneratorJob.debugSettings.drawTileCoordinates) {
			this.canvasRasterer.drawTileCoordinates(this.currentTile);
		}

		clearLists();

		return true;
	}

	/**
	 * @return the start point (may be null).
	 */
	public GeoPoint getStartPoint() {
		if (this.mapDatabase != null && this.mapDatabase.HasOpenFile()) {
			MapFileInfo mapFileInfo = this.mapDatabase.getMapFileInfo();
			if (mapFileInfo.StartPosition != null) {
				return mapFileInfo.StartPosition;
			}
			return mapFileInfo.BoundingBox.getCenterpoint();
		}

		return null;
	}

	/**
	 * @return the start zoom level (may be null).
	 */
	public byte getStartZoomLevel() {
		if (this.mapDatabase != null && this.mapDatabase.HasOpenFile()) {
			MapFileInfo mapFileInfo = this.mapDatabase.getMapFileInfo();
			if (mapFileInfo.StartZoomLevel != null) {
				return mapFileInfo.StartZoomLevel;
			}
		}

		return DEFAULT_START_ZOOM_LEVEL;
	}

	/**
	 * @return the maximum zoom level.
	 */
	public byte getZoomLevelMax() {
		return ZOOM_MAX;
	}

	//@Override
	public void renderArea(Paint fill, Paint stroke, int level) {
		List<ShapePaintContainer> list = this.drawingLayers[level];
		list.Add(new ShapePaintContainer(this.shapeContainer, fill));
		list.Add(new ShapePaintContainer(this.shapeContainer, stroke));
	}

	//@Override
	public void renderAreaCaption(string caption, float verticalOffset, Paint fill, Paint stroke) {
		MapPoint centerPosition = GeometryUtils.calculateCenterOfBoundingBox(this.coordinates[0]);
		this.areaLabels.Add(new PointTextContainer(caption, centerPosition.X, centerPosition.Y, fill, stroke));
	}

	//@Override
	public void renderAreaSymbol(BitmapImage symbol) {
		MapPoint centerPosition = GeometryUtils.calculateCenterOfBoundingBox(this.coordinates[0]);
		double halfSymbolWidth = symbol.Width / 2;
		double halfSymbolHeight = symbol.Height / 2;
		double pointX = centerPosition.X - halfSymbolWidth;
		double pointY = centerPosition.Y - halfSymbolHeight;
		MapPoint shiftedCenterPosition = new MapPoint(pointX, pointY);
		this.Pointsymbols.Add(new SymbolContainer(symbol, shiftedCenterPosition));
	}

	//@Override
	public void renderpointOfInterestCaption(string caption, float verticalOffset, Paint fill, Paint stroke) {
		this.nodes.Add(new PointTextContainer(caption, this.NODEPosition.X, this.NODEPosition.Y + verticalOffset, fill,
				stroke));
	}

	//@Override
	public void renderpointOfInterestCircle(float radius, Paint fill, Paint stroke, int level) {
		List<ShapePaintContainer> list = this.drawingLayers[level];
		list.Add(new ShapePaintContainer(new CircleContainer(this.NODEPosition, radius), fill));
		list.Add(new ShapePaintContainer(new CircleContainer(this.NODEPosition, radius), stroke));
	}

	//@Override
	public void renderpointOfInterestSymbol(BitmapImage symbol) {
		double halfSymbolWidth = symbol.Width / 2;
		double halfSymbolHeight = symbol.Height / 2;
		double pointX = this.NODEPosition.X - halfSymbolWidth;
		double pointY = this.NODEPosition.Y - halfSymbolHeight;
		MapPoint shiftedCenterPosition = new MapPoint(pointX, pointY);
		this.Pointsymbols.Add(new SymbolContainer(symbol, shiftedCenterPosition));
	}

	//@Override
	public void renderWay(Paint stroke, int level) {
		this.drawingLayers[level].Add(new ShapePaintContainer(this.shapeContainer, stroke));
	}

	//@Override
	public void renderWaySymbol(BitmapImage symbolBitmap, bool alignCenter, bool repeatSymbol) {
		WayDecorator.renderSymbol(symbolBitmap, alignCenter, repeatSymbol, this.coordinates, this.waySymbols);
	}

	//@Override
	public void renderWayText(string textKey, Paint fill, Paint stroke) {
		WayDecorator.renderText(textKey, fill, stroke, this.coordinates, this.wayNames);
	}

	private void clearLists() {
		for (int i = this.ways.Count - 1; i >= 0; --i) {
			List<List<ShapePaintContainer>> innerWayList = this.ways[i];
			for (int j = innerWayList.Count - 1; j >= 0; --j) {
				innerWayList[j].Clear();
			}
		}

		this.areaLabels.Clear();
		this.nodes.Clear();
		this.Pointsymbols.Clear();
		this.wayNames.Clear();
		this.waySymbols.Clear();
	}

	private void createWayLists() {
		int levels = this.renderTheme.getLevels();
		this.ways.Clear();

		for (byte i = LAYERS - 1; i >= 0; --i) {
			List<List<ShapePaintContainer>> innerWayList = new List<List<ShapePaintContainer>>(levels);
			for (int j = levels - 1; j >= 0; --j) {
				innerWayList.Add(new List<ShapePaintContainer>(0));
			}
			this.ways.Add(innerWayList);
		}
	}

	private void processReadMapData(MapReadResult mapReadResult) {
		if (mapReadResult == null) {
			return;
		}

		foreach (Node pointOfInterest in mapReadResult.Nodes) {
			renderpointOfInterest(pointOfInterest);
		}

		foreach (Way way in mapReadResult.Ways) {
			renderWay(way);
		}

		if (mapReadResult.IsWater) {
			renderWaterBackground();
		}
	}

	private void renderpointOfInterest(Node pointOfInterest) {
		this.drawingLayers = this.ways[getValidLayer(pointOfInterest.Layer)];
		this.NODEPosition = scaleGeoPoint(pointOfInterest.Position);
		this.renderTheme.matchNode(this, pointOfInterest.Tags, this.currentTile.ZoomFactor);
	}

	private void renderWaterBackground() {
		this.drawingLayers = this.ways[0];
		this.coordinates = WATER_TILE_COORDINATES;
		this.shapeContainer = new WayContainer(this.coordinates);
		this.renderTheme.matchClosedWay(this, new[] { TAG_NATURAL_WATER }, this.currentTile.ZoomFactor);
	}

	private void renderWay(Way way) {
		this.drawingLayers = this.ways[getValidLayer(way.Layer)];
		// TODO what about the label position?

		GeoPointList2 GeoPoints = way.GeoPoints;
		this.coordinates = new MapPoint[GeoPoints.Count][];
		for (int i = 0; i < this.coordinates.Length; ++i) {
			this.coordinates[i] = new MapPoint[GeoPoints[i].Count];

			for (int j = 0; j < this.coordinates[i].Length; ++j) {
				this.coordinates[i][j] = scaleGeoPoint(GeoPoints[i][j]);
			}
		}
		this.shapeContainer = new WayContainer(this.coordinates);

		if (GeometryUtils.isClosedWay(this.coordinates[0])) {
			this.renderTheme.matchClosedWay(this, way.Tags, this.currentTile.ZoomFactor);
		} else {
			this.renderTheme.matchLinearWay(this, way.Tags, this.currentTile.ZoomFactor);
		}
	}

	/**
	 * Converts the given GeoPoint into XY coordinates on the current tile.
	 * 
	 * @param GeoPoint
	 *            the GeoPoint to convert.
	 * @return the XY coordinates on the current tile.
	 */
	private MapPoint scaleGeoPoint(GeoPoint GeoPoint) {
		double pixelX = MercatorProjection.longitudeToPixelX(GeoPoint.Longitude, this.currentTile.ZoomFactor)
				- this.currentTile.PixelX;
		double pixelY = MercatorProjection.latitudeToPixelY(GeoPoint.Latitude, this.currentTile.ZoomFactor)
				- this.currentTile.PixelY;

		return new MapPoint((float) pixelX, (float) pixelY);
	}

	/**
	 * Sets the scale stroke factor for the given zoom level.
	 * 
	 * @param zoomLevel
	 *            the zoom level for which the scale stroke factor should be set.
	 */
	private void setScaleStrokeWidth(byte zoomLevel) {
		int zoomLevelDiff = Math.Max(zoomLevel - STROKE_MIN_ZOOM_LEVEL, 0);
		this.renderTheme.scaleStrokeWidth((float) Math.Pow(STROKE_INCREASE, zoomLevelDiff));
	}

	public void Dispose() {
		if (this.renderTheme != null) {
			this.renderTheme.Dispose();
			this.renderTheme = null;
		}
	}
}
}

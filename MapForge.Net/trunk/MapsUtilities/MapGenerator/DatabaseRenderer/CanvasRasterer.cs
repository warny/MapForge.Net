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
using System.Windows.Controls;
using MapCore.Model;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;
using RenderTheme.Graphics;
using System.Windows.Media.Imaging;

namespace MapsUtilities.MapGenerator.DatabaseRenderer
{

	/**
	 * A CanvasRasterer uses a Canvas for drawing.
	 * 
	 * @see <a href="http://developer.android.com/reference/android/graphics/Canvas.html">Canvas</a>
	 */
	class CanvasRasterer
	{
		private static Canvas createAndroidPaint ()
		{
			return new Canvas();
		}

		private static readonly Canvas PAINT_BITMAP_FILTER = createAndroidPaint();
		private static readonly Canvas PAINT_TILE_COORDINATES = createAndroidPaint();
		private static readonly Canvas PAINT_TILE_COORDINATES_STROKE = createAndroidPaint();
		private static readonly Canvas PAINT_TILE_FRAME = createAndroidPaint();
		private static readonly float[] TILE_FRAME = new float[] { 0, 0, 0, Tile.TileSize, 0, Tile.TileSize, Tile.TileSize,
			Tile.TileSize, Tile.TileSize, Tile.TileSize, Tile.TileSize, 0, Tile.TileSize, 0, 0, 0 };

		private static void configurePaints ()
		{
			PAINT_TILE_COORDINATES.setTypeface(Typeface.defaultFromStyle(Typeface.BOLD));
			PAINT_TILE_COORDINATES.setTextSize(20);

			PAINT_TILE_COORDINATES.setTypeface(Typeface.defaultFromStyle(Typeface.BOLD));
			PAINT_TILE_COORDINATES_STROKE.setStyle(Style.STROKE);
			PAINT_TILE_COORDINATES_STROKE.setStrokeWidth(5);
			PAINT_TILE_COORDINATES_STROKE.setTextSize(20);
			PAINT_TILE_COORDINATES_STROKE.Color = AndroidGraphics.INSTANCE.getColor(Colors.White);
		}

		private Canvas canvas;
		private Path path;
		private CompositeTransform symbolMatrix;

		public CanvasRasterer ()
		{
			this.canvas = new Canvas();
			this.symbolMatrix = new CompositeTransform();
			this.path = new Path();
			this.path.setFillType(Path.FillType.EVEN_ODD);
			configurePaints();
		}

		private void drawTileCoordinate ( string text, int offsetY )
		{
			this.canvas.drawText(text, 20, offsetY, PAINT_TILE_COORDINATES_STROKE);
			this.canvas.drawText(text, 20, offsetY, PAINT_TILE_COORDINATES);
		}

		public void drawNodes ( List<pointTextContainer> pointTextContainers )
		{
			for (int index = pointTextContainers.Count - 1; index >= 0; --index) {
				pointTextContainer pointTextContainer = pointTextContainers.get(index);

				if (pointTextContainer.paintBack != null) {
					Paint androidPaint = AndroidGraphics.getAndroidPaint(pointTextContainer.paintBack);
					this.canvas.drawText(pointTextContainer.text, (float)pointTextContainer.x,
							(float)pointTextContainer.y, androidPaint);
				}


				Paint androidPaint = AndroidGraphics.getAndroidPaint(pointTextContainer.paintFront);
				this.canvas.drawText(pointTextContainer.text, (float)pointTextContainer.x, (float)pointTextContainer.y,
						androidPaint);
			}
		}

		public void drawSymbols ( List<SymbolContainer> symbolContainers )
		{
			for (int index = symbolContainers.Count - 1; index >= 0; --index) {
				SymbolContainer symbolContainer = symbolContainers[index];

				MapPoint point = symbolContainer.point;
				if (symbolContainer.alignCenter) {
					double pivotX = symbolContainer.symbol.Width / 2;
					double pivotY = symbolContainer.symbol.Height / 2;
					this.symbolMatrix.Rotation = symbolContainer.rotation;
					this.symbolMatrix.CenterX = pivotX;
					this.symbolMatrix.CenterY = pivotY;

					this.symbolMatrix.TranslateX = point.X - pivotX	;
					this.symbolMatrix.TranslateY = point.Y - pivotY;
				} else {
					this.symbolMatrix.Rotation = symbolContainer.rotation;
					this.symbolMatrix.TranslateX = point.X;
					this.symbolMatrix.TranslateY = point.Y;
				}

				BitmapImage androidBitmap = AndroidGraphics.getAndroidBitmap(symbolContainer.symbol);
				this.canvas.drawBitmap(androidBitmap, this.symbolMatrix, PAINT_BITMAP_FILTER);
			}
		}

		void drawTileCoordinates ( Tile tile )
		{
			drawTileCoordinate("X: " + tile.TileX, 30);
			drawTileCoordinate("Y: " + tile.TileY, 60);
			drawTileCoordinate("Z: " + tile.ZoomFactor, 90);
		}

		void drawTileFrame ()
		{
			this.canvas.drawLines(TILE_FRAME, PAINT_TILE_FRAME);
		}

		public void drawWayNames ( List<WayTextContainer> wayTextContainers )
		{
			for (int index = wayTextContainers.Count - 1; index >= 0; --index) {
				WayTextContainer wayTextContainer = wayTextContainers[index];
				this.path.rewind();

				double[] textCoordinates = wayTextContainer.coordinates;
				this.path.moveTo((float)textCoordinates[0], (float)textCoordinates[1]);
				for (int i = 2; i < textCoordinates.Length; i += 2) {
					this.path.lineTo((float)textCoordinates[i], (float)textCoordinates[i + 1]);
				}

				Paint androidPaint = AndroidGraphics.getAndroidPaint(wayTextContainer.paint);
				this.canvas.drawTextOnPath(wayTextContainer.text, this.path, 0, 3, androidPaint);
			}
		}

		public void drawWays ( List<List<List<ShapePaintContainer>>> drawWays )
		{
			int levelsPerLayer = drawWays[0].Count;

			for (int layer = 0, layers = drawWays.Count; layer < layers; ++layer) {
				List<List<ShapePaintContainer>> shapePaintContainers = drawWays[layer];

				for (int level = 0; level < levelsPerLayer; ++level) {
					List<ShapePaintContainer> wayList = shapePaintContainers[level];

					for (int index = wayList.Count - 1; index >= 0; --index) {
						ShapePaintContainer shapePaintContainer = wayList[index];
						this.path.rewind();

						switch (shapePaintContainer.shapeContainer.getShapeType()) {
							case ShapeType.CIRCLE:
								CircleContainer circleContainer = (CircleContainer)shapePaintContainer.shapeContainer;
								MapPoint point = circleContainer.point;
								this.path.AddCircle((float)point.X, (float)point.Y, circleContainer.radius,
										Path.Direction.CCW);
								break;

							case ShapeType.WAY:
								WayContainer wayContainer = (WayContainer)shapePaintContainer.shapeContainer;
								MapPoint[][] coordinates = wayContainer.coordinates;
								for (int j = 0; j < coordinates.Length; ++j) {
									// make sure that the coordinates sequence is not empty
									MapPoint[] points = coordinates[j];
									if (points.Length >= 2) {
										MapPoint immutablepoint = points[0];
										this.path.moveTo((float)immutablepoint.X, (float)immutablepoint.Y);
										for (int i = 1; i < points.Length; ++i) {
											immutablepoint = points[i];
											this.path.lineTo((float)immutablepoint.X, (float)immutablepoint.Y);
										}
									}
								}
								break;
						}

						Paint androidPaint = AndroidGraphics.getAndroidPaint(shapePaintContainer.paint);
						this.canvas.drawPath(this.path, androidPaint);
					}
				}
			}
		}

		public void fill ( Color color )
		{
			this.canvas.drawColor(color);
		}

		public void setCanvasBitmap ( BitmapImage BitmapImage )
		{
			this.canvas.setBitmap(BitmapImage);
		}
	}
}
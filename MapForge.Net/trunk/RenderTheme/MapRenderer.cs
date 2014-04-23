using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MapCore;
using MapCore.Model;
using RenderTheme.Rule;

namespace RenderTheme
{
	public class MapRenderer : IBitmapMapProvider
	{
		private readonly IVectorMapProvider vectorMapProvider;
		readonly Theme renderTheme;

		private Dictionary<byte, int> layersIndexes;
		private List<MapElement> elements;
		private Vector correctionVector;
		private Tile baseTile;

		public MapRenderer ( IVectorMapProvider vectorMapProvider, Theme renderTheme )
		{
			this.vectorMapProvider = vectorMapProvider;
			this.renderTheme = renderTheme;
		}

		#region IBitmapMapProvider Membres

		public MapCore.Projections.RepresentationConverter Projection { get { return vectorMapProvider.Projection; } }

		public ImageBrush GetTile ( Tile tile )
		{
			baseTile = tile;
			layersIndexes = new Dictionary<byte, int>();
			elements = new List<MapElement>();

			this.correctionVector = tile.MapPoint1 - new Point(0, 0);

			ImportResults(vectorMapProvider.ReadMapData(tile));
			return RenderTile();
		}

		public void ImportResults ( IMapReadResult mapReadResults )
		{
			elements = new List<MapElement>();
			foreach (var way in mapReadResults.Ways.OrderBy(w => w.Layer)) {
				this.Add(way);
			}

			foreach (var node in mapReadResults.Nodes.OrderBy(w => w.Layer)) {
				this.Add(node);
			}

		}


		public void Add ( MapObjectBase obj )
		{
			MapElement element;
			if (obj is Node) {
				element = new MapElement(Projection, baseTile, correctionVector, (Node)obj);
			} else if (obj is Way) {
				element = new MapElement(Projection, baseTile, correctionVector, (Way)obj);
			} else return;
			renderTheme.Match(element);


			byte lastLayer = 0;
			foreach (var layerIndex in layersIndexes) {
				lastLayer = layerIndex.Key;
				if (layerIndex.Key > obj.Layer) {
					layersIndexes[obj.Layer] = layerIndex.Value;
					layersIndexes[layerIndex.Key] = layerIndex.Value + 1;
				}
			}

			if (lastLayer < obj.Layer) {
				layersIndexes[lastLayer] = elements.Count;
				elements.Add(element);
			} else {
				elements.Insert(0, element);
			}
		}

		public ImageBrush RenderTile ()
		{
			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext drawingContext = drawingVisual.RenderOpen();

			foreach (var item in elements) {
				item.Draw(drawingContext);
			}

			drawingContext.Close();

			RenderTargetBitmap renderBitmap = new RenderTargetBitmap(baseTile.TileSize, baseTile.TileSize, 96, 96, PixelFormats.Pbgra32);
			renderBitmap.Render(drawingVisual);
			return new ImageBrush(BitmapFrame.Create(renderBitmap));
		}

		#endregion
	}
}

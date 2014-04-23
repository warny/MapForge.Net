using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MapCore;
using MapCore.Model;
using MapCore.Projections;
using MapCore.Util;

namespace DisplayMap
{
	/// <summary>
	/// Logique d'interaction pour Map.xaml
	/// </summary>
	public partial class Map : UserControl
	{
		private LRUCache<Tile, MapTile> cache;

		private readonly Queue<Action> actions;
		private bool isAlive;
		private readonly Thread threadActions;

		public Map ()
		{
			cache = new LRUCache<Tile, MapTile>(100);
			InitializeComponent();
			actions = new Queue<Action>();
			threadActions = new Thread(DoActions);
			threadActions.Start();
		}

		private void DoActions()
		{
			isAlive = true;
			while (isAlive) {
				if (!actions.Any()) {
					Thread.Sleep(100);
					continue;
				}
				Action action = actions.Dequeue();
				this.Dispatcher.Invoke(action);
			}								  
			
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			isAlive = false;
			base.OnLostFocus(e);
		}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			if (threadActions.IsAlive) threadActions.Abort();
			threadActions.Start();
			base.OnGotFocus(e);
		}

		private GeoPoint position;

		public GeoPoint Position
		{
		  get { return position; }
		  set { 
			  position = value;
			  Redraw();
		  }
		}

		private byte zoomFactor;

		public byte ZoomFactor
		{
			get { return zoomFactor; }
			set
			{
				zoomFactor = value;
				Redraw();
			}
		}

		private IBitmapMapProvider mapProvider;

		public IBitmapMapProvider MapProvider
		{
			get { return mapProvider; }
			set {
				mapProvider = value;
				Redraw();
			}
		}

		public RepresentationConverter Projection
		{
			get { return mapProvider == null ? null : mapProvider.Projection; }
		}


		public override void BeginInit ()
		{
			base.BeginInit();
			Redraw();
		}

		protected override void OnRenderSizeChanged ( SizeChangedInfo sizeInfo )
		{
			base.OnRenderSizeChanged(sizeInfo);
			Redraw();
		}

		private void Redraw () {
			if (mapProvider == null) return;
			if (position == null) return;
			if (zoomFactor == 0) return;

			var basePoint = Projection.GeoPointToMappoint(position, zoomFactor);
			var baseTile = Projection.MappointToTile(basePoint);

			var toRemove = TilesHolder.Children.OfType<UIElement>().Where(item=> {return 
					(Canvas.GetLeft(item) + baseTile.TileSize < 0) ||
					(Canvas.GetTop(item) + baseTile.TileSize < 0) ||
					(Canvas.GetLeft(item) > this.RenderSize.Width) ||
					(Canvas.GetTop(item) > this.RenderSize.Height);
			}).ToArray();
			foreach (UIElement item in toRemove) {
				TilesHolder.Children.Remove(item);
			}


			Point center = new Point(this.RenderSize.Width / 2, this.RenderSize.Height / 2);

			int tilesInWidth = ((int)this.RenderSize.Width / Projection.TileSize) + 2;
			int tilesInHeight = ((int)this.RenderSize.Height / Projection.TileSize) + 2;

			MapPoint upperLeftPoint = new MapPoint(basePoint.X - center.X, basePoint.Y - center.Y, basePoint.ZoomFactor, basePoint.TileSize);
			Tile upperLeftTile = Projection.MappointToTile(upperLeftPoint);

			double startX = upperLeftTile.MapPoint1.X - upperLeftPoint.X;
			double startY = upperLeftTile.MapPoint1.Y - upperLeftPoint.Y;
			for (int x = 0; x < tilesInWidth; x++) {
				for (int y = 0; y < tilesInHeight; y++) {
					Tile tile = new Tile(x + upperLeftTile.TileX, y + upperLeftTile.TileY, upperLeftTile.ZoomFactor, upperLeftTile.TileSize);
					MapTile mapTile;
					if (!cache.TryGetValue(tile, out mapTile)) {
						mapTile = new MapTile(this.Projection, tile);

						actions.Enqueue(() => {
							if (cache.ContainsKey(tile)) return;
							Brush tileBitmap = mapProvider.GetTile(tile);
							cache.Add(tile, mapTile);
							mapTile.Brush = tileBitmap;
						});
					}
					Canvas.SetLeft(mapTile, startX + upperLeftTile.TileSize * x);
					Canvas.SetTop(mapTile, startY + upperLeftTile.TileSize * y);
					if (TilesHolder.Children.Contains(mapTile)) continue;
						TilesHolder.Children.Add(mapTile);
				}
			}
		}
		protected override void OnMouseWheel ( MouseWheelEventArgs e )
		{
			base.OnMouseWheel(e);
			int zoomFactor = this.ZoomFactor + e.Delta/120;
			if (zoomFactor < 5) zoomFactor = 5;
			if (zoomFactor > 22) ZoomFactor = 22;
			this.ZoomFactor = (byte)zoomFactor;
		}

		Point? previousPosition;

		protected override void OnMouseMove ( MouseEventArgs e )
		{
			base.OnMouseMove(e);

			var basePoint = Projection.GeoPointToMappoint(position, zoomFactor);
			Point center = new Point(this.RenderSize.Width / 2, this.RenderSize.Height / 2);
			Point mouse = e.GetPosition(this);
			MapPoint mousePoint = new MapPoint(basePoint.X - center.X + mouse.X, basePoint.Y - center.Y + mouse.Y, basePoint.ZoomFactor, basePoint.TileSize);
			GeoPoint mouseGeoPoint = Projection.MappointToGeoPoint(mousePoint);
			this.GeoPosition.Text = mouseGeoPoint.ToString();

			if (e.LeftButton == MouseButtonState.Pressed) {
				if (previousPosition != null) {
					MapPoint newCenterPosition = new MapPoint(basePoint.X + previousPosition.Value.X - mouse.X, basePoint.Y + previousPosition.Value.Y - mouse.Y, basePoint.ZoomFactor, basePoint.TileSize); ;
					Position = Projection.MappointToGeoPoint(newCenterPosition);
				}
				previousPosition = mouse;
			} else {
				previousPosition = null;
			}
		}

	}
}
			 
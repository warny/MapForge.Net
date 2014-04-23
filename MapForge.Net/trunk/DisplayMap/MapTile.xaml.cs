using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MapCore;
using MapCore.Model;
using MapCore.Projections;
using RenderTheme;
using RenderTheme.Rule;

namespace DisplayMap
{
	/// <summary>
	/// Logique d'interaction pour MapTile.xaml
	/// </summary>
	public partial class MapTile : UserControl
	{
		private RepresentationConverter projection;
		private Tile baseTile;

		public MapTile(RepresentationConverter projection, Tile baseTile, Brush brush = null)
		{
			this.projection = projection;
			this.baseTile = baseTile;
			this.Width = baseTile.TileSize;
			this.Height = baseTile.TileSize;

			InitializeComponent();
			this.ClipToBounds = true;

			this.Background = brush;
		}

		public Brush Brush
		{
			get { return this.Background; }
			set { this.Background = value; }
		}

	}
}
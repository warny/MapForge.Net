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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DisplayMap;
using MapCore.Model;
using MapCore.Projections;
using RenderTheme;
using RenderTheme.Rule;

namespace RouteWpf
{
	/// <summary>
	/// Logique d'interaction pour MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow ()
		{
			InitializeComponent();

		}

		public override void EndInit()
		{
			base.EndInit();

			XmlRenderTheme xmlRenderTheme = new ExternalRenderTheme(@"H:\projets\perso\Route\java\mapsforge-render-theme\src\main\resources\osmarender\osmarender.xml");
			Theme theme = RenderThemeHandler.GetRenderTheme(xmlRenderTheme);
			MapRenderer renderer = new MapRenderer(new MapDB.Reader.MapDatabase(@"H:\projets\Perso\france.map"), theme);
			displayMap.MapProvider = renderer;
			GeoPoint geoPoint = new GeoPoint("N43.2932E5.3745");//new GeoPoint(45.759723, 4.856);
			displayMap.Position = geoPoint;
			displayMap.ZoomFactor = 18;
		}
	}
}

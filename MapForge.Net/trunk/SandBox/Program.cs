using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapCore.Model;
using MapCore.Projections;
using MapCore.Util;
using RenderTheme.Rule;

namespace SandBox
{
	class Program
	{
		static void Main ( string[] args )
		{
			var projection = new RepresentationConverter(Projections.Mercator);

			var GeoPoint1 = new GeoPoint(45, 5);
			var mappoint1 = projection.GeoPointToMappoint(GeoPoint1, 10);
			var GeoPoint2 = projection.MappointToGeoPoint(mappoint1);
			var tile1 = projection.MappointToTile(mappoint1);
			var tile2 = projection.GeoPointToTile(GeoPoint1, 10);
			Console.WriteLine("GeoPoint1 = {0}", GeoPoint1);
			Console.WriteLine("GeoPoint2 = {0}", GeoPoint2);
			Console.WriteLine("Mappoint1 = {0}", mappoint1);
			Console.WriteLine("Tile1 = {0} {{\n\tpoint1 : {1}  \n\tpoint2 : {2}\n}}", tile1, tile1.MapPoint1, tile1.MapPoint2);
			Console.WriteLine("Tile2 = {0}", tile2);
			Console.WriteLine("Tile1 contains Mappoint1 = {0}", tile1.Contains(mappoint1));
		}
	}
}

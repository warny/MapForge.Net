using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapCore.Model;

namespace MapCore.Projections
{
	public class RepresentationConverter
	{

		/// <summary>
		/// The circumference of the earth at the equator in meters.
		/// </summary>
		public const double EarthCircumference = 40075016.686;

		public int TileSize { get; private set; }

		public IProjectionTransformation Projection { get; private set; }

		public RepresentationConverter ( IProjectionTransformation projection, int tileSize = 256 )
		{
			Projection = projection;
			TileSize = tileSize;
		}

		public MapPoint GeoPointToMappoint ( GeoPoint GeoPoint, byte zoomFactor )
		{
			long zoom = GetMapSize(zoomFactor);

			return new MapPoint(
				Projection.LongitudeToCoordinateX(GeoPoint.Longitude) * zoom,
				Projection.LatitudeToCoordinateY(GeoPoint.Latitude) * zoom,
				zoomFactor,
				TileSize);

		}

		public GeoPoint MappointToGeoPoint ( MapPoint point )
		{
			long zoom = GetMapSize(point.ZoomFactor);

			return new GeoPoint(
				Projection.CoordinateYToLatitude(point.Y / zoom),
				Projection.CoordinateXToLongitude(point.X / zoom));
		}

		public Tile MappointToTile ( MapPoint mappoint )
		{
			long zoom = 1 << mappoint.ZoomFactor;
			return new Tile(
				(long)Math.Min(Math.Max(mappoint.X / TileSize, 0), zoom - 1),
				(long)Math.Min(Math.Max(mappoint.Y / TileSize, 0), zoom - 1),
				mappoint.ZoomFactor,
				TileSize);

		}

		public Tile GeoPointToTile ( GeoPoint GeoPoint, byte zoomLevel )
		{
			return MappointToTile(GeoPointToMappoint(GeoPoint, zoomLevel));
		}

		public long GetMapSize ( byte zoomLevel )
		{
			if (zoomLevel < 0) {
				throw new ArgumentException("zoom level must not be negative: " + zoomLevel, "zoomLevel");
			}
			return (long)TileSize << zoomLevel;
		}

		public double calculateGroundResolution ( double latitude, byte zoomLevel )
		{
			long mapSize = GetMapSize(zoomLevel);
			return Math.Cos(latitude * (Math.PI / 180)) * EarthCircumference / mapSize;
		}

	}
}

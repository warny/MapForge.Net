using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapCore.Projections
{
	public class MercatorProjection : IProjectionTransformation
	{
		#region IProjectionTransformation Membres

		public double MaxLatitude { get { return 85.05112877980659; } }

		public double LongitudeToCoordinateX ( double longitude )
		{
			longitude = longitude % 360;
			return (longitude / 360) + 0.5;
		}

		public double LatitudeToCoordinateY ( double latitude )
		{
			if (latitude > MaxLatitude) latitude = MaxLatitude;
			else if (latitude < -MaxLatitude) latitude = -MaxLatitude;
				//throw new ArgumentException("latitude is superior to maxlatitude", "latitude");

			double sinLatitude = Math.Sin(latitude * (Math.PI / 180));
			return (0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI));
		}

		public double CoordinateXToLongitude ( double coordinateX )
		{
			coordinateX = coordinateX % 1;
			return 360 * (coordinateX - 0.5);
		}

		public double CoordinateYToLatitude ( double coordinateX )
		{
			//if (coordinateX < 0 || coordinateX > 1)
			//	throw new ArgumentException("La valeur doit être comprise entre 0 et 1", "coordinateX");
			double y = 0.5 - coordinateX;
			return 90 - 360 * Math.Atan(Math.Exp(-y * (2 * Math.PI))) / Math.PI;
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapCore.Projections
{
	public interface IProjectionTransformation
	{
		/// <summary>
		/// Tranforme la longitude en coordonnée X dans la projection donnée
		/// </summary>
		/// <param name="longitude">Longitude</param>
		/// <param name="zoomFactor">facteur de zoom</param>
		/// <returns>Coordonnée X</returns>
		double LongitudeToCoordinateX ( double longitude );

		/// <summary>
		/// Tranforme la latitude en coordonnée Y dans la projection donnée
		/// </summary>
		/// <param name="longitude">Matitude</param>
		/// <param name="zoomFactor">facteur de zoom</param>
		/// <returns>Coordonnée Y</returns>
		double LatitudeToCoordinateY ( double latitude );

		/// <summary>
		/// Tranforme la coordonnée X en longitude dans la projection donnée
		/// </summary>
		/// <param name="longitude">Coordonnée X</param>
		/// <returns>Longitude</returns>
		double CoordinateXToLongitude ( double coordinateX );

		/// <summary>
		/// Tranforme la coordonnée Y en latitude dans la projection donnée
		/// </summary>
		/// <param name="longitude">Coordonnée Y</param>
		/// <returns>Latitude</returns>
		double CoordinateYToLatitude ( double coordinateX );
	}
}

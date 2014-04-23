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
namespace MapsUtilities.MapGenerator.DatabaseRenderer
{

	public static class GeometryUtils
	{
		/**
		 * Calculates the center of the minimum bounding rectangle for the given coordinates.
		 * 
		 * @param coordinates
		 *            the coordinates for which calculation should be done.
		 * @return the center coordinates of the minimum bounding rectangle.
		 */
		public static MapPoint calculateCenterOfBoundingBox ( MapPoint[] coordinates )
		{
			double pointXMin = coordinates[0].X;
			double pointXMax = coordinates[0].X;
			double pointYMin = coordinates[0].Y;
			double pointYMax = coordinates[0].Y;

			for (int i = 1; i < coordinates.Length; ++i) {
				MapPoint immutablepoint = coordinates[i];
				if (immutablepoint.X < pointXMin) {
					pointXMin = immutablepoint.X;
				} else if (immutablepoint.X > pointXMax) {
					pointXMax = immutablepoint.X;
				}

				if (immutablepoint.Y < pointYMin) {
					pointYMin = immutablepoint.Y;
				} else if (immutablepoint.Y > pointYMax) {
					pointYMax = immutablepoint.Y;
				}
			}

			return new MapPoint((pointXMin + pointXMax) / 2, (pointYMax + pointYMin) / 2);
		}

		/**
		 * @param way
		 *            the coordinates of the way.
		 * @return true if the given way is closed, false otherwise.
		 */
		public static bool isClosedWay ( MapPoint[] way )
		{
			return way[0].Equals(way[way.Length - 1]);
		}
	}
}
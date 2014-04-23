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
using RenderTheme.Graphics;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
namespace MapsUtilities.MapGenerator
{

	/**
	 * Interface for tile image caches.
	 */
	public interface TileCache : IDictionary<MapGeneratorJob, BitmapImage> 
	{
		/**
		 * @return true if this cache is persistent, false otherwise.
		 */
		bool isPersistent ();

		/**
		 * Sets the persistence of this cache.
		 * 
		 * @param persistent
		 *            the new persistence of this cache.
		 * @throws UnsupportedOperationException
		 *             if this cache does not support persistence.
		 */
		void setPersistent ( bool persistent );
	}
}
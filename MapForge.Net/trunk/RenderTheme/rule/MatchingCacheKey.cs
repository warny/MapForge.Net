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
using System.Collections.Generic;
using MapCore.Model;
namespace RenderTheme.Rule
{


	public class MatchingCacheKey
	{
		private TagList tags;
		private byte zoomLevel;

		public MatchingCacheKey ( TagList tags, byte zoomLevel )
		{
			this.tags = tags;
			this.zoomLevel = zoomLevel;
		}

		public override bool Equals ( object obj )
		{
			if (this == obj) {
				return true;
			} else if (!(obj is MatchingCacheKey)) {
				return false;
			}
			MatchingCacheKey other = (MatchingCacheKey)obj;
			if (this.tags == null) {
				if (other.tags != null) {
					return false;
				}
			} else if (!this.tags.Equals(other.tags)) {
				return false;
			}
			if (this.zoomLevel != other.zoomLevel) {
				return false;
			}
			return true;
		}

		public override int GetHashCode ()
		{
			int prime = 31;
			int result = 1;
			result = prime * result + ((this.tags == null) ? 0 : this.tags.GetHashCode());
			result = prime * result + this.zoomLevel;
			return result;
		}
	}
}

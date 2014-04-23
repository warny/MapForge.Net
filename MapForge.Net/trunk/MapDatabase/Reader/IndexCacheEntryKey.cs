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
using System;
using MapDB.Reader.Header;

namespace MapDB.Reader {

/**
 * An immutable container class which is the key for the index cache.
 */
class IndexCacheEntryKey {
	private int hashCodeValue;
	private long indexBlockNumber;
	private SubFileParameter subFileParameter;

	/**
	 * Creates an immutable key to be stored in a map.
	 * 
	 * @param subFileParameter
	 *            the parameters of the map file.
	 * @param indexBlockNumber
	 *            the number of the index block.
	 */
	public IndexCacheEntryKey(SubFileParameter subFileParameter, long indexBlockNumber) {
		this.subFileParameter = subFileParameter;
		this.indexBlockNumber = indexBlockNumber;
		this.hashCodeValue = calculateHashCode();
	}

	public override bool Equals(Object obj) {
		if (this == obj) {
			return true;
		} else if (!(obj is IndexCacheEntryKey)) {
			return false;
		}
		IndexCacheEntryKey other = (IndexCacheEntryKey) obj;
		if (this.subFileParameter == null && other.subFileParameter != null) {
			return false;
		} else if (this.subFileParameter != null && !this.subFileParameter.Equals(other.subFileParameter)) {
			return false;
		} else if (this.indexBlockNumber != other.indexBlockNumber) {
			return false;
		}
		return true;
	}

	public override int GetHashCode() {
		return this.hashCodeValue;
	}

	/**
	 * @return the hash code of this object.
	 */
	private int calculateHashCode() {
		int result = 7;
		result = 31 * result + ((this.subFileParameter == null) ? 0 : this.subFileParameter.GetHashCode());
		result = 31 * result + (int) (this.indexBlockNumber ^ (this.indexBlockNumber >> 32));
		return result;
	}
}
}
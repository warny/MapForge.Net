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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MapCore.Model
{

	/**
	 * A tag represents an immutable key-value pair.
	 */
	public class Tag : IComparable<Tag>, IComparable<string> {
		private const char KEY_VALUE_SEPARATOR = '=';
		private const long serialVersionUID = 1L;

		/**
		 * The key of this tag.
		 */
		public string Key { get; set; }

		/**
		 * The value of this tag.
		 */
		public string Value { get; set; }

		/**
		 * @param tag
		 *            the textual representation of the tag.
		 */
		public Tag ( string tag )
			: this(tag, tag.IndexOf(KEY_VALUE_SEPARATOR))
		{

		}

		/**
		 * @param key
		 *            the key of the tag.
		 * @param value
		 *            the value of the tag.
		 */
		public Tag ( string key, string value )
		{
			this.Key = key;
			this.Value = value;
		}

		private Tag ( string tag, int splitPosition )
			: this(tag.Substring(0, splitPosition), tag.Substring(splitPosition + 1))
		{
		}

		public override bool Equals ( Object obj )
		{
			if (this == obj) {
				return true;
			} else if (!(obj is Tag)) {
				return false;
			}
			Tag other = (Tag)obj;
			if (this.Key == null) {
				if (other.Key != null) {
					return false;
				}
			} else if (!this.Key.Equals(other.Key)) {
				return false;
			} else if (this.Value == null) {
				if (other.Value != null) {
					return false;
				}
			} else if (!this.Value.Equals(other.Value)) {
				return false;
			}
			return true;
		}

		public override int GetHashCode ()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((this.Key == null) ? 0 : this.Key.GetHashCode());
			result = prime * result + ((this.Value == null) ? 0 : this.Value.GetHashCode());
			return result;
		}

		public override string ToString ()
		{
			return string.Format("{0}={1}", this.Key, this.Value);
		}

		#region IComparable<Tag> Membres

		public int CompareTo ( Tag other )
		{
			return this.Key.CompareTo(other.Key);
		}

		#endregion

		#region IComparable<string> Membres

		public int CompareTo ( string other )
		{
			return this.Key.CompareTo(other);
		}

		#endregion
	}
}
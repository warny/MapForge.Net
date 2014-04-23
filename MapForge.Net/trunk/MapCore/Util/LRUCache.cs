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

//import java.util.LinkedHashMap;
//import java.util.Map;

using System;
using System.Runtime.Serialization;
using System.Text;
using System.Collections.Generic;

namespace MapCore.Util
{


	/**
	 * An LRUCache with a fixed size and an access-order policy. Old mappings are automatically removed from the cache when
	 * new mappings are added. This implementation uses an {@link LinkedHashMap} internally.
	 * 
	 * @param <K>
	 *            the type of the map key, see {@link Map}.
	 * @param <V>
	 *            the type of the map value, see {@link Map}.
	 */
	public class LRUCache<TKey, TValue> : System.Collections.Generic.IDictionary<TKey, TValue>
	{
		private Dictionary<TKey, int> index;
		private KeyValuePair<TKey, TValue>[] values;
		private int cursor;

		/**
		 * @param capacity
		 *            the maximum capacity of this cache.
		 * @throws IllegalArgumentException
		 *             if the capacity is negative.
		 */
		public LRUCache ( int capacity )
		{
			values = new KeyValuePair<TKey, TValue>[capacity];
			cursor = 0;
			index = new Dictionary<TKey, int>();
		}


		#region IDictionary<TKey,TValue> Membres

		public void Add ( TKey key, TValue value )
		{
			this.Add(new KeyValuePair<TKey, TValue>(key, value));
		}

		public bool ContainsKey ( TKey key )
		{
			return index.ContainsKey(key);
		}

		public ICollection<TKey> Keys
		{
			get { return index.Keys; }
		}

		public bool Remove ( TKey key )
		{
			int elementIndex;
			if (!index.TryGetValue(key, out elementIndex)) return false;
			values[elementIndex] = new KeyValuePair<TKey, TValue>();
			return true;
		}

		public bool TryGetValue ( TKey key, out TValue value )
		{
			int elementIndex;
			if (!index.TryGetValue(key, out elementIndex)) {
				value = default(TValue);
				return false;
			}
			value = values[elementIndex].Value;
			return true;
		}

		public ICollection<TValue> Values
		{
			get
			{
				List<TValue> values = new List<TValue>();
				foreach (var item in index) {
					values.Add(this.values[item.Value].Value); 
				}
				return values;
			}
		}

		public TValue this[TKey key]
		{
			get
			{
				return values[index[key]].Value;
			}
			set
			{
				int elementIndex;
				if (!index.TryGetValue(key, out elementIndex)) {
					this.Add(key, value);
				} else {
					values[elementIndex] = new KeyValuePair<TKey, TValue>(key, value);
				}
			}
		}

		#endregion

		#region ICollection<KeyValuePair<TKey,TValue>> Membres

		public void Add ( KeyValuePair<TKey, TValue> item )
		{
			var element = values[cursor];
			if (element.Key != null) index.Remove(element.Key);
			index.Add(item.Key, cursor);
			values[cursor] = item;
			cursor = (cursor + 1) % values.Length;
		}

		public void Clear ()
		{
			index.Clear();
			values = new KeyValuePair<TKey, TValue>[values.Length];
			cursor = 0;
		}

		public bool Contains ( KeyValuePair<TKey, TValue> item )
		{
			int elementIndex;
			if (!index.TryGetValue(item.Key, out elementIndex)) return false;
			return values[elementIndex].Value.Equals(item.Value);
		}

		public void CopyTo ( KeyValuePair<TKey, TValue>[] array, int arrayIndex )
		{
			foreach (var item in this) {
				if (arrayIndex > array.Length) return;
				array[arrayIndex] = item;
				arrayIndex++;
			}
		}

		public int Count
		{
			get { return index.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove ( KeyValuePair<TKey, TValue> item )
		{
			if (!this.Contains(item)) return false;
			return this.Remove(item.Key);
		}

		#endregion

		#region IEnumerable<KeyValuePair<TKey,TValue>> Membres

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
		{
			return new Enumerator<TKey, TValue>(index, values);
		}

		#endregion

		#region IEnumerable Membres

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator();
		}

		#endregion

		private class Enumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
		{
			private Dictionary<TKey, int> index;
			private KeyValuePair<TKey, TValue>[] values;

			private IEnumerator<KeyValuePair<TKey, int>> indexEnumerator;

			public Enumerator ( Dictionary<TKey, int> index, KeyValuePair<TKey, TValue>[] values )
			{
				this.index = index;
				this.values = values;

				this.indexEnumerator = index.GetEnumerator();
			}

			#region IEnumerator<KeyValuePair<TKey,TValue>> Membres

			public KeyValuePair<TKey, TValue> Current
			{
				get { return values[indexEnumerator.Current.Value]; }
			}

			#endregion

			#region IDisposable Membres

			public void Dispose ()
			{
				indexEnumerator.Dispose();
			}

			#endregion

			#region IEnumerator Membres

			object System.Collections.IEnumerator.Current
			{
				get { return this.Current; }
			}

			public bool MoveNext ()
			{
				return indexEnumerator.MoveNext();
			}

			public void Reset ()
			{
				indexEnumerator.Reset();
			}

			#endregion
		}
	}
}
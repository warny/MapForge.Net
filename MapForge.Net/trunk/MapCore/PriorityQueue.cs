using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace MapCore
{
	public class PriorityQueue<T> : ICollection<T> where T : class
	{
		private class Key<TKey> : IComparable<Key<TKey>>
		{
			private TKey Value;
			private IComparer<TKey> comparer;

			public Key (IComparer<TKey> comparer, TKey value)
			{
				this.Value = value;
				this.comparer = comparer;
			}

			#region IComparable<Key<T>> Membres

			public int CompareTo ( Key<TKey> other )
			{
				return comparer.Compare(Value, other.Value);
			}

			#endregion
		}

		public PriorityQueue (IComparer<T> comparer)
		{
			this.comparer = comparer;
			innerList = new Dictionary<Key<T>, T>();
		}

		public PriorityQueue (int capacity,  IComparer<T> comparer )
		{
			this.comparer = comparer;
			innerList = new Dictionary<Key<T>, T>(capacity);
		}

		private Dictionary<Key<T>, T> innerList;
		private IComparer<T> comparer;

		private Key<T> key ( T value )
		{
			return new Key<T>(this.comparer, value);
		}

		#region ICollection<T> Membres

		public void Add ( T item )
		{
			innerList.Add(key(item), item);
		}

		public void AddRange ( params T[] items )
		{
			AddRange((IEnumerable<T>)items);
		}

		public void AddRange ( IEnumerable<T> items )
		{
			foreach (var item in items) {
				Add(item);
			}
		}

		public T Peek ()
		{
			KeyValuePair<Key<T>, T> item;
			using (var e = innerList.GetEnumerator()) {
				if (e.MoveNext()) {
					item = e.Current;
				} else return null;
			}
			return item.Value;
		}

		public T Pool ()
		{
			KeyValuePair<Key<T>, T> item;
			using (var e = innerList.GetEnumerator()) {
				if (e.MoveNext()) {
					item = e.Current;
				} else return null;
			}
			innerList.Remove(item.Key);
			return item.Value;
		}

		public void Clear ()
		{
			innerList.Clear();
		}

		public bool Contains ( T item )
		{
			return innerList.ContainsKey(key(item));
		}

		public void CopyTo ( T[] array, int arrayIndex )
		{
			int maxIndex = array.Length;
			foreach (var element in innerList) {
				array[arrayIndex] = element.Value;
				arrayIndex++;
				if (arrayIndex >= maxIndex) break;
			}
		}

		public int Count
		{
			get { return innerList.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove ( T item )
		{
			return innerList.Remove(key(item));
		}

		#endregion

		#region IEnumerable<T> Membres

		public IEnumerator<T> GetEnumerator ()
		{
			return innerList.Values.GetEnumerator();
		}

		#endregion

		#region IEnumerable Membres

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return innerList.Values.GetEnumerator();
		}

		#endregion
	}
}

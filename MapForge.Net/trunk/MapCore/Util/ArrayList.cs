using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapCore.Util
{
	public class ArrayList<T> : ICollection<T>, IReadOnlyDictionary<int, T>
	{
		List<T> innerList;

		public ArrayList ()
		{
			innerList = new List<T>();
		}

		public ArrayList ( IEnumerable<T> values ) : this()
		{
			AddRange(values);
		}

		public ArrayList ( int capacity )
		{
			innerList = new List<T>(capacity);
		}

		#region ICollection<T> Membres

		public void Add ( T item )
		{
			innerList.Add(item);
		}

		public void AddRange ( IEnumerable<T> values )
		{
			innerList.AddRange(values);
		}

		public void Clear ()
		{
			innerList.Clear();
		}

		public bool Contains ( T item )
		{
			return innerList.Contains(item);
		}

		public void CopyTo ( T[] array, int arrayIndex )
		{
			innerList.CopyTo(array, arrayIndex);
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
			return innerList.Remove(item);
		}

		#endregion

		#region IEnumerable<T> Membres

		public IEnumerator<T> GetEnumerator ()
		{
			return innerList.GetEnumerator();
		}

		#endregion

		#region IEnumerable Membres

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return innerList.GetEnumerator();
		}

		#endregion

		#region IReadOnlyDictionary<int,T> Membres

		public bool ContainsKey ( int key )
		{
			return key >= 0 && key < innerList.Count;
		}

		public IEnumerable<int> Keys
		{
			get { 
				int count = innerList.Count;
				for (int key = 0; key < count; key++) yield return key;
			}
		}

		public bool TryGetValue ( int key, out T value )
		{
			if (ContainsKey(key)) {
				value = innerList[key];
				return true;
			} else {
				value = default(T);
				return false;
			}
		}

		public IEnumerable<T> Values
		{
			get { return innerList; }
		}

		public T this[int key]
		{
			get { return innerList[key]; }
			set { innerList[key] = value; }
		}

		#endregion

		#region IEnumerable<KeyValuePair<int,T>> Membres

		IEnumerator<KeyValuePair<int, T>> IEnumerable<KeyValuePair<int, T>>.GetEnumerator ()
		{
			int key = 0;
			foreach (var item in innerList) {
				yield return new KeyValuePair<int, T>(key, item);
				key++;
			}
		}

		#endregion
	}
}

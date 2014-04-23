using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MapCore.Util;

namespace MapCore.Model
{
	public class TagList : ICollection<Tag>
	{
		private static readonly Regex matchRegEx = new Regex(@"^(?<tagNames>.+?)(\=(?<tagValues>.+))?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

		private Dictionary<string, List<string>> innerContainer;

		public TagList ()
		{
			innerContainer = new Dictionary<string, List<string>>();
		}

		public TagList ( IEnumerable<Tag> tags ) : this()
		{
			this.AddRange(tags);
		}

		public TagList ( int capacity )
		{
			innerContainer = new Dictionary<string, List<string>>(capacity);
		}

		public bool Match ( string pattern )
		{
			var m = matchRegEx.Match(pattern);
			var names = m.Groups["tagNames"].Value.Split('|');
			var values = m.Groups["tagValues"].Value.Split('|');

			return Match(names, values);
		}

		public bool Match ( string names, string values )
		{
			var patternNames = names.Split('|');
			var patternValues = string.IsNullOrWhiteSpace(values) ? null : values.Split('|');

			return Match(patternNames, patternValues);
		}

		public bool Match ( string names, IEnumerable<string> patternValues )
		{
			var patternNames = names.Split('|');

			return Match(patternNames, patternValues);
		}

		public bool Match ( IEnumerable<string> patternNames, IEnumerable<string> patternValues = null )
		{
			var simpleNames = patternNames.Where(n => !(n.Contains("*") || n.Contains("?"))).Select(n => n.Trim()).ToArray();
			var wildcardNames = patternNames.Where(n => n.Contains("*") || n.Contains("?")).Select(n => (Wildcard)n.Trim()).ToArray();

			var patternWilcards = patternValues != null ? patternValues.Select(v => (Wildcard)v.Trim()) : null;

			foreach (var name in simpleNames) {
				List<string> testedValues;
				if (!innerContainer.TryGetValue(name, out testedValues)) continue;
				if (MatchValue(testedValues, patternWilcards)) return true;
			}

			foreach (var item in innerContainer) {
				foreach (var name in wildcardNames) {
					if (item.Key == name) {
						List<string> testedValues = item.Value;
						if (MatchValue(testedValues, patternWilcards)) return true;
						break;
					}
				}
			}
			return false;
		}

		private bool MatchValue ( List<string> values, IEnumerable<Wildcard> wildCards )
		{
			if (wildCards == null || !wildCards.Any()) return true;

			foreach (var value in values) {
				foreach (var wildCard in wildCards) {
					if (value == wildCard) return true;
				}
			}

			return false;
		}

		#region ICollection<Tag> Membres

		public void Add ( Tag item )
		{
			List<string> content;
			if (!innerContainer.TryGetValue(item.Key, out content)) {
				content = new List<string>();
				innerContainer.Add(item.Key, content);
			}
			if (!content.Contains(item.Value))
				content.Add(item.Value);
		}

		public void AddRange ( IEnumerable<Tag> tags )
		{
			foreach (var tag in tags) Add(tag);
		}

		public void Clear ()
		{
			innerContainer.Clear();
		}

		public bool Contains ( Tag item )
		{

			return innerContainer.ContainsKey(item.Key) && innerContainer[item.Key].Contains(item.Value);
		}

		public void CopyTo ( Tag[] array, int arrayIndex )
		{
			foreach (var tagkey in innerContainer) {
				foreach(var tagvalue in tagkey.Value) {
					if (arrayIndex>array.Length ) return;
					array[arrayIndex] = new Tag(tagkey.Key, tagvalue);
					arrayIndex++;
				}
			}
		}

		public int Count
		{
			get { return innerContainer.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove ( Tag item )
		{
			return innerContainer.Remove(item.Key);
		}

		#endregion

		private class TagEnumerator : IEnumerator<Tag>
		{
			private IEnumerator<KeyValuePair<string, List<string>>> tagKeys = null;
			private IEnumerator<string> tagValues = null;
			private Tag currentValue;

			internal TagEnumerator (Dictionary<string, List<string>> dictionary)
			{
				tagKeys = dictionary.GetEnumerator();
			}

			#region IEnumerator<Tag> Membres

			public Tag Current
			{
				get { return currentValue; }
			}

			#endregion

			#region IDisposable Membres

			public void Dispose ()
			{
				if (tagValues != null) tagValues.Dispose();
				tagKeys.Dispose();
			}

			#endregion

			#region IEnumerator Membres

			object System.Collections.IEnumerator.Current
			{
				get { throw new NotImplementedException(); }
			}

			public bool MoveNext ()
			{
				if (tagValues == null || !tagValues.MoveNext()) {
					if (tagValues != null) tagValues.Dispose();
					if (!tagKeys.MoveNext()) return false;
					tagValues = tagKeys.Current.Value.GetEnumerator();
					tagValues.MoveNext();
				}
				currentValue = new Tag(tagKeys.Current.Key, tagValues.Current);
				return true;
			}

			public void Reset ()
			{
				if (tagValues!=null) tagValues.Dispose();
				tagKeys.Reset();
			}

			#endregion
		}

		#region IEnumerable<Tag> Membres

		public IEnumerator<Tag> GetEnumerator ()
		{
			return new TagEnumerator(innerContainer);
		}

		#endregion

		#region IEnumerable Membres

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return innerContainer.Values.GetEnumerator();
		}

		#endregion

		public bool ContainsKey ( string key )
		{
			return innerContainer.ContainsKey(key);
		}

		public List<string> this[string key]
		{
			get {
				List<string> value;
				if (innerContainer.TryGetValue(key, out value)) {
					return value;
				}
				return new List<string>(); 
			}
		}

		public override string ToString ()
		{
			return string.Join(" ; ", this.Select(t => t.ToString()));
		}

		public override int GetHashCode ()
		{
			unchecked {
				int hashCode = 0;
				foreach (var item in innerContainer) {
					hashCode = (hashCode << 5) + 3 + hashCode ^ (item.Key + "=").GetHashCode();
					foreach (var value in item.Value) {
						hashCode = (hashCode << 5) + 3 + hashCode ^ value.GetHashCode();						
					}
				}
				return hashCode;
			}
		}
	}
}

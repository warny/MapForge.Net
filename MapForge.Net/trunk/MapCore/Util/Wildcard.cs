using System;
using System.Collections.Generic;
using System.Text;

namespace MapCore.Util
{

	/// <summary>
	/// Classe permettant de gérer la comparaison de chaînes par wildcard
	/// </summary>
	public class Wildcard : IComparable<string>
	{
		private string wildcard;

		/// <summary>
		/// Créé un wildcard
		/// </summary>
		/// <param name="wildcard"></param>
		public Wildcard ( string wildcard )
		{
			this.wildcard = wildcard;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals ( object obj )
		{
			if (obj is Wildcard) return this.wildcard == ((Wildcard)obj).wildcard;
			if (obj is string) return this == (string)obj;

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode ()
		{
			return wildcard.GetHashCode();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString ()
		{
			return wildcard;
		}

		#region operateurs
		/// <summary>
		/// Conversion implicite de chaîne en wildcard
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static implicit operator Wildcard ( string value )
		{
			return new Wildcard(value);
		}

		public static bool operator == ( Wildcard wildcard, string value )
		{
			return Compare(value, wildcard.wildcard);
		}

		public static bool operator != ( Wildcard wildcard, string value )
		{
			return !Compare(value, wildcard.wildcard);
		}

		public static bool operator == ( string value, Wildcard wildcard )
		{
			return Compare(value, wildcard.wildcard);
		}

		public static bool operator != ( string value, Wildcard wildcard )
		{
			return !Compare(value, wildcard.wildcard);
		}
		#endregion

		#region IComparable<string> Membres

		/// <summary>
		/// Compares the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="wildcard">The wildcard.</param>
		/// <returns></returns>
		public static bool Compare ( string value, string wildcard )
		{
			wildcard = wildcard.ToLower();
			value = value.ToLower();

			int valueIndex = 0, wildcardIndex = 0;
			int valueNext = 0, wildcardNext = 0;

			while (valueIndex < value.Length && wildcardIndex < wildcard.Length && wildcard[wildcardIndex] != '*') {
				if (value[valueIndex] != wildcard[wildcardIndex] && wildcard[wildcardIndex] != '?') {
					return false;
				}
				wildcardIndex++;
				valueIndex++;
			}

			while (wildcardIndex < wildcard.Length && valueIndex < value.Length) {
				if (wildcard[wildcardIndex] == '*') {
					wildcardNext = wildcardIndex;
					wildcardIndex++;
					if (wildcardIndex >= wildcard.Length) {
						return true;
					}
					valueNext = valueNext + 1;
				} else if (value[valueIndex] == wildcard[wildcardIndex] || wildcard[wildcardIndex] == '?') {
					wildcardIndex++;
					valueIndex++;
					if (wildcardIndex >= wildcard.Length && valueIndex < value.Length && wildcard[wildcardNext] == '*') wildcardIndex = wildcardNext + 1;
				} else {
					wildcardIndex = wildcardNext + 1;
					valueIndex = valueNext++;
				}
			}

			while (wildcardIndex < wildcard.Length && wildcard[wildcardIndex] == '*') wildcardIndex++;

			return (wildcardIndex >= wildcard.Length && valueIndex >= value.Length);
		}


		/// <summary>
		/// Compare l'objet en cours à un autre objet du même type.
		/// </summary>
		/// <param name="other">Objet à comparer avec cet objet.</param>
		/// <returns>
		/// Entier signé 32 bits qui indique l'ordre relatif des objets comparés. La valeur de retour a les significations suivantes :
		/// Valeur
		/// Signification
		/// Inférieure à zéro
		/// Cet objet est inférieur au paramètre <paramref name="other"/>.
		/// Zéro
		/// Cet objet est égal à <paramref name="other"/>.
		/// Supérieure à zéro
		/// Cet objet est supérieur à <paramref name="other"/>.
		/// </returns>
		public int CompareTo ( string other )
		{
			return Compare(other, wildcard) ? 0 : 1;
		}

		#endregion

		public static void TrimPatterns ( List<string> patterns )
		{
			List<string> toBeRemoved = new List<string>();

			for (int i = 0; i < patterns.Count; i++) {
				for (int j = i + 1; j < patterns.Count; j++) {
					string pattern1 = patterns[i];
					string pattern2 = patterns[j];

					Wildcard wPattern1 = pattern1;
					Wildcard wPattern2 = pattern2;

					if (wPattern1 == pattern2) {
						toBeRemoved.Add(pattern2);
						break;
					}
					if (wPattern2 == pattern1) {
						toBeRemoved.Add(pattern1);
					}
				}
			}
			foreach (var item in toBeRemoved) {
				patterns.Remove(item);
			}
		}



	}
}

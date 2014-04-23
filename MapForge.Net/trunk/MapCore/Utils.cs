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
using System.Text.RegularExpressions;

namespace MapCore
{
	public static class Utils
	{
		public static bool Intersects ( this Rect rect1, Rect rect2 )
		{
			return (rect1.Bottom <= rect2.Top && rect1.Top >= rect2.Bottom && rect1.Left <= rect2.Right && rect1.Right >= rect1.Left);
		}

		public static void RemoveAll<T> ( this ICollection<T> collection, Func<T, bool> remove )
		{
			Queue<T> toRemove = new Queue<T>();
			foreach (var element in collection) {
				if (remove(element)) {
					toRemove.Enqueue(element);
				}
			}
			while (toRemove.Count > 0) {
				collection.Remove(toRemove.Dequeue());
			}
		}

		public static Color ParseColor( string color )
		{
			if (color.StartsWith("#")) {
				string A, r, g, b;
				switch (color.Length) {
					case 4: // #rgb
						A = "FF";
						r = new string(color[1], 2);
						g = new string(color[2], 2);
						b = new string(color[3], 2);
						break;
					case 5: //#Argb
						A = new string(color[1], 2);
						r = new string(color[2], 2);
						g = new string(color[3], 2);
						b = new string(color[4], 2);
						break;
					case 7: //#rrggbb
						A = "FF";
						r = color.Substring(1, 2);
						g = color.Substring(3, 2);
						b = color.Substring(5, 2);
						break;
					case 9: //#rrggbb
						A = color.Substring(1, 2);
						r = color.Substring(3, 2);
						g = color.Substring(5, 2);
						b = color.Substring(7, 2);
						break;
					default:
						throw new ArgumentException(string.Format("{0} n'est pas une couleur valide", color), "color");
				}
				return Color.FromArgb(
					byte.Parse(A, System.Globalization.NumberStyles.AllowHexSpecifier),
					byte.Parse(r, System.Globalization.NumberStyles.AllowHexSpecifier),
					byte.Parse(g, System.Globalization.NumberStyles.AllowHexSpecifier),
					byte.Parse(b, System.Globalization.NumberStyles.AllowHexSpecifier));

			} else {
				Type colorsType = typeof(Colors);
				var colorProperty = colorsType.GetProperty(color, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase);
				if (colorProperty == null)
					throw new ArgumentException(string.Format("{0} n'est pas une couleur valide", color), "color");

				return (Color)colorProperty.GetValue(null, null);
			}
		}


		public static bool IsIdentity ( this CompositeTransform transform )
		{
			return transform.Rotation == 0
			&& transform.ScaleX == 1
			&& transform.ScaleY == 1
			&& transform.SkewX == 1
			&& transform.SkewY == 1
			&& transform.TranslateX == 0
			&& transform.TranslateY == 0;
		}
	}
}

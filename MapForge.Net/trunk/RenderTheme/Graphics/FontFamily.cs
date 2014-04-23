using System.Windows;
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
using System.Windows.Media;
namespace RenderTheme.Graphics
{

	public static class FontFamily
	{
		public static Typeface Parse ( string value )
		{
			switch (value.ToUpper()) {
				case "DEFAULT":
					break;
				case "DEFAULT_BOLD":
					break;
				case "MONOSPACE":
					break;
				case "SANS_SERIF":
					break;
				case "SERIF":
					break;
			}
			throw new ArgumentOutOfRangeException("value");
		}

		public static Typeface DEFAULT { get { return new Typeface("Arial"); } }
		public static Typeface DEFAULT_BOLD { get { return new Typeface(new System.Windows.Media.FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal); } }
		public static Typeface MONOSPACE { get { return new Typeface("Consola"); } }
		public static Typeface SANS_SERIF { get { return new Typeface("Arial"); } }
		public static Typeface SERIF { get { return new Typeface("Times new roman"); } }
	}
}

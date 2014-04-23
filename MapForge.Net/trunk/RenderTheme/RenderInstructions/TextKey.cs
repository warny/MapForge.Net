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
using MapCore.Model;
using System.Linq;
namespace RenderTheme.RenderInstructions
{

	public class TextKey
	{
		private static string KEY_ELEVATION = "ele";
		private static string KEY_HOUSENUMBER = "addr:housenumber";
		private static string KEY_NAME = "name";
		private static string KEY_REF = "ref";
		private static TextKey TEXT_KEY_ELEVATION = new TextKey(KEY_ELEVATION);
		private static TextKey TEXT_KEY_HOUSENUMBER = new TextKey(KEY_HOUSENUMBER);
		private static TextKey TEXT_KEY_NAME = new TextKey(KEY_NAME);
		private static TextKey TEXT_KEY_REF = new TextKey(KEY_REF);

		public static TextKey getInstance ( string key )
		{
			if (KEY_ELEVATION.Equals(key)) {
				return TEXT_KEY_ELEVATION;
			} else if (KEY_HOUSENUMBER.Equals(key)) {
				return TEXT_KEY_HOUSENUMBER;
			} else if (KEY_NAME.Equals(key)) {
				return TEXT_KEY_NAME;
			} else if (KEY_REF.Equals(key)) {
				return TEXT_KEY_REF;
			} else {
				throw new ArgumentException("invalid key: " + key, "key");
			}
		}

		private string key;

		private TextKey ( string key )
		{
			this.key = key;
		}

		public string getValue ( TagList tags )
		{
			var tag = tags.FirstOrDefault(t => t.Key == this.key);
			return tag == null ? null : tag.Value;
		}
	}
}

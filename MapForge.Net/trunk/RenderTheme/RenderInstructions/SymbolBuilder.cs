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
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Windows.Media;
namespace RenderTheme.RenderInstructions
{


	/**
	 * A builder for {@link Symbol} instances.
	 */
	public class SymbolBuilder
	{
		static string SRC = "src";

		public ImageSource BitmapImage;

		public SymbolBuilder ( string elementName, IEnumerable<XAttribute> attributes,
				string relativePathPrefix )
		{
			extractValues(elementName, attributes, relativePathPrefix);
		}

		/**
		 * @return a new {@code Symbol} instance.
		 */
		public Symbol build ()
		{
			return new Symbol(this);
		}

		private void extractValues ( string elementName, IEnumerable<XAttribute> attributes,
				string relativePathPrefix )
		{
			foreach (XAttribute attribute in attributes) {
				string name = attribute.Name.LocalName;
				string value = attribute.Value;

				if (SRC.Equals(name)) {
					this.BitmapImage = XmlUtils.createBitmap(relativePathPrefix, value);
				} else {
					throw XmlUtils.createSAXException(elementName, name, value);
				}
			}

			XmlUtils.checkMandatoryAttribute(elementName, SRC, this.BitmapImage);
		}
	}
}
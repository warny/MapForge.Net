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
using RenderTheme.Graphics;
using System.IO;
using System.Xml;
using System.Text;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Windows.Media;
namespace RenderTheme
{

	public static class XmlUtils
	{
		private static string PREFIX_FILE = "file:";
		private static string PREFIX_JAR = "jar:";

		public static void checkMandatoryAttribute ( string elementName, string attributeName, Object attributeValue )
		{
			if (attributeValue == null) {
				throw new XmlException("missing attribute '" + attributeName + "' for element: " + elementName);
			}
		}

		public static ImageSource createBitmap ( string relativePathPrefix, string src )
		{
			if (src == null || src.Length == 0) {
				// no image source defined
				return null;
			}

			using (Stream stream = createInputStream(relativePathPrefix, src)) {
				BitmapImage BitmapImage = new BitmapImage();
#if WINDOWS_PHONE
				BitmapImage.SetSource(stream);
#else
				BitmapImage.BeginInit();
				BitmapImage.StreamSource = stream;
				BitmapImage.EndInit();
#endif
				return BitmapImage;
			}
		}

		public static XmlException createSAXException ( string element, string name, string value )
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("unknown attribute in element '");
			stringBuilder.Append(element);
			stringBuilder.Append("': ");
			stringBuilder.Append(name);
			stringBuilder.Append('=');
			stringBuilder.Append(value);

			return new XmlException(stringBuilder.ToString());
		}

		public static byte parseNonNegativeByte ( string name, string value )
		{
			byte parsedByte = Byte.Parse(value, CultureInfo.InvariantCulture);
			checkForNegativeValue(name, parsedByte);
			return parsedByte;
		}

		public static float parseNonNegativeFloat ( string name, string value )
		{
			float parsedFloat = float.Parse(value, CultureInfo.InvariantCulture);
			checkForNegativeValue(name, parsedFloat);
			return parsedFloat;
		}

		public static int parseNonNegativeInteger ( string name, string value )
		{
			int parsedInt = int.Parse(value, CultureInfo.InvariantCulture);
			checkForNegativeValue(name, parsedInt);
			return parsedInt;
		}

		private static void checkForNegativeValue ( string name, float value )
		{
			if (value < 0) {
				throw new XmlException("Attribute '" + name + "' must not be negative: " + value);
			}
		}

		private static Stream createInputStream ( string relativePathPrefix, string src )
		{
			if (src.StartsWith(PREFIX_JAR)) {
				string absoluteName = getAbsoluteName(relativePathPrefix, src.Substring(PREFIX_JAR.Length));
				Stream Stream = new FileStream(absoluteName, FileMode.Open, FileAccess.Read);
				if (Stream == null) {
					throw new FileNotFoundException("resource not found: " + absoluteName);
				}
				return Stream;
			} else if (src.StartsWith(PREFIX_FILE)) {
				FileInfo file = getFile(relativePathPrefix, src.Substring(PREFIX_FILE.Length));
				if (!file.Exists) {
					throw new FileNotFoundException("file does not exist: " + file.FullName);
				}
				return file.OpenRead();
			}

			throw new FileNotFoundException("invalid BitmapImage source: " + src);
		}

		private static string getAbsoluteName ( string relativePathPrefix, string name )
		{
			if (name[0] == '/') {
				return name;
			}
			return relativePathPrefix + name;
		}

		private static FileInfo getFile ( string parentPath, string pathName )
		{
			if (pathName[0] == Path.PathSeparator) {
				return new FileInfo(pathName);
			}
			return new FileInfo(Path.Combine(parentPath, pathName));
		}

	}
}
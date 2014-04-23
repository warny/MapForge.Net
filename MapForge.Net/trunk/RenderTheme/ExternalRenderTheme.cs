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
using System.IO;
namespace RenderTheme
{

	/**
	 * An ExternalRenderTheme allows for customizing the rendering style of the map via an XML file.
	 */
	public class ExternalRenderTheme : XmlRenderTheme
	{
		private long lastModifiedTime;
		private FileInfo renderThemeFile;

		public ExternalRenderTheme ( string filename )
			: this(new FileInfo(filename))
		{
		}

		/**
		 * @param renderThemeFile
		 *            the XML render theme file.
		 * @
		 *             if the file does not exist or cannot be read.
		 */
		public ExternalRenderTheme ( FileInfo renderThemeFile )
		{
			if (!renderThemeFile.Exists) {
				throw new FileNotFoundException("file does not exist: " + renderThemeFile.FullName);
			}

			this.lastModifiedTime = renderThemeFile.LastWriteTime.Ticks;
			if (this.lastModifiedTime == 0L) {
				throw new FileNotFoundException("cannot read last modified time: " + renderThemeFile.FullName);
			}
			this.renderThemeFile = renderThemeFile;
		}

		public override bool Equals ( object obj )
		{
			if (this == obj) {
				return true;
			} else if (!(obj is ExternalRenderTheme)) {
				return false;
			}
			ExternalRenderTheme other = (ExternalRenderTheme)obj;
			if (this.lastModifiedTime != other.lastModifiedTime) {
				return false;
			}
			if (this.renderThemeFile == null) {
				if (other.renderThemeFile != null) {
					return false;
				}
			} else if (!this.renderThemeFile.Equals(other.renderThemeFile)) {
				return false;
			}
			return true;
		}

		public string RelativePathPrefix { get { return this.renderThemeFile.DirectoryName; } }

		public Stream OpenStream() { 
			return this.renderThemeFile.OpenRead(); 
		}

		public override int GetHashCode ()
		{
			int prime = 31;
			int result = 1;
			result = prime * result + (int)(this.lastModifiedTime ^ (this.lastModifiedTime >> 32));
			result = prime * result + ((this.renderThemeFile == null) ? 0 : this.renderThemeFile.GetHashCode());
			return result;
		}
	}
}
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
using System.IO;
using System.Threading;

namespace RenderTheme
{

	/**
	 * Enumeration of all internal rendering themes.
	 */
	public class InternalRenderTheme : XmlRenderTheme
	{
		/**
		 * A render-theme similar to the OpenStreetMap Osmarender style.
		 * 
		 * @see <a href="http://wiki.openstreetmap.org/wiki/Osmarender">Osmarender</a>
		 */
		public static InternalRenderTheme OSMARENDER = new InternalRenderTheme("/osmarender/", "osmarender.xml");

		private string absolutePath;
		private string file;

		private InternalRenderTheme ( string absolutePath, string file )
		{
			this.absolutePath = absolutePath;
			this.file = file;
		}

		public string RelativePathPrefix { get { return this.absolutePath; } }

		public Stream OpenStream()
		{
			return new FileStream(Path.Combine(this.absolutePath, this.file), FileMode.Open, FileAccess.Read); 
		}
	}
}
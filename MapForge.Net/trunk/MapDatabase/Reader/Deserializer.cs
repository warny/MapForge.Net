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
namespace MapDB.Reader
{

	/**
	 * An utility class to convert byte arrays to numbers.
	 */
	internal static class Deserializer
	{
		/**
		 * Converts five bytes of a byte array to an unsigned long.
		 * <p>
		 * The byte order is big-endian.
		 * 
		 * @param buffer
		 *            the byte array.
		 * @param offset
		 *            the offset in the array.
		 * @return the long value.
		 */
		public static long GetFiveBytesLong ( byte[] buffer, long offset )
		{
			return GetLong(buffer, offset, 5);
		}

		public static long GetLong ( byte[] buffer, long offset, byte size )
		{
			if (size > 8) throw new ArgumentOutOfRangeException("Size can't exceed long byte representation", "size"); 
			long ret = 0;
			for (int i = 0; i < size; i++) {
				ret <<= 8;
				ret |= buffer[offset + i];
			}
			return ret;
		}

		/**
		 * Converts four bytes of a byte array to a signed int.
		 * <p>
		 * The byte order is big-endian.
		 * 
		 * @param buffer
		 *            the byte array.
		 * @param offset
		 *            the offset in the array.
		 * @return the int value.
		 */
		public static int GetInt ( byte[] buffer, long offset )
		{
			return buffer[offset] << 24 | (buffer[offset + 1] & 0xff) << 16 | (buffer[offset + 2] & 0xff) << 8
					| (buffer[offset + 3] & 0xff);
		}

		/**
		 * Converts eight bytes of a byte array to a signed long.
		 * <p>
		 * The byte order is big-endian.
		 * 
		 * @param buffer
		 *            the byte array.
		 * @param offset
		 *            the offset in the array.
		 * @return the long value.
		 */
		public static long GetLong ( byte[] buffer, long offset )
		{
			return GetLong(buffer, offset, 8);
		}

		/**
		 * Converts two bytes of a byte array to a signed int.
		 * <p>
		 * The byte order is big-endian.
		 * 
		 * @param buffer
		 *            the byte array.
		 * @param offset
		 *            the offset in the array.
		 * @return the int value.
		 */
		public static short GetShort ( byte[] buffer, long offset )
		{
			return (short)(buffer[offset] << 8 | (buffer[offset + 1] & 0xff));
		}
	}
}
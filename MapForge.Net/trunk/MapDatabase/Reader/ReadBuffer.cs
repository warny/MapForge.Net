using System;
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
using System.Text;
namespace MapDB.Reader
{

	/**
	 * Reads from a {@link Stream} into a buffer and decodes the data.
	 */
	public class BufferStream : Stream
	{
		private const string CHARSET_UTF8 = "UTF-8";

		public static readonly DateTime BaseDateTime = new DateTime(1970,1,1);

		/**
		 * Maximum buffer size which is supported by this implementation.
		 */
		public const int MAXIMUM_BUFFER_SIZE = 2500000;

		private byte[] bufferData;
		private Stream inputFile;

		public BufferStream ( Stream inputFile )
		{
			this.inputFile = inputFile;
		}

		/**
		 * Reads the given amount of bytes from the file into the read buffer and resets the internal buffer position. If
		 * the capacity of the read buffer is too small, a larger one is created automatically.
		 * 
		 * @param length
		 *            the amount of bytes to read from the file.
		 * @return true if the whole data was read successfully, false otherwise.
		 * @throws IOException
		 *             if an error occurs while reading the file.
		 */
		public bool ReadFromFile ( int length )
		{
			// ensure that the read buffer is large enough
			if (this.bufferData == null || this.bufferData.Length < length) {
				// ensure that the read buffer is not too large
				if (length > MAXIMUM_BUFFER_SIZE) {
					return false;
				}
				this.bufferData = new byte[length];
			}

			// reset the buffer position and read the data into the buffer
			this.Position = 0;
			return this.inputFile.Read(this.bufferData, 0, length) == length;
		}

		public override int ReadByte ()
		{
			return bufferData[Position++];
		}
		/**
		 * Converts four bytes from the read buffer to a signed int.
		 * <p>
		 * The byte order is big-endian.
		 * 
		 * @return the int value.
		 */
		public int ReadInt ()
		{
			this.Position += 4;
			return Deserializer.GetInt(this.bufferData, this.Position - 4);
		}

		/**
		 * Converts eight bytes from the read buffer to a signed long.
		 * <p>
		 * The byte order is big-endian.
		 * 
		 * @return the long value.
		 */
		public long ReadLong ()
		{
			this.Position += 8;
			return Deserializer.GetLong(this.bufferData, this.Position - 8);
		}

		public DateTime ReadDateTime ()
		{
			return BaseDateTime.AddMilliseconds(ReadLong());
		}

		/**
		 * Converts two bytes from the read buffer to a signed int.
		 * <p>
		 * The byte order is big-endian.
		 * 
		 * @return the int value.
		 */
		public short Readshort ()
		{
			this.Position += 2;
			return Deserializer.GetShort(this.bufferData, this.Position - 2);
		}

		/**
		 * Converts a variable amount of bytes from the read buffer to a signed int.
		 * <p>
		 * The first bit is for continuation info, the other six (last byte) or seven (all other bytes) bits are for data.
		 * The second bit in the last byte indicates the sign of the number.
		 * 
		 * @return the int value.
		 */
		public int ReadSignedInt ()
		{
			int variablebyteDecode = 0;
			byte variablebyteShift = 0;

			// check if the continuation bit is set
			while ((this.bufferData[this.Position] & 0x80) != 0) {
				variablebyteDecode |= (this.bufferData[this.Position++] & 0x7f) << variablebyteShift;
				variablebyteShift += 7;
			}

			// read the six data bits from the last byte
			if ((this.bufferData[this.Position] & 0x40) != 0) {
				// negative
				return -(variablebyteDecode | ((this.bufferData[this.Position++] & 0x3f) << variablebyteShift));
			}
			// positive
			return variablebyteDecode | ((this.bufferData[this.Position++] & 0x3f) << variablebyteShift);
		}

		/**
		 * Converts a variable amount of bytes from the read buffer to an unsigned int.
		 * <p>
		 * The first bit is for continuation info, the other seven bits are for data.
		 * 
		 * @return the int value.
		 */
		public int ReadUnsignedInt ()
		{
			int variablebyteDecode = 0;
			byte variablebyteShift = 0;

			// check if the continuation bit is set
			while ((this.bufferData[this.Position] & 0x80) != 0) {
				variablebyteDecode |= (this.bufferData[this.Position++] & 0x7f) << variablebyteShift;
				variablebyteShift += 7;
			}

			// read the seven data bits from the last byte
			return variablebyteDecode | (this.bufferData[this.Position++] << variablebyteShift);
		}

		/**
		 * Decodes a variable amount of bytes from the read buffer to a string.
		 * 
		 * @return the UTF-8 decoded string (may be null).
		 */
		public string ReadUTF8Encodedstring ()
		{
			return ReadUTF8Encodedstring(ReadUnsignedInt());
		}

		/**
		 * Decodes the given amount of bytes from the read buffer to a string.
		 * 
		 * @param stringLength
		 *            the length of the string in bytes.
		 * @return the UTF-8 decoded string (may be null).
		 */
		public string ReadUTF8Encodedstring ( int stringLength )
		{
			if (stringLength > 0 && this.Position + stringLength <= this.bufferData.Length) {
				this.Position += stringLength;
				return Encoding.UTF8.GetString(this.bufferData, (int)this.Position - stringLength, stringLength);
			}
			return null;
		}
		
		/**
		 * Skips the given number of bytes in the read buffer.
		 * 
		 * @param bytes
		 *            the number of bytes to skip.
		 */
		public void SkipBytes ( int bytes )
		{
			this.Position += bytes;
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return false; }
		}

		public override void Flush ()
		{
			throw new System.NotSupportedException();
		}

		public override long Length
		{
			get { return bufferData.Length; }
		}

		private long position;

		public override long Position
		{
			get { return position; }
			set {
				if (value < 0 || value > Length) throw new IndexOutOfRangeException("Le curseur est en dehors des limites");
				position = value; 
			}
		}

		public override int Read ( byte[] buffer, int offset, int count )
		{
			count = (int)Math.Min(count, Math.Min(Length - Position - count, buffer.Length - offset - count));
			Array.Copy(bufferData, Position, buffer, offset, count);
			Position += count;
			return count;
		}

		public override long Seek ( long offset, SeekOrigin origin )
		{
			switch (origin) {
				case SeekOrigin.Begin:
					Position = offset;
					break;
				case SeekOrigin.Current:
					Position = position + offset; 
					break;
				case SeekOrigin.End:
					Position = Length + offset;
					break;
				default:
					throw new ArgumentException("La valeur du paramètre n'est pas valide", "origin");
			};
			return Position;
			
		}

		public override void SetLength ( long value )
		{
			throw new System.NotSupportedException();
		}

		public override void Write ( byte[] buffer, int offset, int count )
		{
			throw new System.NotSupportedException();
		}
	}
}
using System;
using System.IO;

namespace Managed.Adb.Utilities.IO
{
    /// <summary>
    /// Collection of utility methods which operate on streams.
    /// (With C# 3.0, these could well become extension methods on Stream.)
    /// </summary>
    public static class StreamUtil
    {
        const int DefaultBufferSize = 8*1024;

        /// <summary>
        /// Reads the given stream up to the end, returning the data as a byte
        /// array.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="IOException">An error occurs while reading from the stream</exception>
        /// <returns>The data read from the stream</returns>
        public static byte[] ReadFully(Stream input)
        {
            return ReadFully(input, DefaultBufferSize);
        }

        /// <summary>
        /// Reads the given stream up to the end, returning the data as a byte
        /// array, using the given buffer size.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="bufferSize">The size of buffer to use when reading</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">bufferSize is less than 1</exception>
        /// <exception cref="IOException">An error occurs while reading from the stream</exception>
        /// <returns>The data read from the stream</returns>
        public static byte[] ReadFully(Stream input, int bufferSize)
        {
            if (bufferSize < 1)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }
            return ReadFully(input, new byte[bufferSize]);
        }

        /// <summary>
        /// Reads the given stream up to the end, returning the data as a byte
        /// array, using the given buffer for transferring data. Note that the
        /// current contents of the buffer is ignored, so the buffer needn't
        /// be cleared beforehand.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="buffer">The buffer to use to transfer data</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="IOException">An error occurs while reading from the stream</exception>
        /// <returns>The data read from the stream</returns>
        public static byte[] ReadFully(Stream input, IBuffer buffer)
        {
            if (buffer==null)
            {
                throw new ArgumentNullException("buffer");
            }
            return ReadFully(input, buffer.Bytes);
        }

        /// <summary>
        /// Reads the given stream up to the end, returning the data as a byte
        /// array, using the given buffer for transferring data. Note that the
        /// current contents of the buffer is ignored, so the buffer needn't
        /// be cleared beforehand.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="buffer">The buffer to use to transfer data</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ArgumentException">buffer is a zero-length array</exception>
        /// <exception cref="IOException">An error occurs while reading from the stream</exception>
        /// <returns>The data read from the stream</returns>
        public static byte[] ReadFully(Stream input, byte[] buffer)
        {
            if (buffer==null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (input==null)
            {
                throw new ArgumentNullException("input");
            }
            if (buffer.Length==0)
            {
                throw new ArgumentException("Buffer has length of 0");
            }
            // We could do all our own work here, but using MemoryStream is easier
            // and likely to be just as efficient.
            using (MemoryStream tempStream = new MemoryStream())
            {
                Copy(input, tempStream, buffer);
                // No need to copy the buffer if it's the right size
                if (tempStream.Length==tempStream.GetBuffer().Length)
                {
                    return tempStream.GetBuffer();
                }
                // Okay, make a copy that's the right size
                return tempStream.ToArray();
            }
        }

        /// <summary>
        /// Copies all the data from one stream into another.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="output">The stream to write to</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentNullException">output is null</exception>
        /// <exception cref="IOException">An error occurs while reading or writing</exception>
        public static void Copy(Stream input, Stream output)
        {
            Copy(input, output, DefaultBufferSize);
        }

        /// <summary>
        /// Copies all the data from one stream into another, using a buffer
        /// of the given size.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="output">The stream to write to</param>
        /// <param name="bufferSize">The size of buffer to use when reading</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentNullException">output is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">bufferSize is less than 1</exception>
        /// <exception cref="IOException">An error occurs while reading or writing</exception>
        public static void Copy(Stream input, Stream output, int bufferSize)
        {
            if (bufferSize < 1)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }
            Copy(input, output, new byte[bufferSize]);
        }

        /// <summary>
        /// Copies all the data from one stream into another, using the given 
        /// buffer for transferring data. Note that the current contents of 
        /// the buffer is ignored, so the buffer needn't be cleared beforehand.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="output">The stream to write to</param>
        /// <param name="buffer">The buffer to use to transfer data</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentNullException">output is null</exception>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="IOException">An error occurs while reading or writing</exception>
        public static void Copy(Stream input, Stream output, IBuffer buffer)
        {
            if (buffer==null)
            {
                throw new ArgumentNullException("buffer");
            }
            Copy(input, output, buffer.Bytes);
        }

        /// <summary>
        /// Copies all the data from one stream into another, using the given 
        /// buffer for transferring data. Note that the current contents of 
        /// the buffer is ignored, so the buffer needn't be cleared beforehand.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="output">The stream to write to</param>
        /// <param name="buffer">The buffer to use to transfer data</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentNullException">output is null</exception>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ArgumentException">buffer is a zero-length array</exception>
        /// <exception cref="IOException">An error occurs while reading or writing</exception>
        public static void Copy(Stream input, Stream output, byte[] buffer)
        {
            if (buffer==null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (input==null)
            {
                throw new ArgumentNullException("input");
            }
            if (output==null)
            {
                throw new ArgumentNullException("output");
            }
            if (buffer.Length==0)
            {
                throw new ArgumentException("Buffer has length of 0");
            }
            int read;
            while ( (read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        /// <summary>
        /// Reads exactly the given number of bytes from the specified stream.
        /// If the end of the stream is reached before the specified amount
        /// of data is read, an exception is thrown.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="bytesToRead">The number of bytes to read</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">bytesToRead is less than 1</exception>
        /// <exception cref="EndOfStreamException">The end of the stream is reached before 
        /// enough data has been read</exception>
        /// <exception cref="IOException">An error occurs while reading from the stream</exception>
        /// <returns>The data read from the stream</returns>
        public static byte[] ReadExactly(Stream input, int bytesToRead)
        {
            return ReadExactly(input, new byte[bytesToRead]);
        }

        /// <summary>
        /// Reads into a buffer, filling it completely.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="buffer">The buffer to read into</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">The buffer is of zero length</exception>
        /// <exception cref="EndOfStreamException">The end of the stream is reached before 
        /// enough data has been read</exception>
        /// <exception cref="IOException">An error occurs while reading from the stream</exception>
        /// <returns>The data read from the stream</returns>
        public static byte[] ReadExactly(Stream input, IBuffer buffer)
        {
            return ReadExactly(input, buffer.Bytes);
        }

        /// <summary>
        /// Reads into a buffer, filling it completely.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="buffer">The buffer to read into</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">The buffer is of zero length</exception>
        /// <exception cref="EndOfStreamException">The end of the stream is reached before 
        /// enough data has been read</exception>
        /// <exception cref="IOException">An error occurs while reading from the stream</exception>
        /// <returns>The data read from the stream</returns>
        public static byte[] ReadExactly(Stream input, byte[] buffer)
        {
            return ReadExactly(input, buffer, buffer.Length);
        }

        /// <summary>
        /// Reads into a buffer, for the given number of bytes.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="buffer">The buffer to read into</param>
        /// <param name="bytesToRead">The number of bytes to read</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">The buffer is of zero length, or bytesToRead
        /// exceeds the buffer length</exception>
        /// <exception cref="EndOfStreamException">The end of the stream is reached before 
        /// enough data has been read</exception>
        /// <exception cref="IOException">An error occurs while reading from the stream</exception>
        /// <returns>The data read from the stream</returns>
        public static byte[] ReadExactly(Stream input, IBuffer buffer, int bytesToRead)
        {
            return ReadExactly(input, buffer.Bytes, bytesToRead);
        }

        /// <summary>
        /// Reads exactly the given number of bytes from the specified stream,
        /// into the given buffer, starting at position 0 of the array.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="buffer">The byte array to read into</param>
        /// <param name="bytesToRead">The number of bytes to read</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">bytesToRead is less than 1</exception>
        /// <exception cref="EndOfStreamException">The end of the stream is reached before 
        /// enough data has been read</exception>
        /// <exception cref="IOException">An error occurs while reading from the stream</exception>
        public static byte[] ReadExactly(Stream input, byte[] buffer, int bytesToRead)
        {
            return ReadExactly(input, buffer, 0, bytesToRead);
        }

        /// <summary>
        /// Reads into a buffer, for the given number of bytes, from the specified location
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="buffer">The buffer to read into</param>
        /// <param name="startIndex">The index into the buffer at which to start writing</param>
        /// <param name="bytesToRead">The number of bytes to read</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">The buffer is of zero length, or startIndex+bytesToRead
        /// exceeds the buffer length</exception>
        /// <exception cref="EndOfStreamException">The end of the stream is reached before 
        /// enough data has been read</exception>
        /// <exception cref="IOException">An error occurs while reading from the stream</exception>
        /// <returns>The data read from the stream</returns>
        public static byte[] ReadExactly(Stream input, IBuffer buffer, int startIndex, int bytesToRead)
        {
            return ReadExactly(input, buffer.Bytes, 0, bytesToRead);
        }

        /// <summary>
        /// Reads exactly the given number of bytes from the specified stream,
        /// into the given buffer, starting at position 0 of the array.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="buffer">The byte array to read into</param>
        /// <param name="startIndex">The index into the buffer at which to start writing</param>
        /// <param name="bytesToRead">The number of bytes to read</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">bytesToRead is less than 1, startIndex is less than 0,
        /// or startIndex+bytesToRead is greater than the buffer length</exception>
        /// <exception cref="EndOfStreamException">The end of the stream is reached before 
        /// enough data has been read</exception>
        /// <exception cref="IOException">An error occurs while reading from the stream</exception>
        public static byte[] ReadExactly(Stream input, byte[] buffer, int startIndex, int bytesToRead)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (startIndex < 0 || startIndex >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            if (bytesToRead < 1 || startIndex + bytesToRead > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("bytesToRead");
            }

            int index = 0;

            while (index < bytesToRead)
            {
                int read = input.Read(buffer, startIndex + index, bytesToRead - index);
                if (read == 0)
                {
                    throw new EndOfStreamException
                        (String.Format("End of stream reached with {0} byte{1} left to read.",
                                       bytesToRead - index,
                                       bytesToRead - index == 1 ? "s" : ""));
                }
                index += read;
            }
            return buffer;
        }
    }
}

using System.IO;
using System.Text;
using SabreTools.IO;
using SabreTools.Models.Quantum;
using SabreTools.Serialization.Interfaces;
using static SabreTools.Models.Quantum.Constants;

namespace SabreTools.Serialization.Deserializers
{
    public class Quantum :
        IByteDeserializer<Archive>,
        IFileDeserializer<Archive>,
        IStreamDeserializer<Archive>
    {
        #region IByteDeserializer

        /// <inheritdoc cref="IByteDeserializer.Deserialize(byte[]?, int)"/>
        public static Archive? DeserializeBytes(byte[]? data, int offset)
        {
            var deserializer = new Quantum();
            return deserializer.Deserialize(data, offset);
        }

        /// <inheritdoc/>
        public Archive? Deserialize(byte[]? data, int offset)
        {
            // If the data is invalid
            if (data == null)
                return null;

            // If the offset is out of bounds
            if (offset < 0 || offset >= data.Length)
                return null;

            // Create a memory stream and parse that
            var dataStream = new MemoryStream(data, offset, data.Length - offset);
            return DeserializeStream(dataStream);
        }

        #endregion

        #region IFileDeserializer

        /// <inheritdoc cref="IFileDeserializer.Deserialize(string?)"/>
        public static Archive? DeserializeFile(string? path)
        {
            var deserializer = new Quantum();
            return deserializer.Deserialize(path);
        }

        /// <inheritdoc/>
        public Archive? Deserialize(string? path)
        {
            using var stream = PathProcessor.OpenStream(path);
            return DeserializeStream(stream);
        }

        #endregion

        #region IStreamDeserializer

        /// <inheritdoc cref="IStreamDeserializer.Deserialize(Stream?)"/>
        public static Archive? DeserializeStream(Stream? data)
        {
            var deserializer = new Quantum();
            return deserializer.Deserialize(data);
        }

        /// <inheritdoc/>
        public Archive? Deserialize(Stream? data)
        {
            // If the data is invalid
            if (data == null || data.Length == 0 || !data.CanSeek || !data.CanRead)
                return null;

            // If the offset is out of bounds
            if (data.Position < 0 || data.Position >= data.Length)
                return null;

            // Cache the current offset
            int initialOffset = (int)data.Position;

            // Create a new archive to fill
            var archive = new Archive();

            #region Header

            // Try to parse the header
            var header = ParseHeader(data);
            if (header == null)
                return null;

            // Set the archive header
            archive.Header = header;

            #endregion

            #region File List

            // If we have any files
            if (header.FileCount > 0)
            {
                var fileDescriptors = new FileDescriptor[header.FileCount];

                // Read all entries in turn
                for (int i = 0; i < header.FileCount; i++)
                {
                    var file = ParseFileDescriptor(data, header.MinorVersion);
                    if (file == null)
                        return null;

                    fileDescriptors[i] = file;
                }

                // Set the file list
                archive.FileList = fileDescriptors;
            }

            #endregion

            // Cache the compressed data offset
            archive.CompressedDataOffset = data.Position;

            return archive;
        }

        /// <summary>
        /// Parse a Stream into a header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled header on success, null on error</returns>
        private static Header? ParseHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            Header header = new Header();

            byte[]? signature = data.ReadBytes(2);
            if (signature == null)
                return null;

            header.Signature = Encoding.ASCII.GetString(signature);
            if (header.Signature != SignatureString)
                return null;

            header.MajorVersion = data.ReadByteValue();
            header.MinorVersion = data.ReadByteValue();
            header.FileCount = data.ReadUInt16();
            header.TableSize = data.ReadByteValue();
            header.CompressionFlags = data.ReadByteValue();

            return header;
        }

        /// <summary>
        /// Parse a Stream into a file descriptor
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <param name="minorVersion">Minor version of the archive</param>
        /// <returns>Filled file descriptor on success, null on error</returns>
        private static FileDescriptor ParseFileDescriptor(Stream data, byte minorVersion)
        {
            // TODO: Use marshalling here instead of building
            FileDescriptor fileDescriptor = new FileDescriptor();

            fileDescriptor.FileNameSize = ReadVariableLength(data);
            if (fileDescriptor.FileNameSize > 0)
            {
                byte[]? fileName = data.ReadBytes(fileDescriptor.FileNameSize);
                if (fileName != null)
                    fileDescriptor.FileName = Encoding.ASCII.GetString(fileName);
            }

            fileDescriptor.CommentFieldSize = ReadVariableLength(data);
            if (fileDescriptor.CommentFieldSize > 0)
            {
                byte[]? commentField = data.ReadBytes(fileDescriptor.CommentFieldSize);
                if (commentField != null)
                    fileDescriptor.CommentField = Encoding.ASCII.GetString(commentField);
            }

            fileDescriptor.ExpandedFileSize = data.ReadUInt32();
            fileDescriptor.FileTime = data.ReadUInt16();
            fileDescriptor.FileDate = data.ReadUInt16();

            // Hack for unknown format data
            if (minorVersion == 22)
                fileDescriptor.Unknown = data.ReadUInt16();

            return fileDescriptor;
        }

        /// <summary>
        /// Parse a Stream into a variable-length size prefix
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Variable-length size prefix</returns>
        /// <remarks>
        /// Strings are prefixed with their length. If the length is less than 128
        /// then it is stored directly in one byte. If it is greater than 127 then
        /// the high bit of the first byte is set to 1 and the remaining fifteen bits
        /// contain the actual length in big-endian format.
        /// </remarks>
        private static int ReadVariableLength(Stream data)
        {
            byte b0 = data.ReadByteValue();
            if (b0 < 0x7F)
                return b0;

            b0 &= 0x7F;
            byte b1 = data.ReadByteValue();
            return (b0 << 8) | b1;
        }

        #endregion
    }
}
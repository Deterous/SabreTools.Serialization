using System.IO;
using System.Text;
using SabreTools.IO;
using SabreTools.Models.BFPK;
using SabreTools.Serialization.Interfaces;
using static SabreTools.Models.BFPK.Constants;

namespace SabreTools.Serialization.Deserializers
{
    public class BFPK :
        IByteDeserializer<Archive>,
        IFileDeserializer<Archive>,
        IStreamDeserializer<Archive>
    {
        #region IByteDeserializer

        /// <inheritdoc cref="IByteDeserializer.Deserialize(byte[]?, int)"/>
        public static Archive? DeserializeBytes(byte[]? data, int offset)
        {
            var deserializer = new BFPK();
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
            var deserializer = new BFPK();
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
            var deserializer = new BFPK();
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

            #region Files

            // If we have any files
            if (header.Files > 0)
            {
                var files = new FileEntry[header.Files];

                // Read all entries in turn
                for (int i = 0; i < header.Files; i++)
                {
                    var file = ParseFileEntry(data);
                    if (file == null)
                        return null;

                    files[i] = file;
                }

                // Set the files
                archive.Files = files;
            }

            #endregion

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

            byte[]? magic = data.ReadBytes(4);
            if (magic == null)
                return null;

            header.Magic = Encoding.ASCII.GetString(magic);
            if (header.Magic != SignatureString)
                return null;

            header.Version = data.ReadInt32();
            header.Files = data.ReadInt32();

            return header;
        }

        /// <summary>
        /// Parse a Stream into a file entry
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled file entry on success, null on error</returns>
        private static FileEntry ParseFileEntry(Stream data)
        {
            // TODO: Use marshalling here instead of building
            FileEntry fileEntry = new FileEntry();

            fileEntry.NameSize = data.ReadInt32();
            if (fileEntry.NameSize > 0)
            {
                byte[]? name = data.ReadBytes(fileEntry.NameSize);
                if (name != null)
                    fileEntry.Name = Encoding.ASCII.GetString(name);
            }

            fileEntry.UncompressedSize = data.ReadInt32();
            fileEntry.Offset = data.ReadInt32();
            if (fileEntry.Offset > 0)
            {
                long currentOffset = data.Position;
                data.Seek(fileEntry.Offset, SeekOrigin.Begin);
                fileEntry.CompressedSize = data.ReadInt32();
                data.Seek(currentOffset, SeekOrigin.Begin);
            }

            return fileEntry;
        }

        #endregion
    }
}
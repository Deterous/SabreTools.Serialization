using System.IO;
using System.Text;
using SabreTools.IO;
using SabreTools.Models.PAK;
using SabreTools.Serialization.Interfaces;
using static SabreTools.Models.PAK.Constants;

namespace SabreTools.Serialization.Deserializers
{
    public class PAK :
        BaseBinaryDeserializer<Models.PAK.File>,
        IStreamDeserializer<Models.PAK.File>
    {
        #region IStreamDeserializer

        /// <inheritdoc/>
        public Models.PAK.File? Deserialize(Stream? data)
        {
            // If the data is invalid
            if (data == null || data.Length == 0 || !data.CanSeek || !data.CanRead)
                return null;

            // If the offset is out of bounds
            if (data.Position < 0 || data.Position >= data.Length)
                return null;

            // Cache the current offset
            long initialOffset = data.Position;

            // Create a new Half-Life Package to fill
            var file = new Models.PAK.File();

            #region Header

            // Try to parse the header
            var header = ParseHeader(data);
            if (header == null)
                return null;

            // Set the package header
            file.Header = header;

            #endregion

            #region Directory Items

            // Get the directory items offset
            uint directoryItemsOffset = header.DirectoryOffset;
            if (directoryItemsOffset < 0 || directoryItemsOffset >= data.Length)
                return null;

            // Seek to the directory items
            data.Seek(directoryItemsOffset, SeekOrigin.Begin);

            // Create the directory item array
            file.DirectoryItems = new DirectoryItem[header.DirectoryLength / 64];

            // Try to parse the directory items
            for (int i = 0; i < file.DirectoryItems.Length; i++)
            {
                var directoryItem = ParseDirectoryItem(data);
                file.DirectoryItems[i] = directoryItem;
            }

            #endregion

            return file;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life Package header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life Package header on success, null on error</returns>
        private static Header? ParseHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            Header header = new Header();

            byte[]? signature = data.ReadBytes(4);
            if (signature == null)
                return null;

            header.Signature = Encoding.ASCII.GetString(signature);
            if (header.Signature != SignatureString)
                return null;

            header.DirectoryOffset = data.ReadUInt32();
            header.DirectoryLength = data.ReadUInt32();

            return header;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life Package directory item
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life Package directory item on success, null on error</returns>
        private static DirectoryItem ParseDirectoryItem(Stream data)
        {
            // TODO: Use marshalling here instead of building
            DirectoryItem directoryItem = new DirectoryItem();

            byte[]? itemName = data.ReadBytes(56);
            if (itemName != null)
                directoryItem.ItemName = Encoding.ASCII.GetString(itemName).TrimEnd('\0');
            directoryItem.ItemOffset = data.ReadUInt32();
            directoryItem.ItemLength = data.ReadUInt32();

            return directoryItem;
        }

        #endregion
    }
}
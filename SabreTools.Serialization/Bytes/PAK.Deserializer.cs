using System.IO;
using SabreTools.Serialization.Interfaces;

namespace SabreTools.Serialization.Bytes
{
    public partial class PAK : IByteSerializer<Models.PAK.File>
    {
        /// <inheritdoc cref="IByteSerializer.Deserialize(byte[]?, int)"/>
        public static Models.PAK.File? DeserializeBytes(byte[]? data, int offset)
        {
            var deserializer = new PAK();
            return deserializer.Deserialize(data, offset);
        }

        /// <inheritdoc/>
        public Models.PAK.File? Deserialize(byte[]? data, int offset)
        {
            // If the data is invalid
            if (data == null)
                return null;

            // If the offset is out of bounds
            if (offset < 0 || offset >= data.Length)
                return null;

            // Create a memory stream and parse that
            var dataStream = new MemoryStream(data, offset, data.Length - offset);
            return Streams.PAK.DeserializeStream(dataStream);
        }
    }
}
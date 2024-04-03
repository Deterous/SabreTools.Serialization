using System.IO;
using SabreTools.Serialization.Interfaces;

namespace SabreTools.Serialization.Bytes
{
    public partial class LinearExecutable : IByteSerializer<Models.LinearExecutable.Executable>
    {
        /// <inheritdoc cref="IByteSerializer.Deserialize(byte[]?, int)"/>
        public static Models.LinearExecutable.Executable? DeserializeBytes(byte[]? data, int offset)
        {
            var deserializer = new LinearExecutable();
            return deserializer.Deserialize(data, offset);
        }

        /// <inheritdoc/>
        public Models.LinearExecutable.Executable? Deserialize(byte[]? data, int offset)
        {
            // If the data is invalid
            if (data == null)
                return null;

            // If the offset is out of bounds
            if (offset < 0 || offset >= data.Length)
                return null;

            // Create a memory stream and parse that
            var dataStream = new MemoryStream(data, offset, data.Length - offset);
            return Streams.LinearExecutable.DeserializeStream(dataStream);
        }
    }
}
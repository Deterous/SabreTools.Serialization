namespace SabreTools.Serialization.Deserializers
{
    public class Listxml :
        XmlFile<Models.Listxml.Mame>
    {
        #region IByteDeserializer

        /// <inheritdoc cref="Interfaces.IByteDeserializer.Deserialize(byte[]?, int)"/>
        public static Models.Listxml.Mame? DeserializeBytes(byte[]? data, int offset)
        {
            var deserializer = new Listxml();
            return deserializer.Deserialize(data, offset);
        }

        #endregion

        #region IFileDeserializer

        /// <inheritdoc cref="Interfaces.IFileDeserializer.Deserialize(string?)"/>
        public static Models.Listxml.Mame? DeserializeFile(string? path)
        {
            var deserializer = new Listxml();
            return deserializer.Deserialize(path);
        }

        #endregion

        #region IStreamDeserializer

        /// <inheritdoc cref="Interfaces.IStreamDeserializer.Deserialize(Stream?)"/>
        public static Models.Listxml.Mame? DeserializeStream(System.IO.Stream? data)
        {
            var deserializer = new Listxml();
            return deserializer.Deserialize(data);
        }

        #endregion
    }
}
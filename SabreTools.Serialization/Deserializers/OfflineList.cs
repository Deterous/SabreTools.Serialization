namespace SabreTools.Serialization.Deserializers
{
    public class OfflineList :
        XmlFile<Models.OfflineList.Dat>
    {
        #region IByteDeserializer

        /// <inheritdoc cref="Interfaces.IByteDeserializer.Deserialize(byte[]?, int)"/>
        public static Models.OfflineList.Dat? DeserializeBytes(byte[]? data, int offset)
        {
            var deserializer = new OfflineList();
            return deserializer.Deserialize(data, offset);
        }

        #endregion

        #region IFileDeserializer

        /// <inheritdoc cref="Interfaces.IFileDeserializer.Deserialize(string?)"/>
        public static Models.OfflineList.Dat? DeserializeFile(string? path)
        {
            var deserializer = new OfflineList();
            return deserializer.Deserialize(path);
        }

        #endregion

        #region IStreamDeserializer

        /// <inheritdoc cref="Interfaces.IStreamDeserializer.Deserialize(Stream?)"/>
        public static Models.OfflineList.Dat? DeserializeStream(System.IO.Stream? data)
        {
            var deserializer = new OfflineList();
            return deserializer.Deserialize(data);
        }

        #endregion
    }
}
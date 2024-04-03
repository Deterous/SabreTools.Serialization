using SabreTools.Serialization.Interfaces;

namespace SabreTools.Serialization.Files
{
    public partial class LinearExecutable : IFileSerializer<Models.LinearExecutable.Executable>
    {
        /// <inheritdoc cref="IFileSerializer.Serialize(T?, string?)"/>
        public static bool SerializeFile(Models.LinearExecutable.Executable? obj, string? path)
        {
            var serializer = new LinearExecutable();
            return serializer.Serialize(obj, path);
        }
        
        /// <inheritdoc/>
        public bool Serialize(Models.LinearExecutable.Executable? obj, string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            using var stream = Streams.LinearExecutable.SerializeStream(obj);
            if (stream == null)
                return false;

            using var fs = System.IO.File.OpenWrite(path);
            stream.CopyTo(fs);
            return true;
        }
    }
}
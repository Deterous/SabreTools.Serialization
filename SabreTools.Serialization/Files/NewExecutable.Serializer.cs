using SabreTools.Serialization.Interfaces;

namespace SabreTools.Serialization.Files
{
    public partial class NewExecutable : IFileSerializer<Models.NewExecutable.Executable>
    {
        /// <inheritdoc cref="IFileSerializer.Serialize(T?, string?)"/>
        public static bool SerializeFile(Models.NewExecutable.Executable? obj, string? path)
        {
            var serializer = new NewExecutable();
            return serializer.Serialize(obj, path);
        }
        
        /// <inheritdoc/>
        public bool Serialize(Models.NewExecutable.Executable? obj, string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            using var stream = Streams.NewExecutable.SerializeStream(obj);
            if (stream == null)
                return false;

            using var fs = System.IO.File.OpenWrite(path);
            stream.CopyTo(fs);
            return true;
        }
    }
}
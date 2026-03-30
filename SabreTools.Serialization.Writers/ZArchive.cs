using System.IO;
using SabreTools.Data.Models.ZArchive;
using SabreTools.IO.Extensions;

namespace SabreTools.Serialization.Writers
{
    public class ZArchive : BaseBinaryWriter<Archive>
    {
        /// <inheritdoc/>
        public override Stream? SerializeStream(Archive? obj)
        {
            // If the metadata file is null
            if (obj is null)
                return null;

            // Setup the writer and output
            var stream = new MemoryStream();
            var writer = new Writer(stream, Encoding.UTF8);

            // Write out the file contents
            // WriteLocalFiles(obj.LocalFiles, writer);

            // Return the stream
            stream.SeekIfPossible(0, SeekOrigin.Begin);
            return stream;
        }
    }
}

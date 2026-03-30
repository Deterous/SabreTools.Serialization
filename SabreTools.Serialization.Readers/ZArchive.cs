using System.Collections.Generic;
using System.IO;
using SabreTools.Data.Models.ZArchive;
using SabreTools.IO.Extensions;
using SabreTools.Numerics.Extensions;

#pragma warning disable IDE0017 // Simplify object initialization
namespace SabreTools.Serialization.Readers
{
    public class ZArchive : BaseBinaryReader<Archive>
    {
        /// <inheritdoc/>
        public override Archive? Deserialize(Stream? data)
        {
            // If the data is invalid
            if (data is null || !data.CanRead)
                return null;

            try
            {
                var archive = new Archive();

                return archive;
            }
            catch
            {
                // Ignore the actual error
                return null;
            }
        }
    }
}

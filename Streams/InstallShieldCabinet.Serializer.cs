using System;
using System.IO;
using SabreTools.Models.InstallShieldCabinet;

namespace SabreTools.Serialization.Streams
{
    public partial class InstallShieldCabinet : IStreamSerializer<Cabinet>
    {
        /// <inheritdoc/>
#if NET48
        public Stream Serialize(Cabinet obj) => throw new NotImplementedException();
#else
        public Stream? Serialize(Cabinet? obj) => throw new NotImplementedException();
#endif
    }
}
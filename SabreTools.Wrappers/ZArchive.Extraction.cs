using System;

namespace SabreTools.Wrappers
{
    public partial class ZArchive : IExtractable
    {
        /// <inheritdoc/>
        public bool Extract(string outputDirectory, bool includeDebug)
        {
            if (_dataSource is null || !_dataSource.CanRead)
                return false;
            
            // Not yet implemented
            return false;
        }
    }
}

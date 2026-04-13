using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SabreTools.Data.Models.XboxISO;

namespace SabreTools.Wrappers
{
    public partial class XboxISO : IExtractable
    {
        /// <inheritdoc/>
        public virtual bool Extract(string outputDirectory, bool includeDebug)
        {
            var videoWrapper = SabreTools.Wrappers.ISO9660.Create(VideoPartition, _dataSource, initialOffset, _dataSource.Length);
            bool success = videoWrapper.Extract(outputDirectory, includeDebug);

            var gameWrapper = SabreTools.Wrappers.XDVDFS.Create(GamePartition, _dataSource, initialOffset + Constants.XisoOffsets[XGDType], Constants.XisoLengths[XGDType]);
            success |= gameWrapper.Extract(outputDirectory, includeDebug);

            return success;
        }
    }
}

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
            var ISO9660 = SabreTools.Wrappers.ISO9660.Create(VideoPartition, _dataSource, initialOffset, _dataSource.Length);
            bool success |= ISO9660.Extract(outputDirectory, includeDebug);

            var XDVDFS = SabreTools.Wrappers.XDVDFS.Create(GamePartition, _dataSource, initialOffset + Constants.XisoOffsets[XGDType], Constants.XisoLengths[XGDType]);
            success |= XDVDFS.Extract(outputDirectory, includeDebug);

            return success;
        }
    }
}

using System;
using System.Text;
using SabreTools.Data.Models.XboxISO;
using SabreTools.Text.Extensions;

namespace SabreTools.Wrappers
{
    public partial class XboxISO : IPrintable
    {
#if NETCOREAPP
        /// <inheritdoc/>
        public string ExportJSON() => System.Text.Json.JsonSerializer.Serialize(Model, _jsonSerializerOptions);
#endif

        /// <inheritdoc/>
        public void PrintInformation(StringBuilder builder)
        {
            builder.AppendLine("Xbox / Xbox 360 Disc Image Information:");
            builder.AppendLine("-------------------------");
            builder.AppendLine();

            long initialOffset = _dataSource.Position;

            var ISO9660 = new SabreTools.Wrappers.ISO9660(VideoPartition, _dataSource, initialOffset, _dataSource.Length);
            ISO9660.PrintInformation(builder);
            builder.AppendLine();

            var XDVDFS = new SabreTools.Wrappers.XDVDFS(GamePartition, _dataSource, initialOffset + Constants.XisoOffsets[XGDType], Constants.XisoLengths[XGDType]);
            XDVDFS.PrintInformation(builder);
        }
    }
}

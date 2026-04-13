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

            var ISO9660 = SabreTools.Wrappers.ISO9660.Create(VideoPartition, _dataSource, initialOffset, _dataSource.Length);
            ISO9660.Print(builder);
            builder.AppendLine();

            var XDVDFS = SabreTools.Wrappers.XDVDFS.Create(GamePartition, _dataSource, initialOffset + Constants.XisoOffsets[XGDType], Constants.XisoLengths[XGDType]);
            XDVDFS.Print(builder);
        }
    }
}

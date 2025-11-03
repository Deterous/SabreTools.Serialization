using System;
using System.Text;

namespace SabreTools.Serialization.Wrappers
{
    public partial class CDROM : IPrintable
    {
#if NETCOREAPP
        /// <inheritdoc/>
        public string ExportJSON() => System.Text.Json.JsonSerializer.Serialize(Model, _jsonSerializerOptions);
#endif

        /// <inheritdoc/>
        public void PrintInformation(StringBuilder builder)
        {
            builder.AppendLine("CD-ROM Data Track Information:");
            builder.AppendLine("-------------------------");
            builder.AppendLine();

            if (FileSystem != null)
                FileSystem.PrintInformation(builder);
        }
    }
}


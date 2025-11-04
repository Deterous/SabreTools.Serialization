using System;
using System.Text;

namespace SabreTools.Serialization.Wrappers
{
    public partial class CDROM : IPrintable
    {
#if NETCOREAPP
        /// <inheritdoc/>
        public new string ExportJSON() => System.Text.Json.JsonSerializer.Serialize(Model, _jsonSerializerOptions);
#endif

        /// <inheritdoc/>
        public override void PrintInformation(StringBuilder builder)
        {
            builder.AppendLine("CD-ROM Data Track Information:");
            builder.AppendLine("-------------------------");
            builder.AppendLine();

            builder.AppendLine("  ISO 9660 Information:");
            builder.AppendLine("  -------------------------");
            builder.AppendLine();

            Print(builder, Model.Volume.SystemArea);

            Print(builder, Model.Volume.VolumeDescriptorSet);

            // TODO: Parse the volume descriptors to print the Path Table Groups and Directory Descriptors with proper encoding
            Encoding encoding = Encoding.UTF8;
            Print(builder, Model.Volume.PathTableGroups, encoding);
            Print(builder, Model.Volume.DirectoryDescriptors, encoding);
        }
    }
}


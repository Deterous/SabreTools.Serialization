using System;
using System.Text;
using SabreTools.Data.Models.OperaFS;
using SabreTools.Text.Extensions;

namespace SabreTools.Wrappers
{
    public partial class OperaFS : IPrintable
    {
#if NETCOREAPP
        /// <inheritdoc/>
        public string ExportJSON() => System.Text.Json.JsonSerializer.Serialize(Model, _jsonSerializerOptions);
#endif

        /// <inheritdoc/>
        public void PrintInformation(StringBuilder builder)
        {
            builder.AppendLine("3DO / M2 (Opera) Filesystem Information:");
            builder.AppendLine("-------------------------");
            builder.AppendLine();

            Print(builder, Model.VolumeDescriptor);
        }

        internal static void Print(StringBuilder builder, VolumeDescriptor vd)
        {
            builder.AppendLine("  Volume Descriptor:");
            builder.AppendLine("  -------------------------");

            builder.AppendLine(vd.RecordType, "    Record Type");
            builder.AppendLine(vd.VolumeSyncBytes, "    Volume Sync Bytes");
            builder.AppendLine(vd.StructureVersion, "    Structure Version");
            builder.AppendLine((byte)vd.VolumeFlags, "    Volume Flags");
            builder.AppendLine(Encoding.UTF8.GetString(vd.VolumeCommentary), "    Volume Commentary");
            builder.AppendLine(Encoding.UTF8.GetString(vd.VolumeIdentifier), "    Volume Identifier");
            builder.AppendLine(vd.VolumeUniqueIdentifier, "    Volume Unique Identifier");
            builder.AppendLine(vd.VolumeBlockSize, "    Volume Block Size");
            builder.AppendLine(vd.VolumeBlockCount, "    Volume Block Count");
            builder.AppendLine(vd.RootUniqueIdentifier, "    Root Unique Identifier");
            builder.AppendLine(vd.RootDirectoryBlockCount, "    Root Directory Block Count");
            builder.AppendLine(vd.RootDirectoryBlockSize, "    Root Directory Block Size");
            builder.AppendLine(vd.RootDirectoryLastAvatarIndex, "    Root Directory Last Avatar Index");
            builder.AppendLine(vd.RootDirectoryAvatarList, "    Root Directory Avatar List");

            int offset = Array.IndexOf(Constants.PaddingBytes, vd.Padding[0]);
            int index = 0;
            bool isDuck = Array.TrueForAll(vd.Padding, b => b == Constants.PaddingBytes[(index++ + offset) % Constants.PaddingBytes.Length]);
            if (isDuck)
                builder.AppendLine("Expected data", "    Padding");
            else
                builder.AppendLine("Unexpected data", "    Padding");            

            builder.AppendLine();
        }
    }
}

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
            foreach (var kvp in Model.Directories)
                Print(builder, kvp.Key, kvp.Value);
        }

        internal static void Print(StringBuilder builder, VolumeDescriptor vd)
        {
            builder.AppendLine("  Volume Descriptor:");
            builder.AppendLine("  -------------------------");

            builder.AppendLine(vd.RecordType, "  Record Type");
            builder.AppendLine(vd.VolumeSyncBytes, "  Volume Sync Bytes");
            builder.AppendLine(vd.StructureVersion, "  Structure Version");
            builder.AppendLine(vd.VolumeFlags, "  Volume Flags");
            builder.AppendLine(Encoding.UTF8.GetString(vd.VolumeCommentary), "  Volume Commentary");
            builder.AppendLine(Encoding.UTF8.GetString(vd.VolumeIdentifier), "  Volume Identifier");
            builder.AppendLine(vd.VolumeUniqueIdentifier, "  Volume Unique Identifier");
            builder.AppendLine(vd.VolumeBlockSize, "  Volume Block Size");
            builder.AppendLine(vd.VolumeBlockCount, "  Volume Block Count");
            builder.AppendLine(vd.RootUniqueIdentifier, "  Root Unique Identifier");
            builder.AppendLine(vd.RootDirectoryBlockCount, "  Root Directory Block Count");
            builder.AppendLine(vd.RootDirectoryBlockSize, "  Root Directory Block Size");
            builder.AppendLine(vd.RootDirectoryLastAvatarIndex, "  Root Directory Last Avatar Index");
            builder.AppendLine(vd.RootDirectoryAvatarList, "  Root Directory Avatar List");

            int offset = Array.IndexOf(Constants.PaddingBytes, vd.Padding[0]);
            int index = 0;
            bool isDuck = Array.TrueForAll(vd.Padding, b => b == Constants.PaddingBytes[(index++ + offset) % Constants.PaddingBytes.Length]);
            if (isDuck)
                builder.AppendLine("Expected data", "  Padding");
            else
                builder.AppendLine("Unexpected data", "  Padding");            

            builder.AppendLine();
        }

        internal static void Print(StringBuilder builder, uint sector, DirectoryDescriptor dir)
        {
            builder.AppendLine($"  Directory Descriptor (Sector {sector}):");
            builder.AppendLine("  -------------------------");

            builder.AppendLine(dir.NextBlock, "  Next Block");
            builder.AppendLine(dir.PreviousBlock, "  Previous Block");
            builder.AppendLine(dir.Flags, "  Flags");
            builder.AppendLine(dir.FirstFreeByte, "  First Free Byte");
            builder.AppendLine(dir.FirstEntryOffset, "  First Entry Offset");

            foreach (var dr in dir.DirectoryRecords)
            {
                Print(builder, dr);
            }

            builder.AppendLine();
        }

        internal static void Print(StringBuilder builder, DirectoryRecord dr)
        {
            builder.AppendLine("    Directory Record:");
            builder.AppendLine("    -------------------------");

            builder.AppendLine(dir.DirectoryRecordFlags, "    Directory Record Flags");
            builder.AppendLine(dir.UniqueIdentifier, "    Unique Identifier");
            builder.AppendLine(Encoding.UTF8.GetString(dir.Type), "    Type");
            builder.AppendLine(dir.BlockSize, "    BlockSize");
            builder.AppendLine(dir.ByteCount, "    ByteCount");
            builder.AppendLine(dir.BlockCount, "    BlockCount");
            builder.AppendLine(dir.Burst, "    Burst");
            builder.AppendLine(dir.Gap, "    Gap");
            builder.AppendLine(Encoding.UTF8.GetString(dir.Filename), "    Filename");
            builder.AppendLine(dir.LastAvatarIndex, "    LastAvatarIndex");
            builder.AppendLine(dir.AvatarList, "    AvatarList");

            builder.AppendLine();
        }
    }
}

using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Map of printed directories to their sector offset
        /// </summary>
        private readonly Dictionary<DirectoryDescriptor, uint> printedDirectories = [];

        /// <inheritdoc/>
        public void PrintInformation(StringBuilder builder)
        {
            Console.WriteLine("1");
            builder.AppendLine("3DO / M2 (Opera) Filesystem Information:");
            builder.AppendLine("-------------------------");
            builder.AppendLine();

            Print(builder, VolumeDescriptor);
            Console.WriteLine("2");
            foreach (var kvp in Directories)
            {
                if (printedDirectories.ContainsKey(kvp.Value))
                {
                    builder.AppendLine($"  Directory Descriptor (Sector {kvp.Key}):");
                    builder.AppendLine("  -------------------------");
                    builder.AppendLine($"  Duplicate of Descriptor at Sector {printedDirectories[kvp.Value]}");
                    builder.AppendLine();
                    continue;
                }

                Console.WriteLine("3a");
                Print(builder, kvp.Key, kvp.Value);
                Console.WriteLine("3b");
                printedDirectories.Add(kvp.Value, kvp.Key);
            }
        }

        internal static void Print(StringBuilder builder, VolumeDescriptor vd)
        {
            builder.AppendLine("  Volume Descriptor:");
            builder.AppendLine("  -------------------------");

            builder.AppendLine(vd.RecordType, "  Record Type");
            builder.AppendLine(vd.VolumeSyncBytes, "  Volume Sync Bytes");
            builder.AppendLine(vd.StructureVersion, "  Structure Version");

            Console.WriteLine("a");
            builder.AppendLine((byte)vd.VolumeFlags, "  Volume Flags");
            if ((byte)vd.VolumeFlags != 0)
            {
                builder.AppendLine("    Volume Flags (Parsed)");
                builder.AppendLine((vd.VolumeFlags & VolumeFlags.M2) == VolumeFlags.M2, "      M2 Disc (or M1 Data Disc)");
                builder.AppendLine((vd.VolumeFlags & VolumeFlags.M2_ONLY) == VolumeFlags.M2_ONLY, "      M2 Only");
                builder.AppendLine((vd.VolumeFlags & VolumeFlags.M2_DATA_DISC) == VolumeFlags.M2_DATA_DISC, "      M2 Data Disc");
                builder.AppendLine((vd.VolumeFlags & VolumeFlags.M2_SIGNED) == VolumeFlags.M2_SIGNED, "      M2 Signed");
                builder.AppendLine((vd.VolumeFlags & VolumeFlags.RESERVED_MASK) != 0, "      Reserved Bits Set");
            }

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

            Console.WriteLine("b");
            int offset = Array.IndexOf(Constants.PaddingBytes, vd.Padding[0]);
            int index = 0;
            bool isDuck = Array.TrueForAll(vd.Padding, b => b == Constants.PaddingBytes[(index++ + offset) % Constants.PaddingBytes.Length]);
            if (isDuck)
                builder.AppendLine("Expected data", "  Padding");
            else
                builder.AppendLine("Unexpected data", "  Padding");
            Console.WriteLine("c");       

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

            Console.WriteLine(".");
            foreach (var dr in dir.DirectoryRecords)
            {
                Console.WriteLine("..");
                Print(builder, dr);
            }
            Console.WriteLine("...");

            builder.AppendLine();
        }

        internal static void Print(StringBuilder builder, DirectoryRecord dr)
        {
            builder.AppendLine("    Directory Record:");
            builder.AppendLine("    -------------------------");

            builder.AppendLine((byte)dr.DirectoryRecordFlags, "    Directory Record Flags");
            Console.WriteLine("abc");
            if ((byte)dr.DirectoryRecordFlags != 0)
            {
                builder.AppendLine("    Directory Record Flags (Parsed)");
                builder.AppendLine((dr.DirectoryRecordFlags & DirectoryRecordFlags.DIRECTORY) == DirectoryRecordFlags.DIRECTORY, "      Directory");
                builder.AppendLine((dr.DirectoryRecordFlags & DirectoryRecordFlags.READ_ONLY) == DirectoryRecordFlags.READ_ONLY, "      Read-only");
                builder.AppendLine((dr.DirectoryRecordFlags & DirectoryRecordFlags.SYSTEM) == DirectoryRecordFlags.SYSTEM, "      Filesystem Use");
                builder.AppendLine((dr.DirectoryRecordFlags & DirectoryRecordFlags.BLOCK_FINAL) == DirectoryRecordFlags.BLOCK_FINAL, "      Final in Block");
                builder.AppendLine((dr.DirectoryRecordFlags & DirectoryRecordFlags.DIRECTORY_FINAL) == DirectoryRecordFlags.DIRECTORY_FINAL, "      Final in Directory");
                builder.AppendLine((dr.DirectoryRecordFlags & DirectoryRecordFlags.RESERVED_MASK) != 0, "      Reserved Bits Set");
            }

            builder.AppendLine(dr.UniqueIdentifier, "    Unique Identifier");
            builder.AppendLine(dr.Type, "    Type");
            builder.AppendLine(Encoding.UTF8.GetString(dr.Type), "    Type (Parsed)");
            builder.AppendLine(dr.BlockSize, "    BlockSize");
            builder.AppendLine(dr.ByteCount, "    ByteCount");
            builder.AppendLine(dr.BlockCount, "    BlockCount");
            builder.AppendLine(dr.Burst, "    Burst");
            builder.AppendLine(dr.Gap, "    Gap");
            builder.AppendLine(dr.Filename, "    Filename");
            builder.AppendLine(Encoding.UTF8.GetString(dr.Filename), "    Filename (Parsed)");
            builder.AppendLine(dr.LastAvatarIndex, "    LastAvatarIndex");
            builder.AppendLine(dr.AvatarList, "    AvatarList");
            Console.WriteLine("123");

            builder.AppendLine();
        }
    }
}

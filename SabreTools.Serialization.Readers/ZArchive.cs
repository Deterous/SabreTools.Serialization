using System.Collections.Generic;
using System.IO;
using SabreTools.Data.Models.ZArchive;
using SabreTools.Hashing;
using SabreTools.IO.Extensions;
using SabreTools.Matching;
using SabreTools.Numerics.Extensions;

#pragma warning disable IDE0017 // Simplify object initialization
namespace SabreTools.Serialization.Readers
{
    public class ZArchive : BaseBinaryReader<Archive>
    {
        /// <inheritdoc/>
        public override Archive? Deserialize(Stream? data)
        {
            // If the data is invalid
            if (data is null || !data.CanRead)
                return null;

            // Simple check for a valid stream length
            if (data.Length - data.Position <  Constants.FooterSize)
                return null;

            try
            {
                // Cache the current offset
                long initialOffset = data.Position;

                var archive = new Archive();

                // Parse the footer first
                data.SeekIfPossible(-Constants.FooterSize, SeekOrigin.End);
                var footer = ParseFooter(data, initialOffset);
                if (footer is not null)
                    archive.Footer = footer;
                else
                    return null;

                // Seek to and then read the compression offset records
                data.SeekIfPossible(archive.Footer.SectionOffsetRecords.Offset, SeekOrigin.Begin);
                var offsetRecords = ParseOffsetRecords(data, archive.Footer.SectionOffsetRecords.Size);
                if (offsetRecords is not null)
                    archive.OffsetRecords = offsetRecords;
                else
                    return null;

                // Seek to and then read the name table entries
                data.SeekIfPossible(archive.Footer.SectionNameTable.Offset, SeekOrigin.Begin);
                var nameTable = ParseNameTable(data, archive.Footer.SectionNameTable.Size);
                if (nameTable is not null)
                    archive.NameTable = nameTable;
                else
                    return null;

                // Seek to and then read the file tree entries
                data.SeekIfPossible(archive.Footer.SectionFileTree.Offset, SeekOrigin.Begin);
                var fileTree = ParseFileTree(data, archive.Footer.SectionFileTree.Size, archive.Footer.SectionNameTable.Size);
                if (fileTree is not null)
                    archive.FileTree = fileTree;
                else
                    return null;

                // Do not attempt to read compressed data into memory

                return archive;
            }
            catch
            {
                // Ignore the actual error
                return null;
            }
        }

        /// <summary>
        /// Parse a Stream into an ZArchive footer
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled ZArchive footer on success, null on error</returns>
        public static Footer? ParseFooter(Stream data, long initialOffset)
        {
            var obj = new Footer();

            // Read and validate section offset and size values
            obj.SectionLocalFiles.Offset = data.ReadUInt64BigEndian();
            obj.SectionLocalFiles.Size = data.ReadUInt64BigEndian();
            if (obj.SectionLocalFiles.Offset + obj.SectionLocalFiles.Size > data.Length)
                return null;

            // Read and validate section offset and size values
            obj.SectionOffsetRecords.Offset = data.ReadUInt64BigEndian();
            obj.SectionOffsetRecords.Size = data.ReadUInt64BigEndian();
            if (obj.SectionOffsetRecords.Offset + obj.SectionOffsetRecords.Size > data.Length)
                return null;
            if (obj.SectionOffsetRecords.Size > Constants.MaxOffsetRecordsSize)
                return null;
            if (obj.SectionOffsetRecords.Size % Constants.OffsetRecordSize != 0)
                return null;

            // Read and validate section offset and size values
            obj.SectionNameTable.Offset = data.ReadUInt64BigEndian();
            obj.SectionNameTable.Size = data.ReadUInt64BigEndian();
            if (obj.SectionNameTable.Offset + obj.SectionNameTable.Size > data.Length)
                return null;
            if (obj.SectionNameTable.Size > Constants.MaxNameTableSize)
                return null;

            // Read and validate section offset and size values
            obj.SectionFileTree.Offset = data.ReadUInt64BigEndian();
            obj.SectionFileTree.Size = data.ReadUInt64BigEndian();
            if (obj.SectionFileTree.Offset + obj.SectionFileTree.Size > data.Length)
                return null;
            if (obj.SectionFileTree.Size > Constants.MaxFileTreeSize)
                return null;
            if (obj.SectionFileTree.Size % Constants.FileDirectoryEntrySize != 0)
                return null;

            // Read and validate section offset and size values
            obj.SectionMetaDirectory.Offset = data.ReadUInt64BigEndian();
            obj.SectionMetaDirectory.Size = data.ReadUInt64BigEndian();
            if (obj.SectionMetaDirectory.Offset + obj.SectionMetaDirectory.Size > data.Length)
                return null;

            // Read and validate section offset and size values
            obj.SectionMetaData.Offset = data.ReadUInt64BigEndian();
            obj.SectionMetaData.Size = data.ReadUInt64BigEndian();
            if (obj.SectionMetaData.Offset + obj.SectionMetaData.Size > data.Length)
                return null;

            // Read and validate archive integrity hash
            obj.IntegrityHash = data.ReadBytes(32);
            // data.SeekIfPossible(initialOffset, SeekOrigin.Begin);
            // TODO: Read all bytes and hash them with SHA256
            // TODO: Compare obj.Integrity with calculated hash

            // Read and validate archive size
            obj.Size = data.ReadUInt64BigEndian();
            if (obj.Size != data.Length - initialOffset)
                return null;

            // Read and validate version bytes, only Version 1 is supported
            obj.Version = data.ReadBytes(4);
            if (!obj.Version.EqualsExactly(Constants.Version1Bytes))
                return null;

            // Read and validate magic bytes
            obj.Magic = data.ReadBytes(4);
            if (!obj.Magic.EqualsExactly(Constants.MagicBytes))
                return null;

            return obj;
        }

        /// <summary>
        /// Parse a Stream into an ZArchive OffsetRecords section
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <param name="size">Size of OffsetRecords section</param>
        /// <returns>Filled ZArchive OffsetRecords section on success, null on error</returns>
        public static OffsetRecord[]? ParseOffsetRecords(Stream data, ulong size)
        {
            int entries = (int)(size / Constants.OffsetRecordSize);

            var obj = new OffsetRecord[entries];

            for (int i = 0; i < entries; i++)
            {
                obj[i].Offset = data.ReadUInt64BigEndian();
                for (int block = 0; block < Constants.BlocksPerOffsetRecord; block++)
                {
                    obj[i].Size[block] = data.ReadUInt16BigEndian();
                }
            }

            return obj;
        }

        /// <summary>
        /// Parse a Stream into an ZArchive NameTable section
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <param name="size">Size of NameTable section</param>
        /// <returns>Filled ZArchive NameTable section on success, null on error</returns>
        public static NameTable? ParseNameTable(Stream data, ulong size)
        {
            var obj = new NameTable();
            var nameEntries = new List<NameEntry>();
            var nameOffsets = new List<uint>();

            uint bytesRead = 0;

            while (bytesRead < (uint)size)
            {
                var nameEntry = new NameEntry();

                // Cache the offset into the NameEntry table
                nameOffsets.Add(bytesRead);                

                // Read length of name
                uint nameLength = (uint)data.ReadByteValue();
                bytesRead += 1;
                if ((nameLength & 0x80) == 0x80)
                {
                    nameLength += (uint)data.ReadByteValue() << 7;
                    bytesRead += 1;
                    nameEntry.NodeLengthLong = (ushort)nameLength;
                }
                else
                {
                    nameEntry.NodeLengthShort = (byte)nameLength;
                }

                // Validate name length
                if (bytesRead + nameLength > (uint)size)
                    return null;

                // Add valid name entry to the table
                nameEntry.NodeName = data.ReadBytes(nameLength);
                bytesRead += nameLength;
                nameEntries.Add(nameEntry);
            }

            obj.NameEntries = [..nameEntries];
            obj.NameOffsets = [..nameOffsets];

            return obj;
        }

        /// <summary>
        /// Parse a Stream into an ZArchive FileTree section
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <param name="size">Size of FileTree section</param>
        /// <returns>Filled ZArchive FileTree section on success, null on error</returns>
        public static FileDirectoryEntry[]? ParseFileTree(Stream data, ulong size, uint nameTableSize)
        {
            int entries = (int)(size / Constants.FileDirectoryEntrySize);

            var obj = new FileDirectoryEntry[entries];

            for (int i = 0; i < entries; i++)
            {
                obj[i].NameOffsetAndTypeFlag = data.ReadUInt32BigEndian();

                // Validate name table offset value
                if ((obj[i].NameOffsetAndTypeFlag & 0x7FFFFFFF) > nameTableSize)
                    return null;

                if (obj[i].IsFile)
                {
                    if (obj[i] is FileEntry fileEntry)
                    {
                        fileEntry.FileOffsetLow = data.ReadUInt32BigEndian();
                        fileEntry.FileSizeLow = data.ReadUInt32BigEndian();
                        fileEntry.FileOffsetHigh = data.ReadUInt16BigEndian();
                        fileEntry.FileSizeHigh = data.ReadUInt16BigEndian();
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (obj[i] is DirectoryEntry directoryEntry)
                    {
                        directoryEntry.NodeStartIndex = data.ReadUInt32BigEndian();
                        directoryEntry.Count = data.ReadUInt32BigEndian();
                        directoryEntry.Reserved = data.ReadUInt32BigEndian();
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            // First entry of file tree must be root directory
            if ((obj[0].NameOffsetAndTypeFlag & 0x7FFFFFFF) != 0x7FFFFFFF)
                return null;

            return obj;
        }
    }
}

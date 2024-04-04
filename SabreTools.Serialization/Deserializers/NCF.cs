using System.Collections.Generic;
using System.IO;
using System.Text;
using SabreTools.IO;
using SabreTools.Models.NCF;
using SabreTools.Serialization.Interfaces;

namespace SabreTools.Serialization.Deserializers
{
    public class NCF :
        BaseBinaryDeserializer<Models.NCF.File>,
        IFileDeserializer<Models.NCF.File>,
        IStreamDeserializer<Models.NCF.File>
    {
        #region IFileDeserializer

        /// <inheritdoc/>
        public Models.NCF.File? Deserialize(string? path)
        {
            using var stream = PathProcessor.OpenStream(path);
            return DeserializeStream(stream);
        }

        #endregion

        #region IStreamDeserializer

        /// <inheritdoc/>
        public Models.NCF.File? Deserialize(Stream? data)
        {
            // If the data is invalid
            if (data == null || data.Length == 0 || !data.CanSeek || !data.CanRead)
                return null;

            // If the offset is out of bounds
            if (data.Position < 0 || data.Position >= data.Length)
                return null;

            // Cache the current offset
            long initialOffset = data.Position;

            // Create a new Half-Life No Cache to fill
            var file = new Models.NCF.File();

            #region Header

            // Try to parse the header
            var header = ParseHeader(data);
            if (header == null)
                return null;

            // Set the no cache header
            file.Header = header;

            #endregion

            // Cache the current offset
            initialOffset = data.Position;

            #region Directory Header

            // Try to parse the directory header
            var directoryHeader = ParseDirectoryHeader(data);
            if (directoryHeader == null)
                return null;

            // Set the game cache directory header
            file.DirectoryHeader = directoryHeader;

            #endregion

            #region Directory Entries

            // Create the directory entry array
            file.DirectoryEntries = new DirectoryEntry[directoryHeader.ItemCount];

            // Try to parse the directory entries
            for (int i = 0; i < directoryHeader.ItemCount; i++)
            {
                var directoryEntry = ParseDirectoryEntry(data);
                file.DirectoryEntries[i] = directoryEntry;
            }

            #endregion

            #region Directory Names

            if (directoryHeader.NameSize > 0)
            {
                // Get the current offset for adjustment
                long directoryNamesStart = data.Position;

                // Get the ending offset
                long directoryNamesEnd = data.Position + directoryHeader.NameSize;

                // Create the string dictionary
                file.DirectoryNames = new Dictionary<long, string?>();

                // Loop and read the null-terminated strings
                while (data.Position < directoryNamesEnd)
                {
                    long nameOffset = data.Position - directoryNamesStart;
                    string? directoryName = data.ReadString(Encoding.ASCII);
                    if (data.Position > directoryNamesEnd)
                    {
                        data.Seek(-directoryName?.Length ?? 0, SeekOrigin.Current);
                        byte[]? endingData = data.ReadBytes((int)(directoryNamesEnd - data.Position));
                        if (endingData != null)
                            directoryName = Encoding.ASCII.GetString(endingData);
                        else
                            directoryName = null;
                    }

                    file.DirectoryNames[nameOffset] = directoryName;
                }

                // Loop and assign to entries
                foreach (var directoryEntry in file.DirectoryEntries)
                {
                    if (directoryEntry != null)
                        directoryEntry.Name = file.DirectoryNames[directoryEntry.NameOffset];
                }
            }

            #endregion

            #region Directory Info 1 Entries

            // Create the directory info 1 entry array
            file.DirectoryInfo1Entries = new DirectoryInfo1Entry[directoryHeader.Info1Count];

            // Try to parse the directory info 1 entries
            for (int i = 0; i < directoryHeader.Info1Count; i++)
            {
                var directoryInfo1Entry = ParseDirectoryInfo1Entry(data);
                file.DirectoryInfo1Entries[i] = directoryInfo1Entry;
            }

            #endregion

            #region Directory Info 2 Entries

            // Create the directory info 2 entry array
            file.DirectoryInfo2Entries = new DirectoryInfo2Entry[directoryHeader.ItemCount];

            // Try to parse the directory info 2 entries
            for (int i = 0; i < directoryHeader.ItemCount; i++)
            {
                var directoryInfo2Entry = ParseDirectoryInfo2Entry(data);
                file.DirectoryInfo2Entries[i] = directoryInfo2Entry;
            }

            #endregion

            #region Directory Copy Entries

            // Create the directory copy entry array
            file.DirectoryCopyEntries = new DirectoryCopyEntry[directoryHeader.CopyCount];

            // Try to parse the directory copy entries
            for (int i = 0; i < directoryHeader.CopyCount; i++)
            {
                var directoryCopyEntry = ParseDirectoryCopyEntry(data);
                file.DirectoryCopyEntries[i] = directoryCopyEntry;
            }

            #endregion

            #region Directory Local Entries

            // Create the directory local entry array
            file.DirectoryLocalEntries = new DirectoryLocalEntry[directoryHeader.LocalCount];

            // Try to parse the directory local entries
            for (int i = 0; i < directoryHeader.LocalCount; i++)
            {
                var directoryLocalEntry = ParseDirectoryLocalEntry(data);
                file.DirectoryLocalEntries[i] = directoryLocalEntry;
            }

            #endregion

            // Seek to end of directory section, just in case
            data.Seek(initialOffset + directoryHeader.DirectorySize, SeekOrigin.Begin);

            #region Unknown Header

            // Try to parse the unknown header
            var unknownHeader = ParseUnknownHeader(data);
            if (unknownHeader == null)
                return null;

            // Set the game cache unknown header
            file.UnknownHeader = unknownHeader;

            #endregion

            #region Unknown Entries

            // Create the unknown entry array
            file.UnknownEntries = new UnknownEntry[directoryHeader.ItemCount];

            // Try to parse the unknown entries
            for (int i = 0; i < directoryHeader.ItemCount; i++)
            {
                var unknownEntry = ParseUnknownEntry(data);
                file.UnknownEntries[i] = unknownEntry;
            }

            #endregion

            #region Checksum Header

            // Try to parse the checksum header
            var checksumHeader = ParseChecksumHeader(data);
            if (checksumHeader == null)
                return null;

            // Set the game cache checksum header
            file.ChecksumHeader = checksumHeader;

            #endregion

            // Cache the current offset
            initialOffset = data.Position;

            #region Checksum Map Header

            // Try to parse the checksum map header
            var checksumMapHeader = ParseChecksumMapHeader(data);
            if (checksumMapHeader == null)
                return null;

            // Set the game cache checksum map header
            file.ChecksumMapHeader = checksumMapHeader;

            #endregion

            #region Checksum Map Entries

            // Create the checksum map entry array
            file.ChecksumMapEntries = new ChecksumMapEntry[checksumMapHeader.ItemCount];

            // Try to parse the checksum map entries
            for (int i = 0; i < checksumMapHeader.ItemCount; i++)
            {
                var checksumMapEntry = ParseChecksumMapEntry(data);
                file.ChecksumMapEntries[i] = checksumMapEntry;
            }

            #endregion

            #region Checksum Entries

            // Create the checksum entry array
            file.ChecksumEntries = new ChecksumEntry[checksumMapHeader.ChecksumCount];

            // Try to parse the checksum entries
            for (int i = 0; i < checksumMapHeader.ChecksumCount; i++)
            {
                var checksumEntry = ParseChecksumEntry(data);
                file.ChecksumEntries[i] = checksumEntry;
            }

            #endregion

            // Seek to end of checksum section, just in case
            data.Seek(initialOffset + checksumHeader.ChecksumSize, SeekOrigin.Begin);

            return file;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life No Cache header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life No Cache header on success, null on error</returns>
        private static Header? ParseHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            Header header = new Header();

            header.Dummy0 = data.ReadUInt32();
            if (header.Dummy0 != 0x00000001)
                return null;

            header.MajorVersion = data.ReadUInt32();
            if (header.MajorVersion != 0x00000002)
                return null;

            header.MinorVersion = data.ReadUInt32();
            if (header.MinorVersion != 1)
                return null;

            header.CacheID = data.ReadUInt32();
            header.LastVersionPlayed = data.ReadUInt32();
            header.Dummy1 = data.ReadUInt32();
            header.Dummy2 = data.ReadUInt32();
            header.FileSize = data.ReadUInt32();
            header.BlockSize = data.ReadUInt32();
            header.BlockCount = data.ReadUInt32();
            header.Dummy3 = data.ReadUInt32();

            return header;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life No Cache directory header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life No Cache directory header on success, null on error</returns>
        private static DirectoryHeader? ParseDirectoryHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            DirectoryHeader directoryHeader = new DirectoryHeader();

            directoryHeader.Dummy0 = data.ReadUInt32();
            if (directoryHeader.Dummy0 != 0x00000004)
                return null;

            directoryHeader.CacheID = data.ReadUInt32();
            directoryHeader.LastVersionPlayed = data.ReadUInt32();
            directoryHeader.ItemCount = data.ReadUInt32();
            directoryHeader.FileCount = data.ReadUInt32();
            directoryHeader.ChecksumDataLength = data.ReadUInt32();
            directoryHeader.DirectorySize = data.ReadUInt32();
            directoryHeader.NameSize = data.ReadUInt32();
            directoryHeader.Info1Count = data.ReadUInt32();
            directoryHeader.CopyCount = data.ReadUInt32();
            directoryHeader.LocalCount = data.ReadUInt32();
            directoryHeader.Dummy1 = data.ReadUInt32();
            directoryHeader.Dummy2 = data.ReadUInt32();
            directoryHeader.Checksum = data.ReadUInt32();

            return directoryHeader;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life No Cache directory entry
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life No Cache directory entry on success, null on error</returns>
        private static DirectoryEntry ParseDirectoryEntry(Stream data)
        {
            // TODO: Use marshalling here instead of building
            DirectoryEntry directoryEntry = new DirectoryEntry();

            directoryEntry.NameOffset = data.ReadUInt32();
            directoryEntry.ItemSize = data.ReadUInt32();
            directoryEntry.ChecksumIndex = data.ReadUInt32();
            directoryEntry.DirectoryFlags = (HL_NCF_FLAG)data.ReadUInt32();
            directoryEntry.ParentIndex = data.ReadUInt32();
            directoryEntry.NextIndex = data.ReadUInt32();
            directoryEntry.FirstIndex = data.ReadUInt32();

            return directoryEntry;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life No Cache directory info 1 entry
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life No Cache directory info 1 entry on success, null on error</returns>
        private static DirectoryInfo1Entry ParseDirectoryInfo1Entry(Stream data)
        {
            // TODO: Use marshalling here instead of building
            DirectoryInfo1Entry directoryInfo1Entry = new DirectoryInfo1Entry();

            directoryInfo1Entry.Dummy0 = data.ReadUInt32();

            return directoryInfo1Entry;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life No Cache directory info 2 entry
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life No Cache directory info 2 entry on success, null on error</returns>
        private static DirectoryInfo2Entry ParseDirectoryInfo2Entry(Stream data)
        {
            // TODO: Use marshalling here instead of building
            DirectoryInfo2Entry directoryInfo2Entry = new DirectoryInfo2Entry();

            directoryInfo2Entry.Dummy0 = data.ReadUInt32();

            return directoryInfo2Entry;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life No Cache directory copy entry
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life No Cache directory copy entry on success, null on error</returns>
        private static DirectoryCopyEntry ParseDirectoryCopyEntry(Stream data)
        {
            // TODO: Use marshalling here instead of building
            DirectoryCopyEntry directoryCopyEntry = new DirectoryCopyEntry();

            directoryCopyEntry.DirectoryIndex = data.ReadUInt32();

            return directoryCopyEntry;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life No Cache directory local entry
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life No Cache directory local entry on success, null on error</returns>
        private static DirectoryLocalEntry ParseDirectoryLocalEntry(Stream data)
        {
            // TODO: Use marshalling here instead of building
            DirectoryLocalEntry directoryLocalEntry = new DirectoryLocalEntry();

            directoryLocalEntry.DirectoryIndex = data.ReadUInt32();

            return directoryLocalEntry;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life No Cache unknown header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life No Cache unknown header on success, null on error</returns>
        private static UnknownHeader? ParseUnknownHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            UnknownHeader unknownHeader = new UnknownHeader();

            unknownHeader.Dummy0 = data.ReadUInt32();
            if (unknownHeader.Dummy0 != 0x00000001)
                return null;

            unknownHeader.Dummy1 = data.ReadUInt32();
            if (unknownHeader.Dummy1 != 0x00000000)
                return null;

            return unknownHeader;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life No Cache unknown entry
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life No Cacheunknown entry on success, null on error</returns>
        private static UnknownEntry ParseUnknownEntry(Stream data)
        {
            // TODO: Use marshalling here instead of building
            UnknownEntry unknownEntry = new UnknownEntry();

            unknownEntry.Dummy0 = data.ReadUInt32();

            return unknownEntry;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life No Cache checksum header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life No Cache checksum header on success, null on error</returns>
        private static ChecksumHeader? ParseChecksumHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            ChecksumHeader checksumHeader = new ChecksumHeader();

            checksumHeader.Dummy0 = data.ReadUInt32();
            if (checksumHeader.Dummy0 != 0x00000001)
                return null;

            checksumHeader.ChecksumSize = data.ReadUInt32();

            return checksumHeader;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life No Cache checksum map header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life No Cache checksum map header on success, null on error</returns>
        private static ChecksumMapHeader? ParseChecksumMapHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            ChecksumMapHeader checksumMapHeader = new ChecksumMapHeader();

            checksumMapHeader.Dummy0 = data.ReadUInt32();
            if (checksumMapHeader.Dummy0 != 0x14893721)
                return null;

            checksumMapHeader.Dummy1 = data.ReadUInt32();
            if (checksumMapHeader.Dummy1 != 0x00000001)
                return null;

            checksumMapHeader.ItemCount = data.ReadUInt32();
            checksumMapHeader.ChecksumCount = data.ReadUInt32();

            return checksumMapHeader;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life No Cache checksum map entry
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life No Cache checksum map entry on success, null on error</returns>
        private static ChecksumMapEntry ParseChecksumMapEntry(Stream data)
        {
            // TODO: Use marshalling here instead of building
            ChecksumMapEntry checksumMapEntry = new ChecksumMapEntry();

            checksumMapEntry.ChecksumCount = data.ReadUInt32();
            checksumMapEntry.FirstChecksumIndex = data.ReadUInt32();

            return checksumMapEntry;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life No Cache checksum entry
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life No Cache checksum entry on success, null on error</returns>
        private static ChecksumEntry ParseChecksumEntry(Stream data)
        {
            // TODO: Use marshalling here instead of building
            ChecksumEntry checksumEntry = new ChecksumEntry();

            checksumEntry.Checksum = data.ReadUInt32();

            return checksumEntry;
        }

        #endregion
    }
}
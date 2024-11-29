using System.Collections.Generic;
using System.IO;
using System.Text;
using SabreTools.IO.Extensions;
using SabreTools.Models.AACS;

namespace SabreTools.Serialization.Deserializers
{
    public class AACS : BaseBinaryDeserializer<MediaKeyBlock>
    {
        /// <inheritdoc/>
        public override MediaKeyBlock? Deserialize(Stream? data)
        {
            // If the data is invalid
            if (data == null || !data.CanRead)
                return null;

            try
            {
                // Create a new media key block to fill
                var mediaKeyBlock = new MediaKeyBlock();

                #region Records

                // Create the records list
                var records = new List<Record>();

                // Try to parse the records
                while (data.Position < data.Length)
                {
                    // Try to parse the record
                    var record = ParseRecord(data);
                    if (record == null)
                        return null;

                    // Add the record
                    records.Add(record);

                    // If we have an end of media key block record
                    if (record.RecordType == RecordType.EndOfMediaKeyBlock)
                        break;

                    // Align to the 4-byte boundary if we're not at the end
                    if (data.Position < data.Length)
                    {
                        while (data.Position < data.Length && (data.Position % 4) != 0)
                            _ = data.ReadByteValue();
                    }
                    else
                    {
                        break;
                    }
                }

                #endregion

                // Set the records
                if (records.Count > 0)
                    return new MediaKeyBlock { Records = [.. records ]};

                return null;
            }
            catch
            {
                // Ignore the actual error
                return null;
            }
        }

        /// <summary>
        /// Parse a Stream into a record
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled record on success, null on error</returns>
        private static Record? ParseRecord(Stream data)
        {
            // The first 4 bytes make up the type and length
            RecordType type = (RecordType)data.ReadByteValue();
            uint length = data.ReadUInt24();

            // Create a record based on the type
            return type switch
            {
                // Known record types
                RecordType.EndOfMediaKeyBlock => ParseEndOfMediaKeyBlockRecord(data, type, length),
                RecordType.ExplicitSubsetDifference => ParseExplicitSubsetDifferenceRecord(data, type, length),
                RecordType.MediaKeyData => ParseMediaKeyDataRecord(data, type, length),
                RecordType.SubsetDifferenceIndex => ParseSubsetDifferenceIndexRecord(data, type, length),
                RecordType.TypeAndVersion => ParseTypeAndVersionRecord(data, type, length),
                RecordType.DriveRevocationList => ParseDriveRevocationListRecord(data, type, length),
                RecordType.HostRevocationList => ParseHostRevocationListRecord(data, type, length),
                RecordType.VerifyMediaKey => ParseVerifyMediaKeyRecord(data, type, length),
                RecordType.Copyright => ParseCopyrightRecord(data, type, length),

                // Unknown record type
                _ => null,
            };
        }

        /// <summary>
        /// Parse a Stream into an end of media key block record
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled end of media key block record on success, null on error</returns>
        private static EndOfMediaKeyBlockRecord? ParseEndOfMediaKeyBlockRecord(Stream data, RecordType type, uint length)
        {
            // Verify we're calling the right parser
            if (type != RecordType.EndOfMediaKeyBlock)
                return null;

            var record = new EndOfMediaKeyBlockRecord();

            record.RecordType = type;
            record.RecordLength = length;
            if (length > 4)
                record.SignatureData = data.ReadBytes((int)(length - 4));

            return record;
        }

        /// <summary>
        /// Parse a Stream into an explicit subset-difference record
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled explicit subset-difference record on success, null on error</returns>
        private static ExplicitSubsetDifferenceRecord? ParseExplicitSubsetDifferenceRecord(Stream data, RecordType type, uint length)
        {
            // Verify we're calling the right parser
            if (type != RecordType.ExplicitSubsetDifference)
                return null;

            var record = new ExplicitSubsetDifferenceRecord();

            record.RecordType = type;
            record.RecordLength = length;

            // Cache the current offset
            long initialOffset = data.Position - 4;

            // Create the subset difference list
            var subsetDifferences = new List<SubsetDifference>();

            // Try to parse the subset differences
            while (data.Position < initialOffset + length - 5)
            {
                var subsetDifference = new SubsetDifference();

                subsetDifference.Mask = data.ReadByteValue();
                subsetDifference.Number = data.ReadUInt32BigEndian();

                subsetDifferences.Add(subsetDifference);
            }

            // Set the subset differences
            record.SubsetDifferences = [.. subsetDifferences];

            // If there's any data left, discard it
            if (data.Position < initialOffset + length)
                _ = data.ReadBytes((int)(initialOffset + length - data.Position));

            return record;
        }

        /// <summary>
        /// Parse a Stream into a media key data record
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled media key data record on success, null on error</returns>
        private static MediaKeyDataRecord? ParseMediaKeyDataRecord(Stream data, RecordType type, uint length)
        {
            // Verify we're calling the right parser
            if (type != RecordType.MediaKeyData)
                return null;

            var record = new MediaKeyDataRecord();

            record.RecordType = type;
            record.RecordLength = length;

            // Cache the current offset
            long initialOffset = data.Position - 4;

            // Create the media key list
            var mediaKeys = new List<byte[]>();

            // Try to parse the media keys
            while (data.Position < initialOffset + length)
            {
                byte[] mediaKey = data.ReadBytes(0x10);
                mediaKeys.Add(mediaKey);
            }

            // Set the media keys
            record.MediaKeyData = [.. mediaKeys];

            return record;
        }

        /// <summary>
        /// Parse a Stream into a subset-difference index record
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled subset-difference index record on success, null on error</returns>
        private static SubsetDifferenceIndexRecord? ParseSubsetDifferenceIndexRecord(Stream data, RecordType type, uint length)
        {
            // Verify we're calling the right parser
            if (type != RecordType.SubsetDifferenceIndex)
                return null;

            var record = new SubsetDifferenceIndexRecord();

            record.RecordType = type;
            record.RecordLength = length;

            // Cache the current offset
            long initialOffset = data.Position - 4;

            record.Span = data.ReadUInt32BigEndian();

            // Create the offset list
            var offsets = new List<uint>();

            // Try to parse the offsets
            while (data.Position < initialOffset + length)
            {
                uint offset = data.ReadUInt32BigEndian();
                offsets.Add(offset);
            }

            // Set the offsets
            record.Offsets = [.. offsets];

            return record;
        }

        /// <summary>
        /// Parse a Stream into a type and version record
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled type and version record on success, null on error</returns>
        private static TypeAndVersionRecord? ParseTypeAndVersionRecord(Stream data, RecordType type, uint length)
        {
            // Verify we're calling the right parser
            if (type != RecordType.TypeAndVersion)
                return null;

            var record = new TypeAndVersionRecord();

            record.RecordType = type;
            record.RecordLength = length;
            record.MediaKeyBlockType = (MediaKeyBlockType)data.ReadUInt32BigEndian();
            record.VersionNumber = data.ReadUInt32BigEndian();

            return record;
        }

        /// <summary>
        /// Parse a Stream into a drive revocation list record
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled drive revocation list record on success, null on error</returns>
        private static DriveRevocationListRecord? ParseDriveRevocationListRecord(Stream data, RecordType type, uint length)
        {
            // Verify we're calling the right parser
            if (type != RecordType.DriveRevocationList)
                return null;

            var record = new DriveRevocationListRecord();

            record.RecordType = type;
            record.RecordLength = length;

            // Cache the current offset
            long initialOffset = data.Position - 4;

            record.TotalNumberOfEntries = data.ReadUInt32BigEndian();

            // Create the signature blocks list
            var blocks = new List<DriveRevocationSignatureBlock>();

            // Try to parse the signature blocks
            int entryCount = 0;
            while (entryCount < record.TotalNumberOfEntries && data.Position < initialOffset + length)
            {
                var block = new DriveRevocationSignatureBlock();

                block.NumberOfEntries = data.ReadUInt32BigEndian();
                block.EntryFields = new DriveRevocationListEntry[block.NumberOfEntries];
                for (int i = 0; i < block.EntryFields.Length; i++)
                {
                    var entry = new DriveRevocationListEntry();

                    entry.Range = data.ReadUInt16BigEndian();
                    entry.DriveID = data.ReadBytes(6);

                    block.EntryFields[i] = entry;
                    entryCount++;
                }

                blocks.Add(block);

                // If we have an empty block
                if (block.NumberOfEntries == 0)
                    break;
            }

            // Set the signature blocks
            record.SignatureBlocks = [.. blocks];

            // If there's any data left, discard it
            if (data.Position < initialOffset + length)
                _ = data.ReadBytes((int)(initialOffset + length - data.Position));

            return record;
        }

        /// <summary>
        /// Parse a Stream into a host revocation list record
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled host revocation list record on success, null on error</returns>
        private static HostRevocationListRecord? ParseHostRevocationListRecord(Stream data, RecordType type, uint length)
        {
            // Verify we're calling the right parser
            if (type != RecordType.HostRevocationList)
                return null;

            var record = new HostRevocationListRecord();

            record.RecordType = type;
            record.RecordLength = length;

            // Cache the current offset
            long initialOffset = data.Position - 4;

            record.TotalNumberOfEntries = data.ReadUInt32BigEndian();

            // Create the signature blocks list
            var blocks = new List<HostRevocationSignatureBlock>();

            // Try to parse the signature blocks
            int entryCount = 0;
            while (entryCount < record.TotalNumberOfEntries && data.Position < initialOffset + length)
            {
                var block = new HostRevocationSignatureBlock();

                block.NumberOfEntries = data.ReadUInt32BigEndian();
                block.EntryFields = new HostRevocationListEntry[block.NumberOfEntries];
                for (int i = 0; i < block.EntryFields.Length; i++)
                {
                    var entry = new HostRevocationListEntry();

                    entry.Range = data.ReadUInt16BigEndian();
                    entry.HostID = data.ReadBytes(6);

                    block.EntryFields[i] = entry;
                    entryCount++;
                }

                blocks.Add(block);

                // If we have an empty block
                if (block.NumberOfEntries == 0)
                    break;
            }

            // Set the signature blocks
            record.SignatureBlocks = [.. blocks];

            // If there's any data left, discard it
            if (data.Position < initialOffset + length)
                _ = data.ReadBytes((int)(initialOffset + length - data.Position));

            return record;
        }

        /// <summary>
        /// Parse a Stream into a verify media key record
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled verify media key record on success, null on error</returns>
        private static VerifyMediaKeyRecord? ParseVerifyMediaKeyRecord(Stream data, RecordType type, uint length)
        {
            // Verify we're calling the right parser
            if (type != RecordType.VerifyMediaKey)
                return null;

            var record = new VerifyMediaKeyRecord();

            record.RecordType = type;
            record.RecordLength = length;
            record.CiphertextValue = data.ReadBytes(0x10);

            return record;
        }

        /// <summary>
        /// Parse a Stream into a copyright record
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled copyright record on success, null on error</returns>
        private static CopyrightRecord? ParseCopyrightRecord(Stream data, RecordType type, uint length)
        {
            // Verify we're calling the right parser
            if (type != RecordType.Copyright)
                return null;

            var record = new CopyrightRecord();

            record.RecordType = type;
            record.RecordLength = length;
            if (length > 4)
            {
                byte[] copyright = data.ReadBytes((int)(length - 4));
                record.Copyright = Encoding.ASCII.GetString(copyright).TrimEnd('\0');
            }

            return record;
        }
    }
}
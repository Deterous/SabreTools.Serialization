using System;
using System.IO;
using System.Text;
using SabreTools.IO;
using SabreTools.Models.N3DS;
using SabreTools.Serialization.Interfaces;
using static SabreTools.Models.N3DS.Constants;

namespace SabreTools.Serialization.Streams
{
    public partial class N3DS : IStreamSerializer<Cart>
    {
        /// <inheritdoc/>
        public Cart? Deserialize(Stream? data)
        {
            // If the data is invalid
            if (data == null || data.Length == 0 || !data.CanSeek || !data.CanRead)
                return null;

            // If the offset is out of bounds
            if (data.Position < 0 || data.Position >= data.Length)
                return null;

            // Cache the current offset
            int initialOffset = (int)data.Position;

            // Create a new cart image to fill
            var cart = new Cart();

            #region NCSD Header

            // Try to parse the header
            var header = ParseNCSDHeader(data);
            if (header == null)
                return null;

            // Set the cart image header
            cart.Header = header;

            #endregion

            #region Card Info Header

            // Try to parse the card info header
            var cardInfoHeader = ParseCardInfoHeader(data);
            if (cardInfoHeader == null)
                return null;

            // Set the card info header
            cart.CardInfoHeader = cardInfoHeader;

            #endregion

            #region Development Card Info Header

            // Try to parse the development card info header
            var developmentCardInfoHeader = ParseDevelopmentCardInfoHeader(data);
            if (developmentCardInfoHeader == null)
                return null;

            // Set the development card info header
            cart.DevelopmentCardInfoHeader = developmentCardInfoHeader;

            #endregion

            #region Partitions

            // Create the partition table
            cart.Partitions = new NCCHHeader[8];

            // Iterate and build the partitions
            for (int i = 0; i < 8; i++)
            {
                cart.Partitions[i] = ParseNCCHHeader(data);
            }

            #endregion

            // Cache the media unit size for further use
            long mediaUnitSize = 0;
            if (header.PartitionFlags != null)
                mediaUnitSize = (uint)(0x200 * Math.Pow(2, header.PartitionFlags[(int)NCSDFlags.MediaUnitSize]));

            #region Extended Headers

            // Create the extended header table
            cart.ExtendedHeaders = new NCCHExtendedHeader[8];

            // Iterate and build the extended headers
            for (int i = 0; i < 8; i++)
            {
                // If we have an encrypted or invalid partition
                if (cart.Partitions[i]!.MagicID != NCCHMagicNumber)
                    continue;

                // If we have no partitions table
                if (cart.Header!.PartitionsTable == null)
                    continue;

                // Get the extended header offset
                long offset = (cart.Header.PartitionsTable[i]!.Offset * mediaUnitSize) + 0x200;
                if (offset < 0 || offset >= data.Length)
                    continue;

                // Seek to the extended header
                data.Seek(offset, SeekOrigin.Begin);

                // Parse the extended header
                var extendedHeader = ParseNCCHExtendedHeader(data);
                if (extendedHeader != null)
                    cart.ExtendedHeaders[i] = extendedHeader;
            }

            #endregion

            #region ExeFS Headers

            // Create the ExeFS header table
            cart.ExeFSHeaders = new ExeFSHeader[8];

            // Iterate and build the ExeFS headers
            for (int i = 0; i < 8; i++)
            {
                // If we have an encrypted or invalid partition
                if (cart.Partitions[i]!.MagicID != NCCHMagicNumber)
                    continue;

                // If we have no partitions table
                if (cart.Header!.PartitionsTable == null)
                    continue;

                // Get the ExeFS header offset
                long offset = (cart.Header.PartitionsTable[i]!.Offset + cart.Partitions[i]!.ExeFSOffsetInMediaUnits) * mediaUnitSize;
                if (offset < 0 || offset >= data.Length)
                    continue;

                // Seek to the ExeFS header
                data.Seek(offset, SeekOrigin.Begin);

                // Parse the ExeFS header
                cart.ExeFSHeaders[i] = ParseExeFSHeader(data);
            }

            #endregion

            #region RomFS Headers

            // Create the RomFS header table
            cart.RomFSHeaders = new RomFSHeader[8];

            // Iterate and build the RomFS headers
            for (int i = 0; i < 8; i++)
            {
                // If we have an encrypted or invalid partition
                if (cart.Partitions[i]!.MagicID != NCCHMagicNumber)
                    continue;

                // If we have no partitions table
                if (cart.Header!.PartitionsTable == null)
                    continue;

                // Get the RomFS header offset
                long offset = (cart.Header.PartitionsTable[i]!.Offset + cart.Partitions[i]!.RomFSOffsetInMediaUnits) * mediaUnitSize;
                if (offset < 0 || offset >= data.Length)
                    continue;

                // Seek to the RomFS header
                data.Seek(offset, SeekOrigin.Begin);

                // Parse the RomFS header
                var romFsHeader = ParseRomFSHeader(data);
                if (romFsHeader != null)
                    cart.RomFSHeaders[i] = romFsHeader;
            }

            #endregion

            return cart;
        }

        /// <summary>
        /// Parse a Stream into an NCSD header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled NCSD header on success, null on error</returns>
        private static NCSDHeader? ParseNCSDHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            NCSDHeader header = new NCSDHeader();

            header.RSA2048Signature = data.ReadBytes(0x100);
            byte[]? magicNumber = data.ReadBytes(4);
            if (magicNumber == null)
                return null;

            header.MagicNumber = Encoding.ASCII.GetString(magicNumber).TrimEnd('\0'); ;
            if (header.MagicNumber != NCSDMagicNumber)
                return null;

            header.ImageSizeInMediaUnits = data.ReadUInt32();
            header.MediaId = data.ReadBytes(8);
            header.PartitionsFSType = (FilesystemType)data.ReadUInt64();
            header.PartitionsCryptType = data.ReadBytes(8);

            header.PartitionsTable = new PartitionTableEntry[8];
            for (int i = 0; i < 8; i++)
            {
                header.PartitionsTable[i] = ParsePartitionTableEntry(data);
            }

            if (header.PartitionsFSType == FilesystemType.Normal || header.PartitionsFSType == FilesystemType.None)
            {
                header.ExheaderHash = data.ReadBytes(0x20);
                header.AdditionalHeaderSize = data.ReadUInt32();
                header.SectorZeroOffset = data.ReadUInt32();
                header.PartitionFlags = data.ReadBytes(8);

                header.PartitionIdTable = new ulong[8];
                for (int i = 0; i < 8; i++)
                {
                    header.PartitionIdTable[i] = data.ReadUInt64();
                }

                header.Reserved1 = data.ReadBytes(0x20);
                header.Reserved2 = data.ReadBytes(0x0E);
                header.FirmUpdateByte1 = data.ReadByteValue();
                header.FirmUpdateByte2 = data.ReadByteValue();
            }
            else if (header.PartitionsFSType == FilesystemType.FIRM)
            {
                header.Unknown = data.ReadBytes(0x5E);
                header.EncryptedMBR = data.ReadBytes(0x42);
            }

            return header;
        }

        /// <summary>
        /// Parse a Stream into a partition table entry
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled partition table entry on success, null on error</returns>
        private static PartitionTableEntry ParsePartitionTableEntry(Stream data)
        {
            // TODO: Use marshalling here instead of building
            PartitionTableEntry partitionTableEntry = new PartitionTableEntry();

            partitionTableEntry.Offset = data.ReadUInt32();
            partitionTableEntry.Length = data.ReadUInt32();

            return partitionTableEntry;
        }

        /// <summary>
        /// Parse a Stream into a card info header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled card info header on success, null on error</returns>
        private static CardInfoHeader ParseCardInfoHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            CardInfoHeader cardInfoHeader = new CardInfoHeader();

            cardInfoHeader.WritableAddressMediaUnits = data.ReadUInt32();
            cardInfoHeader.CardInfoBitmask = data.ReadUInt32();
            cardInfoHeader.Reserved1 = data.ReadBytes(0xF8);
            cardInfoHeader.FilledSize = data.ReadUInt32();
            cardInfoHeader.Reserved2 = data.ReadBytes(0x0C);
            cardInfoHeader.TitleVersion = data.ReadUInt16();
            cardInfoHeader.CardRevision = data.ReadUInt16();
            cardInfoHeader.Reserved3 = data.ReadBytes(0x0C);
            cardInfoHeader.CVerTitleID = data.ReadBytes(8);
            cardInfoHeader.CVerVersionNumber = data.ReadUInt16();
            cardInfoHeader.Reserved4 = data.ReadBytes(0xCD6);

            return cardInfoHeader;
        }

        /// <summary>
        /// Parse a Stream into a development card info header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled development card info header on success, null on error</returns>
        private static DevelopmentCardInfoHeader? ParseDevelopmentCardInfoHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            DevelopmentCardInfoHeader developmentCardInfoHeader = new DevelopmentCardInfoHeader();

            developmentCardInfoHeader.InitialData = ParseInitialData(data);
            if (developmentCardInfoHeader.InitialData == null)
                return null;

            developmentCardInfoHeader.CardDeviceReserved1 = data.ReadBytes(0x200);
            developmentCardInfoHeader.TitleKey = data.ReadBytes(0x10);
            developmentCardInfoHeader.CardDeviceReserved2 = data.ReadBytes(0x1BF0);

            developmentCardInfoHeader.TestData = ParseTestData(data);
            if (developmentCardInfoHeader.TestData == null)
                return null;

            return developmentCardInfoHeader;
        }

        /// <summary>
        /// Parse a Stream into an initial data
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled initial data on success, null on error</returns>
        private static InitialData? ParseInitialData(Stream data)
        {
            // TODO: Use marshalling here instead of building
            InitialData initialData = new InitialData();

            initialData.CardSeedKeyY = data.ReadBytes(0x10);
            initialData.EncryptedCardSeed = data.ReadBytes(0x10);
            initialData.CardSeedAESMAC = data.ReadBytes(0x10);
            initialData.CardSeedNonce = data.ReadBytes(0xC);
            initialData.Reserved = data.ReadBytes(0xC4);
            initialData.BackupHeader = ParseNCCHHeader(data, true);
            if (initialData.BackupHeader == null)
                return null;

            return initialData;
        }

        /// <summary>
        /// Parse a Stream into an NCCH header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <param name="skipSignature">Indicates if the signature should be skipped</param>
        /// <returns>Filled NCCH header on success, null on error</returns>
        internal static NCCHHeader ParseNCCHHeader(Stream data, bool skipSignature = false)
        {
            // TODO: Use marshalling here instead of building
            NCCHHeader header = new NCCHHeader();

            if (!skipSignature)
                header.RSA2048Signature = data.ReadBytes(0x100);

            byte[]? magicId = data.ReadBytes(4);
            if (magicId != null)
                header.MagicID = Encoding.ASCII.GetString(magicId).TrimEnd('\0');
            header.ContentSizeInMediaUnits = data.ReadUInt32();
            header.PartitionId = data.ReadUInt64();
            header.MakerCode = data.ReadUInt16();
            header.Version = data.ReadUInt16();
            header.VerificationHash = data.ReadUInt32();
            header.ProgramId = data.ReadBytes(8);
            header.Reserved1 = data.ReadBytes(0x10);
            header.LogoRegionHash = data.ReadBytes(0x20);
            byte[]? productCode = data.ReadBytes(0x10);
            if (productCode != null)
                header.ProductCode = Encoding.ASCII.GetString(productCode).TrimEnd('\0');
            header.ExtendedHeaderHash = data.ReadBytes(0x20);
            header.ExtendedHeaderSizeInBytes = data.ReadUInt32();
            header.Reserved2 = data.ReadBytes(4);
            header.Flags = ParseNCCHHeaderFlags(data);
            header.PlainRegionOffsetInMediaUnits = data.ReadUInt32();
            header.PlainRegionSizeInMediaUnits = data.ReadUInt32();
            header.LogoRegionOffsetInMediaUnits = data.ReadUInt32();
            header.LogoRegionSizeInMediaUnits = data.ReadUInt32();
            header.ExeFSOffsetInMediaUnits = data.ReadUInt32();
            header.ExeFSSizeInMediaUnits = data.ReadUInt32();
            header.ExeFSHashRegionSizeInMediaUnits = data.ReadUInt32();
            header.Reserved3 = data.ReadBytes(4);
            header.RomFSOffsetInMediaUnits = data.ReadUInt32();
            header.RomFSSizeInMediaUnits = data.ReadUInt32();
            header.RomFSHashRegionSizeInMediaUnits = data.ReadUInt32();
            header.Reserved4 = data.ReadBytes(4);
            header.ExeFSSuperblockHash = data.ReadBytes(0x20);
            header.RomFSSuperblockHash = data.ReadBytes(0x20);

            return header;
        }

        /// <summary>
        /// Parse a Stream into an NCCH header flags
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled NCCH header flags on success, null on error</returns>
        private static NCCHHeaderFlags ParseNCCHHeaderFlags(Stream data)
        {
            // TODO: Use marshalling here instead of building
            NCCHHeaderFlags headerFlags = new NCCHHeaderFlags();

            headerFlags.Reserved0 = data.ReadByteValue();
            headerFlags.Reserved1 = data.ReadByteValue();
            headerFlags.Reserved2 = data.ReadByteValue();
            headerFlags.CryptoMethod = (CryptoMethod)data.ReadByteValue();
            headerFlags.ContentPlatform = (ContentPlatform)data.ReadByteValue();
            headerFlags.MediaPlatformIndex = (ContentType)data.ReadByteValue();
            headerFlags.ContentUnitSize = data.ReadByteValue();
            headerFlags.BitMasks = (BitMasks)data.ReadByteValue();

            return headerFlags;
        }

        /// <summary>
        /// Parse a Stream into an initial data
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled initial data on success, null on error</returns>
        private static TestData ParseTestData(Stream data)
        {
            // TODO: Use marshalling here instead of building
            TestData testData = new TestData();

            // TODO: Validate some of the values
            testData.Signature = data.ReadBytes(8);
            testData.AscendingByteSequence = data.ReadBytes(0x1F8);
            testData.DescendingByteSequence = data.ReadBytes(0x200);
            testData.Filled00 = data.ReadBytes(0x200);
            testData.FilledFF = data.ReadBytes(0x200);
            testData.Filled0F = data.ReadBytes(0x200);
            testData.FilledF0 = data.ReadBytes(0x200);
            testData.Filled55 = data.ReadBytes(0x200);
            testData.FilledAA = data.ReadBytes(0x1FF);
            testData.FinalByte = data.ReadByteValue();

            return testData;
        }

        /// <summary>
        /// Parse a Stream into an NCCH extended header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled NCCH extended header on success, null on error</returns>
        private static NCCHExtendedHeader? ParseNCCHExtendedHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            NCCHExtendedHeader extendedHeader = new NCCHExtendedHeader();

            extendedHeader.SCI = ParseSystemControlInfo(data);
            if (extendedHeader.SCI == null)
                return null;

            extendedHeader.ACI = ParseAccessControlInfo(data);
            if (extendedHeader.ACI == null)
                return null;

            extendedHeader.AccessDescSignature = data.ReadBytes(0x100);
            extendedHeader.NCCHHDRPublicKey = data.ReadBytes(0x100);

            extendedHeader.ACIForLimitations = ParseAccessControlInfo(data);
            if (extendedHeader.ACI == null)
                return null;

            return extendedHeader;
        }

        /// <summary>
        /// Parse a Stream into a system control info
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled system control info on success, null on error</returns>
        private static SystemControlInfo ParseSystemControlInfo(Stream data)
        {
            // TODO: Use marshalling here instead of building
            SystemControlInfo systemControlInfo = new SystemControlInfo();

            byte[]? applicationTitle = data.ReadBytes(8);
            if (applicationTitle != null)
                systemControlInfo.ApplicationTitle = Encoding.ASCII.GetString(applicationTitle).TrimEnd('\0');
            systemControlInfo.Reserved1 = data.ReadBytes(5);
            systemControlInfo.Flag = data.ReadByteValue();
            systemControlInfo.RemasterVersion = data.ReadUInt16();
            systemControlInfo.TextCodeSetInfo = ParseCodeSetInfo(data);
            systemControlInfo.StackSize = data.ReadUInt32();
            systemControlInfo.ReadOnlyCodeSetInfo = ParseCodeSetInfo(data);
            systemControlInfo.Reserved2 = data.ReadBytes(4);
            systemControlInfo.DataCodeSetInfo = ParseCodeSetInfo(data);
            systemControlInfo.BSSSize = data.ReadUInt32();
            systemControlInfo.DependencyModuleList = new ulong[48];
            for (int i = 0; i < 48; i++)
            {
                systemControlInfo.DependencyModuleList[i] = data.ReadUInt64();
            }
            systemControlInfo.SystemInfo = ParseSystemInfo(data);

            return systemControlInfo;
        }

        /// <summary>
        /// Parse a Stream into a code set info
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled code set info on success, null on error</returns>
        private static CodeSetInfo ParseCodeSetInfo(Stream data)
        {
            // TODO: Use marshalling here instead of building
            CodeSetInfo codeSetInfo = new CodeSetInfo();

            codeSetInfo.Address = data.ReadUInt32();
            codeSetInfo.PhysicalRegionSizeInPages = data.ReadUInt32();
            codeSetInfo.SizeInBytes = data.ReadUInt32();

            return codeSetInfo;
        }

        /// <summary>
        /// Parse a Stream into a system info
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled system info on success, null on error</returns>
        private static SystemInfo ParseSystemInfo(Stream data)
        {
            // TODO: Use marshalling here instead of building
            SystemInfo systemInfo = new SystemInfo();

            systemInfo.SaveDataSize = data.ReadUInt64();
            systemInfo.JumpID = data.ReadUInt64();
            systemInfo.Reserved = data.ReadBytes(0x30);

            return systemInfo;
        }

        /// <summary>
        /// Parse a Stream into an access control info
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled access control info on success, null on error</returns>
        private static AccessControlInfo ParseAccessControlInfo(Stream data)
        {
            // TODO: Use marshalling here instead of building
            AccessControlInfo accessControlInfo = new AccessControlInfo();

            accessControlInfo.ARM11LocalSystemCapabilities = ParseARM11LocalSystemCapabilities(data);
            accessControlInfo.ARM11KernelCapabilities = ParseARM11KernelCapabilities(data);
            accessControlInfo.ARM9AccessControl = ParseARM9AccessControl(data);

            return accessControlInfo;
        }

        /// <summary>
        /// Parse a Stream into an ARM11 local system capabilities
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled ARM11 local system capabilities on success, null on error</returns>
        private static ARM11LocalSystemCapabilities ParseARM11LocalSystemCapabilities(Stream data)
        {
            // TODO: Use marshalling here instead of building
            ARM11LocalSystemCapabilities arm11LocalSystemCapabilities = new ARM11LocalSystemCapabilities();

            arm11LocalSystemCapabilities.ProgramID = data.ReadUInt64();
            arm11LocalSystemCapabilities.CoreVersion = data.ReadUInt32();
            arm11LocalSystemCapabilities.Flag1 = (ARM11LSCFlag1)data.ReadByteValue();
            arm11LocalSystemCapabilities.Flag2 = (ARM11LSCFlag2)data.ReadByteValue();
            arm11LocalSystemCapabilities.Flag0 = (ARM11LSCFlag0)data.ReadByteValue();
            arm11LocalSystemCapabilities.Priority = data.ReadByteValue();
            arm11LocalSystemCapabilities.ResourceLimitDescriptors = new ushort[16];
            for (int i = 0; i < 16; i++)
            {
                arm11LocalSystemCapabilities.ResourceLimitDescriptors[i] = data.ReadUInt16();
            }
            arm11LocalSystemCapabilities.StorageInfo = ParseStorageInfo(data);
            arm11LocalSystemCapabilities.ServiceAccessControl = new ulong[32];
            for (int i = 0; i < 32; i++)
            {
                arm11LocalSystemCapabilities.ServiceAccessControl[i] = data.ReadUInt64();
            }
            arm11LocalSystemCapabilities.ExtendedServiceAccessControl = new ulong[2];
            for (int i = 0; i < 2; i++)
            {
                arm11LocalSystemCapabilities.ExtendedServiceAccessControl[i] = data.ReadUInt64();
            }
            arm11LocalSystemCapabilities.Reserved = data.ReadBytes(0x0F);
            arm11LocalSystemCapabilities.ResourceLimitCategory = (ResourceLimitCategory)data.ReadByteValue();

            return arm11LocalSystemCapabilities;
        }

        /// <summary>
        /// Parse a Stream into a storage info
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled storage info on success, null on error</returns>
        private static StorageInfo ParseStorageInfo(Stream data)
        {
            // TODO: Use marshalling here instead of building
            StorageInfo storageInfo = new StorageInfo();

            storageInfo.ExtdataID = data.ReadUInt64();
            storageInfo.SystemSavedataIDs = data.ReadBytes(8);
            storageInfo.StorageAccessibleUniqueIDs = data.ReadBytes(8);
            storageInfo.FileSystemAccessInfo = data.ReadBytes(7);
            storageInfo.OtherAttributes = (StorageInfoOtherAttributes)data.ReadByteValue();

            return storageInfo;
        }

        /// <summary>
        /// Parse a Stream into an ARM11 kernel capabilities
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled ARM11 kernel capabilities on success, null on error</returns>
        private static ARM11KernelCapabilities ParseARM11KernelCapabilities(Stream data)
        {
            // TODO: Use marshalling here instead of building
            ARM11KernelCapabilities arm11KernelCapabilities = new ARM11KernelCapabilities();

            arm11KernelCapabilities.Descriptors = new uint[28];
            for (int i = 0; i < 28; i++)
            {
                arm11KernelCapabilities.Descriptors[i] = data.ReadUInt32();
            }
            arm11KernelCapabilities.Reserved = data.ReadBytes(0x10);

            return arm11KernelCapabilities;
        }

        /// <summary>
        /// Parse a Stream into an ARM11 access control
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled ARM11 access control on success, null on error</returns>
        private static ARM9AccessControl ParseARM9AccessControl(Stream data)
        {
            // TODO: Use marshalling here instead of building
            ARM9AccessControl arm9AccessControl = new ARM9AccessControl();

            arm9AccessControl.Descriptors = data.ReadBytes(15);
            arm9AccessControl.DescriptorVersion = data.ReadByteValue();

            return arm9AccessControl;
        }

        /// <summary>
        /// Parse a Stream into an ExeFS header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled ExeFS header on success, null on error</returns>
        private static ExeFSHeader ParseExeFSHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            ExeFSHeader exeFSHeader = new ExeFSHeader();

            exeFSHeader.FileHeaders = new ExeFSFileHeader[10];
            for (int i = 0; i < 10; i++)
            {
                exeFSHeader.FileHeaders[i] = ParseExeFSFileHeader(data);
            }
            exeFSHeader.Reserved = data.ReadBytes(0x20);
            exeFSHeader.FileHashes = new byte[10][];
            for (int i = 0; i < 10; i++)
            {
                exeFSHeader.FileHashes[i] = data.ReadBytes(0x20) ?? [];
            }

            return exeFSHeader;
        }

        /// <summary>
        /// Parse a Stream into an ExeFS file header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled ExeFS file header on success, null on error</returns>
        private static ExeFSFileHeader ParseExeFSFileHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            ExeFSFileHeader exeFSFileHeader = new ExeFSFileHeader();

            byte[]? fileName = data.ReadBytes(8);
            if (fileName != null)
                exeFSFileHeader.FileName = Encoding.ASCII.GetString(fileName).TrimEnd('\0');
            exeFSFileHeader.FileOffset = data.ReadUInt32();
            exeFSFileHeader.FileSize = data.ReadUInt32();

            return exeFSFileHeader;
        }

        /// <summary>
        /// Parse a Stream into an RomFS header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled RomFS header on success, null on error</returns>
        private static RomFSHeader? ParseRomFSHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            RomFSHeader romFSHeader = new RomFSHeader();

            byte[]? magicString = data.ReadBytes(4);
            if (magicString == null)
                return null;

            romFSHeader.MagicString = Encoding.ASCII.GetString(magicString).TrimEnd('\0');
            if (romFSHeader.MagicString != RomFSMagicNumber)
                return null;

            romFSHeader.MagicNumber = data.ReadUInt32();
            if (romFSHeader.MagicNumber != RomFSSecondMagicNumber)
                return null;

            romFSHeader.MasterHashSize = data.ReadUInt32();
            romFSHeader.Level1LogicalOffset = data.ReadUInt64();
            romFSHeader.Level1HashdataSize = data.ReadUInt64();
            romFSHeader.Level1BlockSizeLog2 = data.ReadUInt32();
            romFSHeader.Reserved1 = data.ReadBytes(4);
            romFSHeader.Level2LogicalOffset = data.ReadUInt64();
            romFSHeader.Level2HashdataSize = data.ReadUInt64();
            romFSHeader.Level2BlockSizeLog2 = data.ReadUInt32();
            romFSHeader.Reserved2 = data.ReadBytes(4);
            romFSHeader.Level3LogicalOffset = data.ReadUInt64();
            romFSHeader.Level3HashdataSize = data.ReadUInt64();
            romFSHeader.Level3BlockSizeLog2 = data.ReadUInt32();
            romFSHeader.Reserved3 = data.ReadBytes(4);
            romFSHeader.Reserved4 = data.ReadBytes(4);
            romFSHeader.OptionalInfoSize = data.ReadUInt32();

            return romFSHeader;
        }
    }
}
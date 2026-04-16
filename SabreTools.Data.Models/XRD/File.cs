namespace SabreTools.Data.Models.XRD
{
    public class File
    {
        /// <summary>
        /// "XRD\xFF\x00"
        /// </summary>
        public byte[] Magic { get; set; } = new byte[5];

        /// <summary>
        /// XRD Version
        /// Currently 0x01
        /// </summary>
        public byte Version { get; set; }

        /// <summary>
        /// XGD Type: XGD1, XGD2, XGD3, 0xFF = Unknown
        /// </summary>
        public byte XGDType { get; set; }

        /// <summary>
        /// XGD SubType
        /// XGD1:
        ///  subtype 0 = XGD1 Beta (XB00104M)
        ///  subtype 1 = Standard
        /// XGD2:
        ///  subtype 0 = "Wave 0" (3CFB91D5)
        ///  subtype 1-20 = Wave 1-20, Standard
        ///  subtype 0x81 = Hybrid XGD2 / DVD-Video (65472451)
        /// XGD3:
        ///  subtype 0 = XGD3 Beta (152C2978, FD91511A, FFFFFDEB, FFFFFDE3)
        ///  subtype 1 = Standard
        /// 0xFF = Unknown
        /// Others undefined
        /// </summary>
        public byte XGDSubtype { get; set; }

        /// <summary>
        /// 8-character serial in disc ringcode
        /// e.g. XGD1: e.g. "MS00101A"
        /// e.g. XGD2/3: e.g. "1A2B3C4D"
        /// </summary>
        public byte[] Ringcode { get; set; } = new byte[8];

        /// <summary>
        /// Size of the redump ISO in bytes
        /// </summary>
        /// <remarks>Little-endian</remarks>
        public ulong RedumpSize { get; set; }

        /// <summary>
        /// CRC-32 hash of the redump ISO
        /// </summary>
        public byte[] RedumpCRC { get; set; } = new byte[4];

        /// <summary>
        /// MD5 hash of the redump ISO
        /// </summary>
        public byte[] RedumpMD5 { get; set; } = new byte[16];

        /// <summary>
        /// SHA-1 hash of the redump ISO
        /// </summary>
        public byte[] RedumpSHA1 { get; set; } = new byte[20];

        /// <summary>
        /// Size of the Raw XISO in bytes
        /// </summary>
        /// <remarks>Little-endian</remarks>
        public ulong RawXISOSize { get; set; }

        /// <summary>
        /// CRC-32 hash of the Raw XISO
        /// </summary>
        public byte[] RawXISOCRC { get; set; } = new byte[4];

        /// <summary>
        /// MD5 hash of the Raw XISO
        /// </summary>
        public byte[] RawXISOMD5 { get; set; } = new byte[16];

        /// <summary>
        /// SHA-1 hash of the Raw XISO
        /// </summary>
        public byte[] RawXISOSHA1 { get; set; } = new byte[20];

        /// <summary>
        /// Size of the Wiped/Trimmed XISO in bytes
        /// </summary>
        /// <remarks>Little-endian</remarks>
        public ulong CookedXISOSize { get; set; }

        /// <summary>
        /// CRC-32 hash of the Wiped/Trimmed XISO
        /// </summary>
        public byte[] CookedXISOCRC { get; set; } = new byte[4];

        /// <summary>
        /// MD5 hash of the Wiped/Trimmed XISO
        /// </summary>
        public byte[] CookedXISOMD5 { get; set; } = new byte[16];

        /// <summary>
        /// SHA-1 hash of the Wiped/Trimmed XISO
        /// </summary>
        public byte[] CookedXISOSHA1 { get; set; } = new byte[20];

        /// <summary>
        /// Size of the Video ISO in bytes
        /// </summary>
        /// <remarks>Little-endian</remarks>
        public ulong VideoISOSize { get; set; }

        /// <summary>
        /// CRC-32 hash of the Video ISO
        /// </summary>
        public byte[] VideoISOCRC { get; set; } = new byte[4];

        /// <summary>
        /// MD5 hash of the Video ISO
        /// </summary>
        public byte[] VideoISOMD5 { get; set; } = new byte[16];

        /// <summary>
        /// SHA-1 hash of the Video ISO
        /// </summary>
        public byte[] VideoISOSHA1 { get; set; } = new byte[20];

        /// <summary>
        /// CRC-32 Hash of the first sector of the XDVDFS filesystem (filler data)
        /// The hash is used to identify filler data or brute force the seed
        /// </summary>
        public byte[] FillerCRC { get; set; } = new byte[4];

        /// <summary>
        /// MD5 Hash of the first sector of the XDVDFS filesystem (filler data)
        /// The hash is used to identify filler data or brute force the seed
        /// </summary>
        public byte[] FillerMD5 { get; set; } = new byte[16];

        /// <summary>
        /// SHA-1 Hash of the first sector of the XDVDFS filesystem (filler data)
        /// The hash is used to identify filler data or brute force the seed
        /// </summary>
        public byte[] FillerSHA1 { get; set; } = new byte[20];

        /// <summary>
        /// The starting sector offset for each security sector range
        /// Security sector ranges are 4096-sectors long
        /// XGD2/3: 2 sector offsets
        /// Default/XGD1: 16 sector offsets
        public uint[] SecuritySectors { get; set; } = [];

        /// <summary>
        /// SHA-1 Hash of the Video ISO with system update file zeroed
        /// </summary>
        /// <remarks>XGD3 only, 20 bytes</remarks>
        public byte[]? CookedVideoISOSHA1 { get; set; }

        /// <summary>
        /// SHA-1 Hash of the su20076000_00000000 file in the Video ISO
        /// </summary>
        /// <remarks>XGD3 only, 20 bytes</remarks>
        public byte[]? SystemUpdateHash { get; set; }

        /// <summary>
        /// XBE Certificate, starts with length of structure
        /// </summary>
        /// <remarks>XGD1 only, Little-endian</remarks>
        public XboxExecutable.Certificate? XboxCertificate { get; set; }

        /// <summary>
        /// XEX Certificate, starts with length of structure
        /// </summary>
        /// <remarks>XGD2/3 only, Big-endian</remarks>
        public XenonExecutable.Certificate? Xbox360Certificate { get; set; }

        /// <summary>
        /// Number of files in XDVDFS filesystem
        /// </summary>
        /// <remarks>Little-endian</remarks>
        public ulong FileCount { get; set; }

        /// <summary>
        /// File offsets and hashes
        /// Length of array equal to FileCount
        /// </summary>
        public FileInfo[] FileInfo { get; set; } = [];

        /// <summary>
        /// XDVDFS Volume Descriptor
        /// </summary>
        /// <remarks>2048 bytes</remarks>
        public XDVDFS.VolumeDescriptor VolumeDescriptor { get; set; } = new();

        /// <summary>
        /// Xbox DVD Layout Descriptor, immediately follows Volume Descriptor
        /// XGD1: Contains version numbers and signature bytes
        /// XGD2: Zeroed apart from initial signature bytes
        /// XGD3: Sector not present
        /// </summary>
        /// <remarks>2048 bytes</remarks>
        public XDVDFS.LayoutDescriptor? LayoutDescriptor { get; set; }

        /// <summary>
        /// List of descriptors and their sector offsets and sizes
        /// The root directory descriptor is not guaranteed to be the first
        /// </summary>
        public DirectoryInfo[] DirectoryInfo { get; set; } = [];
    }
}

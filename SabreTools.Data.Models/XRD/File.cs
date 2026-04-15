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
        /// XGD1 subtype 1 = Standard
        /// XGD2 subtype = Wave 0-20
        /// XGD2 subtype 
        /// XGD3 subtype 0 = XGD3 Beta
        /// XGD3 subtype 1 = Standard
        /// 0xFF = Unknown
        /// </summary>
        public byte XGDSubtype { get; set; }

        /// <summary>
        /// 8-character serial in disc ringcode
        /// e.g. XGD1: MS00101A
        /// e.g. XGD2/3: 1A2B3C4D
        /// </summary>
        public byte Ringcode { get; set; } = new byte[8];

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

        // XBE / XEX metadata goes here

        /// <summary>
        /// XDVDFS Volume Descriptor
        /// </summary>
        /// <remarks>2048 bytes</remarks>
        public VolumeDescriptor VolumeDescriptor { get; set; } = new();

        /// <summary>
        /// Xbox DVD Layout Descriptor, immediately follows Volume Descriptor
        /// XGD1: Contains version numbers and signature bytes
        /// XGD2: Zeroed apart from initial signature bytes
        /// XGD3: Sector not present
        /// </summary>
        /// <remarks>2048 bytes</remarks>
        public LayoutDescriptor? LayoutDescriptor { get; set; }

        /// <summary>
        /// Map of sector offsets, sizes, and the directory descriptor at that sector number
        /// The root directory descriptor is not guaranteed to be the earliest
        /// </summary>
        public List<(uint, uint, DirectoryDescriptor)> DirectoryDescriptors { get; set; } = [];
    }
}

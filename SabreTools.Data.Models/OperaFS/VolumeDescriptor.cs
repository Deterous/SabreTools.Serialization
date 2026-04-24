namespace SabreTools.Data.Models.OperaFS
{
    /// <summary>
    /// OperaFS Volume Descriptor, first sector of filesystem
    /// </summary>
    /// <see href="https://groups.google.com/g/rec.games.video.3do/c/1U3qrmLSYMQ"/>
    public class VolumeDescriptor
    {
        /// <summary>
        /// Should be 0x01
        /// </summary>
        public byte RecordType { get; set; }

        /// <summary>
        /// "ZZZZZ"
        /// </summary>
        public byte[] VolumeSyncBytes { get; set; } = new byte[5];

        /// <summary>
        /// Should be 0x0100
        /// </summary>
        public ushort StructureVersion { get; set; }

        /// <summary>
        /// Should be 0x00 for all 3DO discs
        /// Is used by M2 discs?
        /// </summary>
        public VolumeFlags VolumeFlags { get; set; }

        /// <summary>
        /// Usually zeroed
        /// </summary>
        public byte[] VolumeCommentary { get; set; } = new byte[32];

        /// <summary>
        /// ASCII "CD-ROM"
        /// </summary>
        public byte[] VolumeIdentifier { get; set; } = new byte[32];

        /// <summary>
        /// Hash or just a random value to identify disc
        /// </summary>
        public uint VolumeUniqueIdentifier { get; set; }

        /// <summary>
        /// Sector size in volume
        /// Usually 0x800 (2048)
        /// </summary>
        public uint VolumeBlockSize { get; set; }

        /// <summary>
        /// Number of sectors in volume
        /// Usually size of disc image minus 300
        /// </summary>
        public uint VolumeBlockCount { get; set; }

        /// <summary>
        /// Hash or just a random value to identify root directory
        /// </summary>
        public uint RootUniqueIdentifier { get; set; }

        /// <summary>
        /// Number of sectors for root directory
        /// Usually 0x01
        /// </summary>
        public uint RootDirectoryBlockCount { get; set; }

        /// <summary>
        /// Sector size in root directory
        /// Usually 0x800
        /// </summary>
        public uint RootDirectoryBlockSize { get; set; }

        /// <summary>
        /// Number of duplicates of the root directory provided
        /// Should be between 0 and 7
        /// </summary>
        public uint RootDirectoryLastAvatarIndex { get; set; }

        /// <summary>
        /// Array of 8 offsets pointing to the root directory
        /// Contents of each root directory should be identical
        /// If RootDirectoryLastAvatarIndex is less than 7, remaining values are zeroed
        /// </summary>
        public uint[] RootDirectoryAvatarList { get; set; } = new uint[8];

        /// <summary>
        /// "iamaduck" repeated, aligned to each QWORD (0x0 or 0x8)
        /// i.e. if padding starts at offset ending in 0x4 or 0xC, then it begins with "duck"
        /// </summary>
        public byte[] Padding { get; set; } = new byte[0x77C];
    }
}

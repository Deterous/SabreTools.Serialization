namespace SabreTools.Data.Models.OperaFS
{
    /// <summary>
    /// OperaFS Directory Record
    public class DirectoryRecord
    {
        /// <summary>
        /// Flags about this directory record
        /// </summary>
        public DirectoryRecordFlags DirectoryRecordFlags { get; set; } = new();

        /// <summary>
        /// Hash or random value to identify this record
        /// </summary>
        public byte[] RecordID { get; set; } = new byte[4];

        /// <summary>
        /// Type of record
        /// </summary>
        public byte[] RecordType { get; set; } = new byte[4];

        /// <summary>
        /// Sector size for this record
        /// Should be 0x800
        /// </summary>
        public uint BlockSize { get; set; }

        /// <summary>
        /// Number of bytes in this record
        /// </summary>
        public uint ByteCount { get; set; }

        /// <summary>
        /// Number of blocks allocated to this record
        /// </summary>
        public uint BlockCount { get; set; }

        /// <summary>
        /// Burst
        /// </summary>
        public uint Burst { get; set; }

        /// <summary>
        /// Gap
        /// </summary>
        public uint Gap { get; set; }

        /// <summary>
        /// Filename of record
        /// </summary>
        public byte[] Filename { get; set; } = new byte[32];
    }
}

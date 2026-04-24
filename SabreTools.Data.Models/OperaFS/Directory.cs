namespace SabreTools.Data.Models.OperaFS
{
    /// <summary>
    /// OperaFS Directory
    public class Directory
    {
        /// <summary>
        /// 0xFFFFFFFF for the root directory
        /// </summary>
        public int NextBlock { get; set; }

        /// <summary>
        /// 0xFFFFFFFF for the root directory
        /// </summary>
        public int PreviousBlock { get; set; }

        /// <summary>
        /// Should be zeroed
        /// </summary>
        public int DirectoryFlags { get; set; }

        /// <summary>
        /// First free byte
        /// </summary>
        public int FirstFreeByteOffset { get; set; }

        /// <summary>
        /// First entry offset
        /// </summary>
        public int FirstEntryOffset { get; set; }

        /// <summary>
        /// Flags about this directory record
        /// </summary>
        public DirectoryRecordFlags DirectoryRecordFlags { get; set; } = new();

        /// <summary>
        /// Directory records in this directory
        /// </summary>
        public DirectoryRecord[] DirectoryRecords { get; set; } = [];
    }
}

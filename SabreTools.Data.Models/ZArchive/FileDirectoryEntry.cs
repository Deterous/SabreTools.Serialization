namespace SabreTools.Data.Models.ZArchive
{
    /// <summary>
    /// Node in the FileTree
    /// </summary>
    /// <see href="https://github.com/Exzap/ZArchive/"/>
    public class FileDirectoryEntry
    {
        /// <summary>
        /// MSB is the type flag, 0 is Directory, 1 is File
        /// Remaining 31 bits are the offset in the NameTable
        /// </summary>
        public uint NameOffsetAndTypeFlag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint FileOffsetLow { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint FileSizeLow { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ushort FileSizeHigh { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ushort FileOffsetHigh { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint NodeStartIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint Count { get; set; }

        /// <summary>
        /// Reserved field
        /// </summary>
        public uint Reserved { get; set; }
    }
}

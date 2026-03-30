namespace SabreTools.Data.Models.ZArchive
{
    /// <summary>
    /// Represents a single ZAR archive
    /// </summary>
    /// <see href="https://github.com/Exzap/ZArchive/"/>
    public class Archive
    {
        /// <summary>
        /// Local file entries, Zstd compressed blocks
        /// A block is stored uncompressed if Zstd does not decrease the size
        /// </summary>
        public LocalFile[]? LocalFiles { get; set; }

        /// <summary>
        /// Padding bytes to be added after compressed blocks to ensure 8-byte alignment
        /// Padding bytes are all NULL (0x00)
        /// </summary>
        public byte[]? Padding { get; set; } = [];

        /// <summary>
        /// Records containing the offsets and block sizes of each group of blocks
        /// This allows the reader to jump to any 65536-byte boundary in the uncompressed stream.
        /// </summary>
        public CompressionOffsetRecord[]? CompressionOffsetRecords { get; set; }

        /// <summary>
        /// UTF-8 strings, prepended by string lengths
        /// </summary>
        public NameTable NameTable { get; set; } = new();

        /// <summary>
        /// Serialized file tree structure using a queue of nodes
        /// </summary>
        public FileDirectoryEntry[] FileTree { get; set; } = [];

        /// <summary>
        /// Section for custom key-value pairs and properties
        /// </summary>
        public Metadata? Metadata { get; set; }

        /// <summary>
        /// Archive footer containing the offsets and sizes of all other sections
        /// Ends with a SHA256 hash/size of the entire archive, and magic bytes
        /// </summary>
        public Footer Footer { get; set; } = new();
    }
}

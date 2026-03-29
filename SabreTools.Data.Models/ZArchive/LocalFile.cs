namespace SabreTools.Data.Models.ZArchive
{
    /// <summary>
    /// ZArchive compressed file data
    /// </summary>
    /// <see href="https://github.com/Exzap/ZArchive/"/>
    public class LocalFile
    {
        /// <summary>
        /// Zstd compressed file data, in 65536 byte blocks of the original file
        /// Blocks are stored uncompressed if ZStd does not decrease the size
        /// </summary>
        public byte[] FileData { get; set; } = [];
    }
}

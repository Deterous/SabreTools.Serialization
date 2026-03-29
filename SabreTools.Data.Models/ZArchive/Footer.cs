namespace SabreTools.Data.Models.ZArchive
{
    /// <summary>
    /// Footer data stored at the end of a ZArchive file
    /// </summary>
    /// <see href="https://github.com/Exzap/ZArchive/"/>
    public class Footer
    {
        /// <summary>
        /// SHA-256 hash of the ZArchive file prior the footer
        /// </summary>
        public byte[] SHA256 { get; set; } = new byte[32];

        /// <summary>
        /// Size of the entire ZArchive file
        /// </summary>
        /// <remarks>Big-endian</remarks>
        public ulong Size { get; set; }

        /// <summary>
        /// Version indicator, also acts as extended magic
        /// </summary>
        public ulong Version { get; set; }

        /// <summary>
        /// Magic bytes to indicate ZArchive file
        /// </summary>
        public ulong Magic { get; set; }
    }
}

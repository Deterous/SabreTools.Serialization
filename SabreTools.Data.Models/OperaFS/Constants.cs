namespace SabreTools.Data.Models.OperaFS
{
    /// <summary>
    /// OperaFS constant values and arrays
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Standard block size for OperaFS disc images
        /// </summary>
        public static readonly int SectorSize = 2048;

        /// <summary>
        /// Start of a standard OperaFS image
        /// </summary>
        public static readonly byte[] MagicBytes = [0x01, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x01];
    }
}

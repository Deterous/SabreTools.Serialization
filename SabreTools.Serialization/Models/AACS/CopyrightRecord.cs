using System.Runtime.InteropServices;

namespace SabreTools.Serialization.Models.AACS
{
    /// <summary>
    /// This record type is undocumented but found in real media key blocks
    /// </summary>
    public sealed class CopyrightRecord : Record
    {
        /// <summary>
        /// Null-terminated ASCII string representing the copyright
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string? Copyright;
    }
}
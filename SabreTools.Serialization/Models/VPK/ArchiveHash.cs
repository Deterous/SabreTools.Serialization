using System.Runtime.InteropServices;

namespace SabreTools.Data.Models.VPK
{
    /// <see href="https://github.com/RavuAlHemio/hllib/blob/master/HLLib/VPKFile.h"/>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class ArchiveHash
    {
        public uint ArchiveIndex;

        public uint ArchiveOffset;

        public uint Length;

        /// <summary>
        /// MD5
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[]? Hash;
    }
}

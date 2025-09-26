using System.Runtime.InteropServices;

namespace SabreTools.Serialization.Models.N3DS
{
    /// <summary>
    /// The test data is the same one encountered in development DS/DSi cartridges.
    /// </summary>
    /// <see href="https://www.3dbrew.org/wiki/NCSD#TestData"/>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class TestData
    {
        /// <summary>
        /// The bytes FF 00 FF 00 AA 55 AA 55.
        /// </summary>
        /// <remarks>8 bytes</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[]? Signature;

        /// <summary>
        /// An ascending byte sequence equal to the offset mod 256 (08 09 0A ... FE FF 00 01 ... FF).
        /// </summary>
        /// <remarks>0x1F8 bytes</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1F8)]
        public byte[]? AscendingByteSequence;

        /// <summary>
        /// A descending byte sequence equal to 255 minus the offset mod 256 (FF FE FD ... 00 FF DE ... 00).
        /// </summary>
        /// <remarks>0x200 bytes</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x200)]
        public byte[]? DescendingByteSequence;

        /// <summary>
        /// Filled with 00 (0b00000000) bytes.
        /// </summary>
        /// <remarks>0x200 bytes</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x200)]
        public byte[]? Filled00;

        /// <summary>
        /// Filled with FF (0b11111111) bytes.
        /// </summary>
        /// <remarks>0x200 bytes</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x200)]
        public byte[]? FilledFF;

        /// <summary>
        /// Filled with 0F (0b00001111) bytes.
        /// </summary>
        /// <remarks>0x200 bytes</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x200)]
        public byte[]? Filled0F;

        /// <summary>
        /// Filled with F0 (0b11110000) bytes.
        /// </summary>
        /// <remarks>0x200 bytes</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x200)]
        public byte[]? FilledF0;

        /// <summary>
        /// Filled with 55 (0b01010101) bytes.
        /// </summary>
        /// <remarks>0x200 bytes</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x200)]
        public byte[]? Filled55;

        /// <summary>
        /// Filled with AA (0b10101010) bytes.
        /// </summary>
        /// <remarks>0x1FF bytes</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1FF)]
        public byte[]? FilledAA;

        /// <summary>
        /// The final byte is 00 (0b00000000).
        /// </summary>
        public byte FinalByte;
    }
}

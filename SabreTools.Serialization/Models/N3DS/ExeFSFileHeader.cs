﻿using System.Runtime.InteropServices;

namespace SabreTools.Data.Models.N3DS
{
    /// <summary>
    /// There are a maximum of 10 file headers in the ExeFS format. (This maximum
    /// number of file headers is disputable, with makerom indicating a maximum of
    /// 8 sections and makecia indicating a maximum of 10. From a non-SDK point of
    /// view, the ExeFS header format can hold no more than 10 file headers within
    /// the currently define size of 0x200 bytes.)
    /// </summary>
    /// <see href="https://www.3dbrew.org/wiki/ExeFS#File_headers"/>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public sealed class ExeFSFileHeader
    {
        /// <summary>
        /// File name
        /// </summary>
        /// <remarks>8 bytes</remarks>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string? FileName;

        /// <summary>
        /// File offset
        /// </summary>
        public uint FileOffset;

        /// <summary>
        /// File size
        /// </summary>
        public uint FileSize;
    }
}

﻿using System.Runtime.InteropServices;

namespace SabreTools.Serialization.Models.N3DS
{
    /// <summary>
    /// Used in FSReg:Register.
    /// </summary>
    /// <see href="https://www.3dbrew.org/wiki/NCCH/Extended_Header#Storage_Info"/>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class StorageInfo
    {
        /// <summary>
        /// Extdata ID
        /// </summary>
        public ulong ExtdataID;

        /// <summary>
        /// System savedata IDs
        /// </summary>
        /// <remarks>8 bytes</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[]? SystemSavedataIDs;

        /// <summary>
        /// Storage accessible unique IDs
        /// </summary>
        /// <remarks>8 bytes</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[]? StorageAccessibleUniqueIDs;

        /// <summary>
        /// Filesystem access info
        /// </summary>
        /// TODO: Create enum for the flag values
        /// TODO: Combine with "other attributes"
        /// <remarks>7 bytes</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[]? FileSystemAccessInfo;

        /// <summary>
        /// Other attributes
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public StorageInfoOtherAttributes OtherAttributes;
    }
}

﻿using System;

namespace SabreTools.Serialization.Models.MicrosoftCabinet
{
    public enum CompressionType : ushort
    {
        /// <summary>
        /// Mask for compression type.
        /// </summary>
        MASK_TYPE = 0x000F,

        /// <summary>
        /// No compression.
        /// </summary>
        TYPE_NONE = 0x0000,

        /// <summary>
        /// MSZIP compression.
        /// </summary>
        TYPE_MSZIP = 0x0001,

        /// <summary>
        /// Quantum compression.
        /// </summary>
        TYPE_QUANTUM = 0x0002,

        /// <summary>
        /// LZX compression.
        /// </summary>
        TYPE_LZX = 0x0003,
    }

    [Flags]
    public enum FileAttributes : ushort
    {
        /// <summary>
        /// File is read-only.
        /// </summary>
        RDONLY = 0x0001,

        /// <summary>
        /// File is hidden.
        /// </summary>
        HIDDEN = 0x0002,

        /// <summary>
        /// File is a system file.
        /// </summary>
        SYSTEM = 0x0004,

        /// <summary>
        /// File has been modified since last backup.
        /// </summary>
        ARCH = 0x0040,

        /// <summary>
        /// File will be run after extraction.
        /// </summary>
        EXEC = 0x0080,

        /// <summary>
        /// The szName field contains UTF.
        /// </summary>
        NAME_IS_UTF = 0x0100,
    }

    public enum FolderIndex : ushort
    {
        /// <summary>
        /// A value of zero indicates that this is the
        /// first folder in this cabinet file.
        /// </summary>
        FIRST_FOLDER = 0x0000,

        /// <summary>
        /// Indicates that the folder index is actually zero, but that
        /// extraction of this file would have to begin with the cabinet named in the
        /// CFHEADER.szCabinetPrev field. 
        /// </summary>
        CONTINUED_FROM_PREV = 0xFFFD,

        /// <summary>
        /// Indicates that the folder index
        /// is actually one less than THE CFHEADER.cFolders field value, and that extraction of this file will
        /// require continuation to the cabinet named in the CFHEADER.szCabinetNext field.
        /// </summary>
        CONTINUED_TO_NEXT = 0xFFFE,

        /// <see cref="CONTINUED_FROM_PREV"/>
        /// <see cref="CONTINUED_TO_NEXT"/>
        CONTINUED_PREV_AND_NEXT = 0xFFFF,
    }

    [Flags]
    public enum HeaderFlags : ushort
    {
        /// <summary>
        /// The flag is set if this cabinet file is not the first in a set of cabinet files.
        /// When this bit is set, the szCabinetPrev and szDiskPrev fields are present in this CFHEADER
        /// structure. The value is 0x0001.
        /// </summary>
        PREV_CABINET = 0x0001,

        /// <summary>
        /// The flag is set if this cabinet file is not the last in a set of cabinet files.
        /// When this bit is set, the szCabinetNext and szDiskNext fields are present in this CFHEADER
        /// structure. The value is 0x0002.
        /// </summary>
        NEXT_CABINET = 0x0002,

        /// <summary>
        /// The flag is set if if this cabinet file contains any reserved fields. When
        /// this bit is set, the cbCFHeader, cbCFFolder, and cbCFData fields are present in this CFHEADER
        /// structure. The value is 0x0004.
        /// </summary>
        RESERVE_PRESENT = 0x0004,
    }
}

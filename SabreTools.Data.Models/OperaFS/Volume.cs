using System.Collections.Generic;

namespace SabreTools.Data.Models.OperaFS
{
    /// <summary>
    /// Opera Filesystem (or user-data-only disc image) present on 3DO game discs
    /// Usually contained within a CDROM disc image (2352-byte bin file)
    /// All fields are Big-Endian
    /// </summary>
    /// <see href=""/>
    public sealed class FileSystem
    {
        /// <summary>
        /// Volume Descriptor
        /// </summary>
        public VolumeDescriptor VolumeDescriptor { get; set; } = new();

        /// <summary>
        /// List of all directories in filesystem
        /// </summary>
        public Directory[] Directories { get; set; } = [];
    }
}

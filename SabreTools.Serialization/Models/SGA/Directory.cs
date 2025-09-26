using System.Collections.Generic;

namespace SabreTools.Data.Models.SGA
{
    /// <see href="https://github.com/RavuAlHemio/hllib/blob/master/HLLib/SGAFile.h"/>
    public abstract class Directory
    {
        /// <summary>
        /// Source SGA file
        /// </summary>
        public Archive? File { get; set; }
    }

    /// <summary>
    /// Specialization File7 and up where the CRC moved to the header and the CRC is of the compressed data and there are stronger hashes.
    /// </summary>
    /// <see href="https://github.com/RavuAlHemio/hllib/blob/master/HLLib/SGAFile.h"/>
    public class Directory<THeader, TDirectoryHeader, TSection, TFolder, TFile, U> : Directory
        where THeader : Header
        where TDirectoryHeader : DirectoryHeader<U>
        where TSection : Section<U>
        where TFolder : Folder<U>
        where TFile : File
    {
        /// <summary>
        /// Directory header data
        /// </summary>
        public TDirectoryHeader? DirectoryHeader { get; set; }

        /// <summary>
        /// Sections data
        /// </summary>
        public TSection[]? Sections { get; set; }

        /// <summary>
        /// Folders data
        /// </summary>
        public TFolder[]? Folders { get; set; }

        /// <summary>
        /// Files data
        /// </summary>
        public TFile[]? Files { get; set; }

        /// <summary>
        /// String table data
        /// </summary>
        public Dictionary<long, string?>? StringTable { get; set; }
    }
}

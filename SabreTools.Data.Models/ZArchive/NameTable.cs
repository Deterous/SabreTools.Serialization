namespace SabreTools.Data.Models.ZArchive
{
    /// <summary>
    /// UTF-8 strings, prepended by string lengths
    /// </summary>
    /// <see href="https://github.com/Exzap/ZArchive/"/>
    public class NameTable
    {
        /// <summary>
        /// List of filename entries
        /// </summary>
        public NameEntry[] NameEntries { get; set; } = [];
    }
}

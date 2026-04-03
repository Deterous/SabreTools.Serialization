namespace SabreTools.Data.Models.XenonExecutable
{
    /// <summary>
    /// Xenon (Xbox 360) Executable format optional header
    /// This is a flexible metadata list of data types and either their value or a pointer to their value
    /// </summary>
    /// <see href="http://oskarsapps.mine.nu/xexdump"/>
    /// <see href="https://free60.org/System-Software/Formats/XEX/"/>
    public class OptionalHeader
    {
        /// <summary>
        /// Header type identifier
        /// Known ID values are stored in Constants.OptionalHeaderTypes
        /// </summary>
        /// <remarks>Big-endian</remarks>
        public uint HeaderID { get; set; }

        /// <summary>
        /// If lowest byte of HeaderID is 0x00/0x01, then HeaderData is the data itself
        /// Otherwise, HeaderData is the data offset into XEX file
        /// </summary>
        /// <remarks>Big-endian</remarks>
        public uint HeaderData { get; set; }

        /// <summary>
        /// If HeaderData is a data offset, then HeaderDataBytes is variable-length data it points to
        /// the meaning and structure of these bytes is dependent on the HeaderID value
        /// If HeaderData is the data itself, then this field is null
        /// </summary>
        public byte[]? HeaderDataBytes { get; set; } = [];
    }
}

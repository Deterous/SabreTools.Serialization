namespace SabreTools.Data.Models.XenonExecutable
{
    /// <summary>
    /// Xenon (Xbox 360) Executable format
    /// It is based on PPC PE format, and therefore Big-endian
    /// </summary>
    /// <see href="http://oskarsapps.mine.nu/xexdump"/>
    /// <see href="https://free60.org/System-Software/Formats/XEX/"/>
    public class Executable
    {
        /// <summary>
        /// XEX header
        /// </summary>
        public Header Header { get; set; } = new();

        /// <summary>
        /// XEX certificate structure
        /// </summary>
        public Certificate Certificate { get; set; } = new();

        // PE data goes here, is an encrypted/compressed blob on most XEX files
    }
}

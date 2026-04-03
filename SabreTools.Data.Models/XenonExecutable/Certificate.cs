namespace SabreTools.Data.Models.XenonExecutable
{
    /// <summary>
    /// Xenon (Xbox 360) Executable format certificate structure
    /// </summary>
    /// <see href="http://oskarsapps.mine.nu/xexdump"/>
    /// <see href="https://free60.org/System-Software/Formats/XEX/"/>
    public class Certificate
    {
        /// <summary>
        /// Length of the certificate structure in bytes
        /// </summary>
        /// <remarks>Big-endian</remarks>
        public uint Length { get; set; }
    }
}

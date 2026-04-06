namespace SabreTools.Data.Models.XenonExecutable
{
    /// <summary>
    /// Xenon (Xbox 360) Executable format certificate table
    /// </summary>
    /// <see href="http://oskarsapps.mine.nu/xexdump"/>
    /// <see href="https://free60.org/System-Software/Formats/XEX/"/>
    public class Executable
    {
        /// <summary>
        /// Table entry ID
        /// Known values:
        /// 0x00000011, 0x00000012, 0x00000013 (Retail games)
        /// 0x00000101, 0x00000102, 0x00000103 (Applications)
        /// </summary>
        /// <remarks>Big-endian</remarks>
        public uint ID { get; set; } = new();

        /// <summary>
        /// Table entry data, 20 bytes
        /// </summary>
        public byte[]? Data { get; set; } = new byte[20];
    }
}

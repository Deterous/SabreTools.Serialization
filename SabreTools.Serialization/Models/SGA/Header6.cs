namespace SabreTools.Serialization.Models.SGA
{
    /// <see href="https://github.com/RavuAlHemio/hllib/blob/master/HLLib/SGAFile.h"/>
    public sealed class Header6 : Header
    {
        public string? Name { get; set; }

        public uint HeaderLength { get; set; }

        public uint FileDataOffset { get; set; }

        public uint Dummy0 { get; set; }
    }
}

namespace SabreTools.Data.Models.MicrosoftCabinet
{
    public static class Constants
    {
        public static readonly byte[] SignatureBytes = [0x4d, 0x53, 0x43, 0x46];

        public const string SignatureString = "MSCF";

        public const uint SignatureUInt32 = 0x4643534d;
    }
}
namespace SabreTools.Serialization.Models.Quantum
{
    public static class Constants
    {
        public static readonly byte[] SignatureBytes = [0x44, 0x53];

        public const string SignatureString = "DS";

        public const ushort SignatureUInt16 = 0x5344;
    }
}
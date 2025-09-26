namespace SabreTools.Serialization.Models.NewExecutable
{
    public static class Constants
    {
        public static readonly byte[] SignatureBytes = [0x4e, 0x45];

        public const string SignatureString = "NE";

        public const ushort SignatureUInt16 = 0x454e;
    }
}
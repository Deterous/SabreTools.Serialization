using System.Runtime.InteropServices;

namespace SabreTools.Serialization.Models.DVD
{
    /// <see href="https://dvd.sourceforge.net/dvdinfo/ifo.html"/>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class CellAddressTableEntry
    {
        /// <summary>
        /// VOBidn
        /// </summary>
        public ushort VOBIdentity;

        /// <summary>
        /// CELLidn
        /// </summary>
        public byte CellIdentity;

        /// <summary>
        /// Reserved
        /// </summary>
        public byte Reserved;

        /// <summary>
        /// Starting sector within VOB
        /// </summary>
        public uint StartingSectorWithinVOB;

        /// <summary>
        /// Ending sector within VOB
        /// </summary>
        public uint EndingSectorWithinVOB;
    }
}
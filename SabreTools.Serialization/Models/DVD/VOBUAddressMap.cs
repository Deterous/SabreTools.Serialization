namespace SabreTools.Serialization.Models.DVD
{
    /// <see href="https://dvd.sourceforge.net/dvdinfo/ifo.html"/>
    public sealed class VOBUAddressMap
    {
        /// <summary>
        /// End address (last byte of last entry)
        /// </summary>
        public uint EndAddress { get; set; }

        /// <summary>
        /// Starting sector within VOB of nth VOBU
        /// </summary>
        public uint[]? StartingSectors { get; set; }
    }
}
namespace SabreTools.Serialization.Models.BSP
{
    /// <see href="https://developer.valvesoftware.com/wiki/BSP_(GoldSrc)"/> 
    /// <see href="https://developer.valvesoftware.com/wiki/BSP_(Source)"/>
    public sealed class EdgesLump : Lump
    {
        /// <summary>
        /// Edge
        /// </summary>
        public Edge[]? Edges { get; set; }
    }
}
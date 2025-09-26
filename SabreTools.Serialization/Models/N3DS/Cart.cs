namespace SabreTools.Data.Models.N3DS
{
    /// <summary>
    /// Represents a 3DS cart image
    /// </summary>
    public class Cart
    {
        /// <summary>
        /// 3DS cart header
        /// </summary>
        public NCSDHeader? Header { get; set; }

        /// <summary>
        /// 3DS card info header
        /// </summary>
        public CardInfoHeader? CardInfoHeader { get; set; }

        /// <summary>
        /// 3DS development card info header
        /// </summary>
        public DevelopmentCardInfoHeader? DevelopmentCardInfoHeader { get; set; }

        /// <summary>
        /// NCCH partitions
        /// </summary>
        public NCCHHeader[]? Partitions { get; set; }

        /// <summary>
        /// NCCH extended headers
        /// </summary>
        public NCCHExtendedHeader[]? ExtendedHeaders { get; set; }

        /// <summary>
        /// ExeFS headers associated with each partition
        /// </summary>
        public ExeFSHeader[]? ExeFSHeaders { get; set; }

        /// <summary>
        /// RomFS headers associated with each partition
        /// </summary>
        public RomFSHeader[]? RomFSHeaders { get; set; }
    }
}
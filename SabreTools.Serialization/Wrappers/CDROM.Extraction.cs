namespace SabreTools.Serialization.Wrappers
{
    public partial class CDROM : IExtractable
    {
        /// <inheritdoc/>
        public virtual bool Extract(string outputDirectory, bool includeDebug)
        {
            if (iso != null)
                iso.Extract(outputDirectory, includeDebug);
        }
    }
}

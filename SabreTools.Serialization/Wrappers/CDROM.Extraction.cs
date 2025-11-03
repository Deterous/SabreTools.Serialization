namespace SabreTools.Serialization.Wrappers
{
    public partial class CDROM : IExtractable
    {
        /// <inheritdoc/>
        public virtual bool Extract(string outputDirectory, bool includeDebug)
        {
            bool success = true;

            if (FileSystem != null)
                success &= FileSystem.Extract(outputDirectory, includeDebug);

            return success;
        }
    }
}

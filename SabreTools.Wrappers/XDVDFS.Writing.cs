using System;
using System.IO;
using SabreTools.Data.Models.XDVDFS;
using SabreTools.Numerics.Extensions;

namespace SabreTools.Wrappers
{
    public partial class XDVDFS : IWritable
    {
        /// <inheritdoc/>
        public bool Write(string outputDirectory, bool includeDebug)
        {
            // Get the base path
            string outputFilename = Filename is null
                ? Guid.NewGuid().ToString()
                : Path.GetFileName(Filename);
            outputFilename += ".xiso";
            string outputPath = Path.Combine(outputDirectory, outputFilename);

            Stream? stream = new Serialization.Writers.XDVDFS().SerializeStream(Model);

            // Check for invalid data
            if (stream is null)
            {
                if (includeDebug) Console.WriteLine("Model was invalid, cannot write!");
                return false;
            }

            // Open the output file for writing
            using var fs = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);

            stream.CopyTo(fs);

            return true;
        }
    }
}

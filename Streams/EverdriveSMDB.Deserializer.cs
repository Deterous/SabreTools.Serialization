using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SabreTools.IO.Readers;
using SabreTools.Models.EverdriveSMDB;
using SabreTools.Serialization.Interfaces;

namespace SabreTools.Serialization.Streams
{
    public partial class EverdriveSMDB : IStreamSerializer<MetadataFile>
    {
        /// <inheritdoc/>
        public MetadataFile? Deserialize(Stream? data)
        {
            // If the stream is null
            if (data == null)
                return default;

            // Setup the reader and output
            var reader = new SeparatedValueReader(data, Encoding.UTF8)
            {
                Header = false,
                Separator = '\t',
                VerifyFieldCount = false,
            };
            var dat = new MetadataFile();

            // Loop through the rows and parse out values
            var rows = new List<Row>();
            while (!reader.EndOfStream)
            {
                // If we have no next line
                if (!reader.ReadNextLine() || reader.Line == null)
                    break;

                // Parse the line into a row
                var row = new Row
                {
                    SHA256 = reader.Line[0],
                    Name = reader.Line[1],
                    SHA1 = reader.Line[2],
                    MD5 = reader.Line[3],
                    CRC32 = reader.Line[4],
                };

                // If we have the size field
                if (reader.Line.Count > 5)
                    row.Size = reader.Line[5];

                // If we have additional fields
                if (reader.Line.Count > 6)
                    row.ADDITIONAL_ELEMENTS = reader.Line.Skip(5).ToArray();

                rows.Add(row);
            }

            // Assign the rows to the Dat and return
            dat.Row = rows.ToArray();
            return dat;
        }
    }
}
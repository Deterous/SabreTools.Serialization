using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SabreTools.IO.Readers;
using SabreTools.Models.RomCenter;
using SabreTools.Serialization.Interfaces;

namespace SabreTools.Serialization.Deserializers
{
    public class RomCenter :
        IByteDeserializer<MetadataFile>,
        IFileDeserializer<MetadataFile>,
        IStreamDeserializer<MetadataFile>
    {
        #region IByteDeserializer

        /// <inheritdoc cref="IByteDeserializer.Deserialize(byte[]?, int)"/>
        public static MetadataFile? DeserializeBytes(byte[]? data, int offset)
        {
            var deserializer = new RomCenter();
            return deserializer.Deserialize(data, offset);
        }

        /// <inheritdoc/>
        public MetadataFile? Deserialize(byte[]? data, int offset)
        {
            // If the data is invalid
            if (data == null)
                return null;

            // If the offset is out of bounds
            if (offset < 0 || offset >= data.Length)
                return null;

            // Create a memory stream and parse that
            var dataStream = new MemoryStream(data, offset, data.Length - offset);
            return DeserializeStream(dataStream);
        }

        #endregion

        #region IFileDeserializer

        /// <inheritdoc cref="IFileDeserializer.Deserialize(string?)"/>
        public static MetadataFile? DeserializeFile(string? path)
        {
            var deserializer = new RomCenter();
            return deserializer.Deserialize(path);
        }

        /// <inheritdoc/>
        public MetadataFile? Deserialize(string? path)
        {
            using var stream = PathProcessor.OpenStream(path);
            return DeserializeStream(stream);
        }

        #endregion

        #region IStreamDeserializer

        /// <inheritdoc cref="IStreamDeserializer.Deserialize(Stream?)"/>
        public static MetadataFile? DeserializeStream(Stream? data)
        {
            var deserializer = new RomCenter();
            return deserializer.Deserialize(data);
        }
        
        /// <inheritdoc/>
        public MetadataFile? Deserialize(Stream? data)
        {
            // If the stream is null
            if (data == null)
                return default;

            // Setup the reader and output
            var reader = new IniReader(data, Encoding.UTF8)
            {
                ValidateRows = false,
            };
            var dat = new MetadataFile();

            // Loop through and parse out the values
            var roms = new List<Rom>();
            var additional = new List<string>();
            var creditsAdditional = new List<string>();
            var datAdditional = new List<string>();
            var emulatorAdditional = new List<string>();
            var gamesAdditional = new List<string>();
            while (!reader.EndOfStream)
            {
                // If we have no next line
                if (!reader.ReadNextLine())
                    break;

                // Ignore certain row types
                switch (reader.RowType)
                {
                    case IniRowType.None:
                    case IniRowType.Comment:
                        continue;
                    case IniRowType.SectionHeader:
                        switch (reader.Section?.ToLowerInvariant())
                        {
                            case "credits":
                                dat.Credits ??= new Credits();
                                break;
                            case "dat":
                                dat.Dat ??= new Dat();
                                break;
                            case "emulator":
                                dat.Emulator ??= new Emulator();
                                break;
                            case "games":
                                dat.Games ??= new Games();
                                break;
                            default:
                                if (reader.CurrentLine != null)
                                    additional.Add(reader.CurrentLine);
                                break;
                        }
                        continue;
                }

                // If we're in credits
                if (reader.Section?.ToLowerInvariant() == "credits")
                {
                    // Create the section if we haven't already
                    dat.Credits ??= new Credits();

                    switch (reader.KeyValuePair?.Key?.ToLowerInvariant())
                    {
                        case "author":
                            dat.Credits.Author = reader.KeyValuePair?.Value;
                            break;
                        case "version":
                            dat.Credits.Version = reader.KeyValuePair?.Value;
                            break;
                        case "email":
                            dat.Credits.Email = reader.KeyValuePair?.Value;
                            break;
                        case "homepage":
                            dat.Credits.Homepage = reader.KeyValuePair?.Value;
                            break;
                        case "url":
                            dat.Credits.Url = reader.KeyValuePair?.Value;
                            break;
                        case "date":
                            dat.Credits.Date = reader.KeyValuePair?.Value;
                            break;
                        case "comment":
                            dat.Credits.Comment = reader.KeyValuePair?.Value;
                            break;
                        default:
                            if (reader.CurrentLine != null)
                                creditsAdditional.Add(reader.CurrentLine);
                            break;
                    }
                }

                // If we're in dat
                else if (reader.Section?.ToLowerInvariant() == "dat")
                {
                    // Create the section if we haven't already
                    dat.Dat ??= new Dat();

                    switch (reader.KeyValuePair?.Key?.ToLowerInvariant())
                    {
                        case "version":
                            dat.Dat.Version = reader.KeyValuePair?.Value;
                            break;
                        case "plugin":
                            dat.Dat.Plugin = reader.KeyValuePair?.Value;
                            break;
                        case "split":
                            dat.Dat.Split = reader.KeyValuePair?.Value;
                            break;
                        case "merge":
                            dat.Dat.Merge = reader.KeyValuePair?.Value;
                            break;
                        default:
                            if (reader.CurrentLine != null)
                                datAdditional.Add(reader.CurrentLine);
                            break;
                    }
                }

                // If we're in emulator
                else if (reader.Section?.ToLowerInvariant() == "emulator")
                {
                    // Create the section if we haven't already
                    dat.Emulator ??= new Emulator();

                    switch (reader.KeyValuePair?.Key?.ToLowerInvariant())
                    {
                        case "refname":
                            dat.Emulator.RefName = reader.KeyValuePair?.Value;
                            break;
                        case "version":
                            dat.Emulator.Version = reader.KeyValuePair?.Value;
                            break;
                        default:
                            if (reader.CurrentLine != null)
                                emulatorAdditional.Add(reader.CurrentLine);
                            break;
                    }
                }

                // If we're in games
                else if (reader.Section?.ToLowerInvariant() == "games")
                {
                    // Create the section if we haven't already
                    dat.Games ??= new Games();

                    // If the line doesn't contain the delimiter
                    if (!(reader.CurrentLine?.Contains('¬') ?? false))
                    {
                        if (reader.CurrentLine != null)
                            gamesAdditional.Add(reader.CurrentLine);

                        continue;
                    }

                    // Otherwise, separate out the line
                    string[] splitLine = reader.CurrentLine.Split('¬');
                    var rom = new Rom
                    {
                        // EMPTY = splitLine[0]
                        ParentName = splitLine[1],
                        ParentDescription = splitLine[2],
                        GameName = splitLine[3],
                        GameDescription = splitLine[4],
                        RomName = splitLine[5],
                        RomCRC = splitLine[6],
                        RomSize = splitLine[7],
                        RomOf = splitLine[8],
                        MergeName = splitLine[9],
                        // EMPTY = splitLine[10]
                    };

                    if (splitLine.Length > 11)
                        rom.ADDITIONAL_ELEMENTS = splitLine.Skip(11).ToArray();

                    roms.Add(rom);
                }

                else
                {
                    if (reader.CurrentLine != null)
                        additional.Add(reader.CurrentLine);
                }
            }

            // Add extra pieces and return
            dat.ADDITIONAL_ELEMENTS = additional.Where(s => s != null).ToArray();
            if (dat.Credits != null)
                dat.Credits.ADDITIONAL_ELEMENTS = creditsAdditional.Where(s => s != null).ToArray();
            if (dat.Dat != null)
                dat.Dat.ADDITIONAL_ELEMENTS = datAdditional.Where(s => s != null).ToArray();
            if (dat.Emulator != null)
                dat.Emulator.ADDITIONAL_ELEMENTS = emulatorAdditional.Where(s => s != null).ToArray();
            if (dat.Games != null)
            {
                dat.Games.Rom = roms.ToArray();
                dat.Games.ADDITIONAL_ELEMENTS = gamesAdditional.Where(s => s != null).Select(s => s).ToArray();
            }
            return dat;
        }

        #endregion
    }
}
using System.IO;
using System.Text;
using SabreTools.IO;
using SabreTools.Models.VBSP;
using SabreTools.Serialization.Interfaces;
using static SabreTools.Models.VBSP.Constants;

namespace SabreTools.Serialization.Deserializers
{
    public class VBSP :
        BaseBinaryDeserializer<Models.VBSP.File>,
        IStreamDeserializer<Models.VBSP.File>
    {
        #region IStreamDeserializer

        /// <inheritdoc/>
        public Models.VBSP.File? Deserialize(Stream? data)
        {
            // If the data is invalid
            if (data == null || data.Length == 0 || !data.CanSeek || !data.CanRead)
                return null;

            // If the offset is out of bounds
            if (data.Position < 0 || data.Position >= data.Length)
                return null;

            // Cache the current offset
            long initialOffset = data.Position;

            // Create a new Half-Life 2 Level to fill
            var file = new Models.VBSP.File();

            #region Header

            // Try to parse the header
            var header = ParseHeader(data);
            if (header == null)
                return null;

            // Set the package header
            file.Header = header;

            #endregion

            return file;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life 2 Level header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Half-Life 2 Level header on success, null on error</returns>
        private static Header? ParseHeader(Stream data)
        {
            // TODO: Use marshalling here instead of building
            Header header = new Header();

            byte[]? signature = data.ReadBytes(4);
            if (signature == null)
                return null;

            header.Signature = Encoding.ASCII.GetString(signature);
            if (header.Signature != SignatureString)
                return null;

            header.Version = data.ReadInt32();
            if ((header.Version < 19 || header.Version > 22) && header.Version != 0x00040014)
                return null;

            header.Lumps = new Lump[HL_VBSP_LUMP_COUNT];
            for (int i = 0; i < HL_VBSP_LUMP_COUNT; i++)
            {
                header.Lumps[i] = ParseLump(data, header.Version);
            }

            header.MapRevision = data.ReadInt32();

            return header;
        }

        /// <summary>
        /// Parse a Stream into a Half-Life 2 Level lump
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <param name="version">VBSP version</param>
        /// <returns>Filled Half-Life 2 Level lump on success, null on error</returns>
        private static Lump ParseLump(Stream data, int version)
        {
            // TODO: Use marshalling here instead of building
            Lump lump = new Lump();

            lump.Offset = data.ReadUInt32();
            lump.Length = data.ReadUInt32();
            lump.Version = data.ReadUInt32();
            lump.FourCC = new char[4];
            for (int i = 0; i < 4; i++)
            {
                lump.FourCC[i] = (char)data.ReadByte();
            }

            // This block was commented out because test VBSPs with header
            // version 21 had the values in the "right" order already and
            // were causing decompression issues

            //if (version >= 21 && version != 0x00040014)
            //{
            //    uint temp = lump.Version;
            //    lump.Version = lump.Offset;
            //    lump.Offset = lump.Length;
            //    lump.Length = temp;
            //}

            return lump;
        }

        #endregion
    }
}
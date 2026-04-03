using System.IO;
using SabreTools.Data.Models.XenonExecutable;
using SabreTools.IO.Extensions;
using SabreTools.Matching;
using SabreTools.Numerics.Extensions;
using static SabreTools.Data.Models.XenonExecutable.Constants;

namespace SabreTools.Serialization.Readers
{
    public class XenonExecutable : BaseBinaryReader<Executable>
    {
        /// <inheritdoc/>
        public override Executable? Deserialize(Stream? data)
        {
            // If the data is invalid
            if (data is null || !data.CanRead)
                return null;

            try
            {
                // Cache the current offset
                long initialOffset = data.Position;

                // Create a new executable to fill
                var xex = new Executable();

                #region ParseHeader

                // Parse the file header
                var header = ParseHeader(data);
                if (header is null)
                    return null;

                // Set the XEX header
                xex.Header = header;

                #endregion

                #region ParseCertificate

                // Parse the file header
                var certificate = ParseCertificate(data);
                if (certificate is null)
                    return null;

                // Set the XEX certificate
                xex.Certificate = certificate;

                #endregion

                return xex;
            }
            catch
            {
                // Ignore the actual error
                return null;
            }
        }

        /// <summary>
        /// Parse a Stream into an XEX Header
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled XEX Header on success, null on error</returns>
        public static Header? ParseHeader(Stream data)
        {
            var obj = new Header();

            var magicNumber = data.ReadBytes(4);
            if (!magicNumber.EqualsExactly(Constants.MagicBytes))
                return null;

            obj.MagicNumber = magicNumber; 

            obj.ModuleFlags = data.ReadUInt32BigEndian();
            obj.PEDataOffset = data.ReadUInt32BigEndian();
            obj.Reserved = data.ReadUInt32BigEndian();
            obj.CertificateOffset = data.ReadUInt32BigEndian();
            obj.OptionalHeaderCount = data.ReadUInt32BigEndian();

            // TODO: Check data stream is long enough for all optional headers

            var optionalHeaders = new OptionalHeader[obj.OptionalHeaderCount];
            for (int i = 0; i < obj.OptionalHeaderCount; i++)
            {
                var optionalHeader = new OptionalHeader();
                optionalHeader.HeaderID = data.ReadUInt32BigEndian();
                optionalHeader.HeaderData = data.ReadUInt32BigEndian();

                // TODO: Fill in HeaderDataBytes

                optionalHeaders[i] = optionalHeader;
            }

            obj.OptionalHeaders = optionalHeaders;

            return obj;
        }

        /// <summary>
        /// Parse a Stream into an XEX Certificate
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled XEX Certificate on success, null on error</returns>
        public static Certificate? ParseCertificate(Stream data)
        {
            var obj = new Certificate();

            obj.Length = data.ReadUInt32BigEndian();

            return obj;
        }
    }
}

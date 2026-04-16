using System.Collections.Generic;
using System.IO;
using SabreTools.Data.Models.XRD;
using SabreTools.IO.Extensions;
using SabreTools.Matching;
using SabreTools.Numerics.Extensions;

namespace SabreTools.Serialization.Readers
{
    public class XRD : BaseBinaryReader<File>
    {
        /// <inheritdoc/>
        public override File? Deserialize(Stream? data)
        {
            // If the data is invalid
            if (data is null || !data.CanRead)
                return null;

            // Simple check for a valid stream length
            if (2312 > data.Length - data.Position)
                return null;

            try
            {
                // Cache the current offset
                long initialOffset = data.Position;

                // Create a new Volume to fill
                var xrd = new File();

                xrd.Magic = data.ReadBytes(5);
                if (!magic.EqualsExactly(Constants.MagicBytes))
                    return null;

                xrd.Version = data.ReadByteValue();
                if (xrd.Version != 0x01)
                    return null;

                xrd.XGDType = data.ReadByteValue();
                bool xgd1 = xrd.XGDType == 1;
                bool xgd2 = xrd.XGDType == 2;
                bool xgd3 = xrd.XGDType == 3;

                xrd.XGDSubtype = data.ReadByteValue();
                xrd.Ringcode = data.ReadBytes(8);
                xrd.RedumpSize = data.ReadUInt64LittleEndian();
                xrd.RedumpCRC = data.ReadBytes(4);
                xrd.RedumpMD5 = data.ReadBytes(16);
                xrd.RedumpSHA1 = data.ReadBytes(20);
                xrd.RawXISOSize = data.ReadUInt64LittleEndian();
                xrd.RawXISOCRC = data.ReadBytes(4);
                xrd.RawXISOMD5 = data.ReadBytes(16);
                xrd.RawXISOSHA1 = data.ReadBytes(20);
                xrd.CookedXISOSize = data.ReadUInt64LittleEndian();
                xrd.CookedXISOCRC = data.ReadBytes(4);
                xrd.CookedXISOMD5 = data.ReadBytes(16);
                xrd.CookedXISOSHA1 = data.ReadBytes(20);
                xrd.VideoISOSize = data.ReadUInt64LittleEndian();
                xrd.VideoISOCRC = data.ReadBytes(4);
                xrd.VideoISOMD5 = data.ReadBytes(16);
                xrd.VideoISOSHA1 = data.ReadBytes(20);
                xrd.FillerCRC = data.ReadBytes(4);
                xrd.FillerMD5 = data.ReadBytes(16);
                xrd.FillerSHA1 = data.ReadBytes(20);

                // Read security sector ranges
                int securityCount = (xgd2 || xgd3) ? 2 : 16;
                xrd.SecuritySectors = new uint[securityCount];
                for (int i = 0; i < securityCount; i++)
                {
                    xrd.SecuritySectors[i] = data.ReadUInt32LittleEndian();
                }

                // Read XGD3 Video ISO details
                if (xgd3)
                {
                    xrd.CookedVideoISOSHA1 = data.ReadBytes(20);
                    xrd.SystemUpdateHash = data.ReadBytes(20);
                }

                // Read Certificate data
                if (xgd1)
                {
                    xrd.XboxCertificate = XboxExecutable.ParseCertificate();
                }
                else if (xgd2 || xgd3)
                {
                    xrd.Xbox360Certificate = XenonExecutable.ParseCertificate();
                }

                xrd.FileCount = data.ReadUInt64LittleEndian();

                FileInfo[] files = new FileInfo[xrd.FileCount];
                for (int i = 0; i < xrd.FileCount; i++)
                {
                    FileInfo fileInfo = new FileInfo();
                    fileInfo.Offset = data.ReadUInt32LittleEndian();
                    fileInfo.SHA1 = data.ReadBytes(20);
                    xrd.FileInfo[i] = fileInfo;
                }

                xrd.DirectoryCount = data.ReadUInt64LittleEndian();

                DirectoryInfo[] directories = new DirectoryInfo[xrd.DirectoryCount];
                for (int i = 0; i < xrd.DirectoryCount; i++)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo();
                    dirInfo.Offset = data.ReadUInt32LittleEndian();
                    dirInfo.Size = data.ReadUInt32LittleEndian();
                    var dd = ParseDirectoryDescriptor();
                    if (dd is null)
                        return null;
                    dirInfo.DirectoryDescriptor = dd;
                    xrd.DirectoryInfo[i] = dirInfo;
                }

                return file;
            }
            catch
            {
                // Ignore the actual error
                return null;
            }
        }
    }
}

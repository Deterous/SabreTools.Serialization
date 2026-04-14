using System;
using System.IO;
using System.Text;
using SabreTools.Data.Models.XDVDFS;
using SabreTools.IO;
using SabreTools.IO.Extensions;
using SabreTools.Numerics;
using SabreTools.Numerics.Extensions;

namespace SabreTools.Serialization.Writers
{
    public class XDVDFS : BaseBinaryWriter<Volume>
    {
        /// <inheritdoc/>
        public override Stream? SerializeStream(Volume? obj)
        {
            // If the data is invalid
            if (obj?.VolumeDescriptor?.StartSignature is null)
                return null;

            // If the magic doesn't match
            string magic = Encoding.ASCII.GetString(obj.VolumeDescriptor.StartSignature);
            if (magic != Constants.VolumeDescriptorSignature)
                return null;

            // Validate model
            if (obj.ReservedArea.Length != 0x10000)
                return null;
            if (obj.VolumeDescriptor.Reserved.Length != 1991)
                return null;
            if (obj.VolumeDescriptor.EndSignature.Length != 20)
                return null;
            if (obj.LayoutDescriptor is not null)
            {
                if (obj.LayoutDescriptor.Signature.Length != 20)
                    return null;
                if (obj.LayoutDescriptor.Unused8Bytes.Length != 8)
                    return null;
                if (obj.LayoutDescriptor.Reserved.Length != 1968)
                    return null;
            }

            // Create the output stream
            var stream = new MemoryStream();

            stream.Write(obj.ReservedArea, 0, obj.ReservedArea.Length);

            stream.Write(obj.VolumeDescriptor.StartSignature, 0, obj.VolumeDescriptor.StartSignature.Length);
            stream.WriteLittleEndian(obj.VolumeDescriptor.RootOffset);
            stream.WriteLittleEndian(obj.VolumeDescriptor.RootSize);
            stream.WriteLittleEndian(obj.VolumeDescriptor.MasteringTimestamp);
            stream.WriteByte(obj.VolumeDescriptor.UnknownByte);
            stream.Write(obj.VolumeDescriptor.Reserved, 0, obj.VolumeDescriptor.Reserved.Length);
            stream.Write(obj.VolumeDescriptor.EndSignature, 0, obj.VolumeDescriptor.EndSignature.Length);

            if (obj.LayoutDescriptor is not null)
            {
                stream.Write(obj.LayoutDescriptor.Signature, 0, obj.LayoutDescriptor.Signature.Length);
                stream.Write(obj.LayoutDescriptor.Unused8Bytes, 0, obj.LayoutDescriptor.Unused8Bytes.Length);
                SerializeFourPartVersionType(stream, obj.LayoutDescriptor.XBLayoutVersion);
                SerializeFourPartVersionType(stream, obj.LayoutDescriptor.XBPremasterVersion);
                SerializeFourPartVersionType(stream, obj.LayoutDescriptor.XBGameDiscVersion);
                SerializeFourPartVersionType(stream, obj.LayoutDescriptor.XBOther1Version);
                SerializeFourPartVersionType(stream, obj.LayoutDescriptor.XBOther2Version);
                SerializeFourPartVersionType(stream, obj.LayoutDescriptor.XBOther3Version);
                stream.Write(obj.LayoutDescriptor.Reserved, 0, obj.LayoutDescriptor.Reserved.Length);
            }

            // Loop over all directory descriptors in order of offset
            uint[] keys = new uint[obj.DirectoryDescriptors.Count];
            obj.DirectoryDescriptors.Keys.CopyTo(keys, 0);
            Array.Sort(keys);
            for (int i = 0; i < keys.Length; i++)
            {
                uint sectorOffset = keys[i];
                stream.SeekIfPossible(sectorOffset * Constants.SectorSize, SeekOrigin.Begin);
                SerializeDirectoryDescriptor(stream, obj.DirectoryDescriptors[sectorOffset]);
            }

            return stream;
        }

        public static void SerializeFourPartVersionType(Stream stream, FourPartVersionType obj)
        {
            stream.WriteLittleEndian(obj.Major);
            stream.WriteLittleEndian(obj.Minor);
            stream.WriteLittleEndian(obj.Build);
            stream.WriteLittleEndian(obj.Revision);
        }

        public static void SerializeDirectoryDescriptor(Stream stream, DirectoryDescriptor obj)
        {
            foreach (var dr in obj.DirectoryRecords)
                SerializeDirectoryRecord(stream, dr);
            if (obj.Padding is not null && obj.Padding.Length > 0)
                stream.Write(obj.Padding, 0, obj.Padding.Length);
        }

        public static void SerializeDirectoryRecord(Stream stream, DirectoryRecord obj)
        {
            stream.WriteLittleEndian(obj.LeftChildOffset);
            stream.WriteLittleEndian(obj.RightChildOffset);
            stream.WriteLittleEndian(obj.ExtentOffset);
            stream.WriteLittleEndian(obj.ExtentSize);
            stream.WriteByte((byte)obj.FileFlags);
            stream.WriteByte(obj.FilenameLength);
            stream.Write(obj.Filename, 0, obj.Filename.Length);
            if (obj.Padding is not null && obj.Padding.Length > 0)
                stream.Write(obj.Padding, 0, obj.Padding.Length);
        }
    }
}

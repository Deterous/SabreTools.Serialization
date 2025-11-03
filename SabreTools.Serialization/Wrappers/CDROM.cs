using System.Collections.Generic;
using System.IO;
using SabreTools.Data.Models.CDROM;
using SabreTools.Data.Models.ISO9660;

namespace SabreTools.Serialization.Wrappers
{
    public partial class CDROM : WrapperBase<DataTrack>
    {
        #region Descriptive Properties

        /// <inheritdoc/>
        public override string DescriptionString => "CD-ROM Data Track";

        #endregion

        #region Sub Wrappers

        public ISO9660? FileSystem;

        #endregion

        #region Extension Properties

        /// <inheritdoc cref="DataTrack.Sectors"/>
        public Sector[] Sectors => Model.Sectors;

        /// <inheritdoc cref="DataTrack.Volume.SystemArea"/>
        public byte[] SystemArea => Model.Volume.SystemArea;

        /// <inheritdoc cref="DataTrack.Volume.VolumeDescriptorSet"/>
        public VolumeDescriptor[] VolumeDescriptorSet => Model.Volume.VolumeDescriptorSet;

        /// <inheritdoc cref="DataTrack.Volume.PathTableGroups"/>
        public PathTableGroup[] PathTableGroups => Model.Volume.PathTableGroups;

        /// <inheritdoc cref="DataTrack.Volume.DirectoryDescriptors"/>
        public Dictionary<int, FileExtent> DirectoryDescriptors => Model.Volume.DirectoryDescriptors;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public CDROM(DataTrack model, byte[] data, ISO9660 iso9660) : base(model, data)
        {
            FileSystem = iso9660;
        }

        /// <inheritdoc/>
        public CDROM(DataTrack model, byte[] data, int offset, ISO9660 iso9660) : base(model, data, offset)
        {
            FileSystem = iso9660;
        }

        /// <inheritdoc/>
        public CDROM(DataTrack model, byte[] data, int offset, int length, ISO9660 iso9660) : base(model, data, offset, length)
        {
            FileSystem = iso9660;
        }

        /// <inheritdoc/>
        public CDROM(DataTrack model, Stream data, ISO9660 iso9660) : base(model, data)
        {
            FileSystem = iso9660;
        }

        /// <inheritdoc/>
        public CDROM(DataTrack model, Stream data, long offset, ISO9660 iso9660) : base(model, data, offset)
        {
            FileSystem = iso9660;
        }

        /// <inheritdoc/>
        public CDROM(DataTrack model, Stream data, long offset, long length, ISO9660 iso9660) : base(model, data, offset, length)
        {
            FileSystem = iso9660;
        }

        #endregion

        #region Static Constructors

        /// <summary>
        /// Create a CDROM data track from a byte array and offset
        /// </summary>
        /// <param name="data">Byte array representing the CDROM data track</param>
        /// <param name="offset">Offset within the array to parse</param>
        /// <returns>A CDROM data track wrapper on success, null on failure</returns>
        public static CDROM? Create(byte[]? data, int offset)
        {
            // If the data is invalid
            if (data == null || data.Length == 0)
                return null;

            // If the offset is out of bounds
            if (offset < 0 || offset >= data.Length)
                return null;

            // Create a memory stream and use that
            var dataStream = new MemoryStream(data, offset, data.Length - offset);
            return Create(dataStream);
        }

        /// <summary>
        /// Create a CDROM data track from a Stream
        /// </summary>
        /// <param name="data">Stream representing the CDROM data track</param>
        /// <returns>A CDROM data track wrapper on success, null on failure</returns>
        public static CDROM? Create(Stream? data)
        {
            // If the data is invalid
            if (data == null || !data.CanRead)
                return null;

            try
            {
                // Cache the current offset
                long currentOffset = data.Position;

                // Create sub-streams
                // TODO: CDROM sub-stream
                SabreTools.Data.Extensions.CDROM.ISO9660Stream userData = new(data);

                var model = new DataTrack();
                // TODO: Set model.Sectors using CDROM Deserializer on CDROM sub-stream
                ISO9660 iso = ISO9660.Create(userData);
                if (iso != null)
                    model.Volume = iso.Model;
                if (model == null)
                    return null;

                return new CDROM(model, data, currentOffset, iso);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}



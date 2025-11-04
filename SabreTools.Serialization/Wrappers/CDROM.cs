using System.Collections.Generic;
using System.IO;
using SabreTools.Data.Models.CDROM;
using SabreTools.Data.Models.ISO9660;

namespace SabreTools.Serialization.Wrappers
{
    public partial class CDROM : ISO9660, IWrapper<DataTrack>
    {
        #region Descriptive Properties

        /// <inheritdoc/>
        public override string DescriptionString => "CD-ROM Data Track";

        #endregion

        public DataTrack GetModel() => CDROMModel;

        /// <summary>
        /// Internal model
        /// </summary>
        public DataTrack CDROMModel { get; }

        #region Extension Properties

        /// <inheritdoc cref="DataTrack.Sectors"/>
        public Sector[] Sectors => Model.Sectors;

        /// <inheritdoc cref="DataTrack.Volume.SystemArea"/>
        public override byte[] SystemArea => Model.Volume.SystemArea;

        /// <inheritdoc cref="DataTrack.Volume.VolumeDescriptorSet"/>
        public override VolumeDescriptor[] VolumeDescriptorSet => Model.Volume.VolumeDescriptorSet;

        /// <inheritdoc cref="DataTrack.Volume.PathTableGroups"/>
        public override PathTableGroup[] PathTableGroups => Model.Volume.PathTableGroups;

        /// <inheritdoc cref="DataTrack.Volume.DirectoryDescriptors"/>
        public override Dictionary<int, FileExtent> DirectoryDescriptors => Model.Volume.DirectoryDescriptors;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public CDROM(DataTrack model, byte[] data) : base(model, data) { }

        /// <inheritdoc/>
        public CDROM(DataTrack model, byte[] data, int offset) : base(model, data, offset) { }

        /// <inheritdoc/>
        public CDROM(DataTrack model, byte[] data, int offset, int length) : base(model, data, offset, length) { }

        /// <inheritdoc/>
        public CDROM(DataTrack model, Stream data) : base(model, data) { }

        /// <inheritdoc/>
        public CDROM(DataTrack model, Stream data, long offset) : base(model, data, offset) { }

        /// <inheritdoc/>
        public CDROM(DataTrack model, Stream data, long offset, long length) : base(model, data, offset, length) { }

        #endregion

        #region Static Constructors

        /// <summary>
        /// Create a CDROM data track from a byte array and offset
        /// </summary>
        /// <param name="data">Byte array representing the CDROM data track</param>
        /// <param name="offset">Offset within the array to parse</param>
        /// <returns>A CDROM data track wrapper on success, null on failure</returns>
        public new static CDROM? Create(byte[]? data, int offset)
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
        public new static CDROM? Create(Stream? data)
        {
            // If the data is invalid
            if (data == null || !data.CanRead || !data.CanSeek)
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
                model.Volume = new Readers.ISO9660().Deserialize(data);

                return new CDROM(model, data, currentOffset);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}



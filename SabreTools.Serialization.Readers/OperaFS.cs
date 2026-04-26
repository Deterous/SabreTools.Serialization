using System.Collections.Generic;
using System.IO;
using SabreTools.Data.Extensions;
using SabreTools.Data.Models.OperaFS;
using SabreTools.IO.Extensions;
using SabreTools.Matching;
using SabreTools.Numerics.Extensions;

namespace SabreTools.Serialization.Readers
{
    public class OperaFS : BaseBinaryReader<FileSystem>
    {
        /// <inheritdoc cref="Deserialize(Stream?)" />
        public FileSystem? Deserialize(Stream? data)
        {
            // If the data is invalid
            if (data is null || !data.CanRead)
                return null;

            // Simple check for a valid stream length
            if (sectorLength < Constants.SectorSize)
                return null;

            try
            {
                // Cache the current offset
                long initialOffset = data.Position;

                // Create a new FileSystem to fill
                var volume = new FileSystem();

                var volumeDescriptor = ParseVolumeDescriptor(data);
                if (volumeDescriptor is null)
                    return null;

                volume.VolumeDescriptor = volumeDescriptor;

                var directories = ParseDirectories(data, volumeDescriptor, initialOffset);
                if (directories is null)
                    return null;
                
                volume.Directories = directories;

                return volume;
            }
            catch
            {
                // Ignore the actual error
                return null;
            }
        }

        /// <summary>
        /// Parse a Stream into an OperaFS Volume Descriptor
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled VolumeDescriptor on success, null on error</returns>
        public static VolumeDescriptor ParseVolumeDescriptor(Stream data)
        {
            var volumeDescriptor = new VolumeDescriptor();
            
            volumeDescriptor.RecordType = data.ReadByteValue();
            volumeDescriptor.VolumeSyncBytes = data.ReadBytes(5);
            volumeDescriptor.StructureVersion = data.ReadUInt16BigEndian();
            volumeDescriptor.VolumeFlags = (VolumeFlags)data.ReadByteValue();
            volumeDescriptor.VolumeCommentary = data.ReadBytes(32);
            volumeDescriptor.VolumeIdentifier = data.ReadBytes(32);
            volumeDescriptor.VolumeUniqueIdentifier = data.ReadUInt32BigEndian();
            volumeDescriptor.VolumeBlockSize = data.ReadUInt32BigEndian();
            volumeDescriptor.VolumeBlockCount = data.ReadUInt32BigEndian();
            volumeDescriptor.RootUniqueIdentifier = data.ReadUInt32BigEndian();
            volumeDescriptor.RootDirectoryBlockCount = data.ReadUInt32BigEndian();
            volumeDescriptor.RootDirectoryBlockSize = data.ReadUInt32BigEndian();
            volumeDescriptor.RootDirectoryLastAvatarIndex = data.ReadUInt32BigEndian();
            
            for (int i = 0; i < 8; i++)
            {
                volumeDescriptor.RootDirectoryAvatarList[i] = data.ReadUInt32BigEndian();
            }

            volumeDescriptor.Padding = data.ReadBytes(0x77C);
        }

        /// <summary>
        /// Parse a Stream into an map of OperaFS directories from a volume descriptor
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled map of directories on success, null on error</returns>
        public static Dictionary<uint, Directory> ParseDirectories(Stream data, VolumeDescriptor volumeDescriptor, long initialOffset)
        {
            var directories = new Dictionary<uint, Directory>();

            data.SeekIfPossible(initialOffset + volumeDescriptor.RootDirectoryAvatarList[0] * Constants.SectorSize, SeekOrigin.Begin);
            var rootDirectory = ParseDirectory(data);
            for (int i = 0; i <= volumeDescriptor.RootDirectoryLastAvatarIndex; i++)
            {
                directories.Add(volumeDescriptor.RootDirectoryAvatarList[i], rootDirectory);
            }

            var childDirectories = ParseChildDirectories(data, rootDirectory, initialOffset);
            foreach (var kvp in childDirectories)
            {
                if (!directories.ContainsKey(kvp.Key))
                    obj.Add(kvp.Key, kvp.Value);
            }

            return directories;
        }

        /// <summary>
        /// Parse a Stream into an map of OperaFS directories from a directory
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled map of directories on success, null on error</returns>
        public static Dictionary<uint, Directory> ParseChildDirectories(Stream data, Directory parent, long initialOffset)
        {
            var directories = new Dictionary<uint, Directory>();

            foreach (var dr in parent.DirectoryRecords)
            {
                data.SeekIfPossible(initialOffset + dr.AvatarList[0] * Constants.SectorSize, SeekOrigin.Begin);
                var directory = ParseDirectory(data);
                for (int i = 0; i <= volumeDescriptor.LastAvatarIndex; i++)
                {
                    directories.Add(volumeDescriptor.AvatarList[i], directory);
                }
            }

            return directories;
        }

        /// <summary>
        /// Parse a Stream into an OperaFS Directory
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Directory on success, null on error</returns>
        public static Directory ParseDirectory(Stream data)
        {
            var directory = new Directory();

            directory.NextBlock = data.ReadInt32BigEndian();
            directory.PreviousBlock = data.ReadInt32BigEndian();
            directory.Flags = data.ReadUInt32BigEndian();
            directory.FirstFreeByte = data.ReadUInt32BigEndian();
            directory.FirstEntryOffset = data.ReadUInt32BigEndian();

            var directoryRecords = List<DirectoryRecord>();

            long startPosition = data.Position;
            while (data.Position < startPosition + directory.FirstFreeByte)
            {
                var directoryRecord = ParseDirectoryRecord(Stream data);
                directoryRecords.Add(directoryRecord);

                if ((directoryRecord.DirectoryRecordFlags & DirectoryRecordFlags.DIRECTORY_FINAL) != 0)
                    break;

                if ((directoryRecord.DirectoryRecordFlags & DirectoryRecordFlags.BLOCK_FINAL) != 0)
                {
                    long nextBlock = Constants.SectorSize - ((data.Position - startPosition) % Constants.SectorSize);
                    data.SeekIfPossible(nextBlock, SeekOrigin.Current);
                }
            }

            directory.DirectoryRecords = directoryRecords.ToArray();

            return directory;
        }

        /// <summary>
        /// Parse a Stream into an OperaFS Directory Record
        /// </summary>
        /// <param name="data">Stream to parse</param>
        /// <returns>Filled Directory Record on success, null on error</returns>
        public static Directory ParseDirectoryRecord(Stream data)
        {
            var directoryRecord = new DirectoryRecord();
            
            directoryRecord.DirectoryRecordFlags = (DirectoryRecordFlags)data.ReadUInt32BigEndian();
            directoryRecord.UniqueIdentifier = data.ReadBytes(4);
            directoryRecord.Type = data.ReadBytes(4);
            directoryRecord.BlockSize = data.ReadUInt32BigEndian();
            directoryRecord.ByteCount = data.ReadUInt32BigEndian();
            directoryRecord.BlockCount = data.ReadUInt32BigEndian();
            directoryRecord.Burst = data.ReadUInt32BigEndian();
            directoryRecord.Gap = data.ReadUInt32BigEndian();
            directoryRecord.Filename = data.ReadBytes(32);
            directoryRecord.LastAvatarIndex = data.ReadUInt32BigEndian();
            
            directoryRecord.AvatarList = new uint[directoryRecord.LastAvatarIndex + 1];
            for (int i = 0; i < directoryRecord.LastAvatarIndex; i++)
            {
                directoryRecord.AvatarList[i] = data.ReadUInt32BigEndian();
            }

            return directoryRecord;
        }
    }
}

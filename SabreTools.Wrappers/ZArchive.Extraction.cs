using System;
#if NET462_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
using System.Collections.Generic;
using System.IO;
using System.Text;
using SabreTools.Data.Extensions;
using SabreTools.Data.Models.ZArchive;
using SabreTools.IO.Extensions;
using SabreTools.Numerics.Extensions;
using SharpCompress.Compressors.ZStandard;
#endif

namespace SabreTools.Wrappers
{
    public partial class ZArchive : IExtractable
    {
        /// <inheritdoc/>
        public bool Extract(string outputDirectory, bool includeDebug)
        {
            if (_dataSource is null || !_dataSource.CanRead)
                return false;
            
#if NET462_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            try
            {
                // Extract all files and directories from root (index 0)
                return ExtractDirectory(outputDirectory, includeDebug, 0);
            }
            catch (Exception ex)
            {
                if (includeDebug) Console.Error.WriteLine(ex);
                return false;
            }
#else
            Console.WriteLine("Extraction is not supported for this framework!");
            Console.WriteLine();
            return false;
#endif
        }

#if NET462_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
        /// <inheritdoc/>
        public bool ExtractDirectory(string outputDirectory, bool includeDebug, uint index)
        {
            bool success = true;

            // Create directory
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            // Extract all children of current node
            FileDirectoryEntry node = FileTree[index];
            if (node is DirectoryEntry dir)
            {
                for (uint i = 0; i < dir.Count; i++)
                {
                    uint childIndex = dir.NodeStartIndex + i;
                    var child = FileTree[childIndex];
                    string? name = child.GetName(NameTable);
                    if (string.IsNullOrEmpty(name))
                    {
                        if (includeDebug) Console.WriteLine("Invalid node name");
                        return false;
                    }

                    string outputPath = Path.Combine(outputDirectory, name);
                    if (child.IsDirectory())
                        success |= ExtractDirectory(outputPath, includeDebug, childIndex);
                    else
                        success |= ExtractFile(outputPath, includeDebug, childIndex);
                }

                return success;
            }
            else
            {
                if (includeDebug) Console.WriteLine("Invalid directory node");
                return false;
            }
        }

        /// <inheritdoc/>
        public bool ExtractFile(string outputPath, bool includeDebug, uint index)
        {
            // Decompress each chunk to output
            var node = FileTree[index];
            var rawOffset = Footer.SectionCompressedData.Offset;
            if (node is FileEntry file)
            {
                var fileOffset = ((ulong)file.FileOffsetHigh << 32) | (ulong)file.FileOffsetLow;
                var fileSize = ((ulong)file.FileSizeHigh << 32) | (ulong)file.FileSizeLow;

                // Write the output file
                if (includeDebug) Console.WriteLine($"Extracting: {outputPath}");
                using var fs = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                ulong readOffset = 0;

                lock (_dataSourceLock)
                {
                    while (readOffset < fileSize)
                    {
                        // Determine which block to read
                        int blockIndex = (int)((fileOffset + readOffset) / (ulong)Constants.BlockSize);
                        int recordIndex = blockIndex / Constants.BlocksPerOffsetRecord;
                        int withinRecordIndex = blockIndex % Constants.BlocksPerOffsetRecord;

                        int expectedSize = Math.Min((int)(fileSize - readOffset), Constants.BlockSize);
                        int bytesToRead = Math.Min((int)(OffsetRecords[recordIndex].Size[withinRecordIndex]), Constants.BlockSize);

                        ulong recordOffset = OffsetRecords[recordIndex].Offset;
                        ulong blockOffset = recordOffset;
                        for (int i = 0; i < withinRecordIndex; i++)
                        {
                            blockOffset += OffsetRecords[recordIndex].Size[i];
                        }

                        _dataSource.SeekIfPossible((long)blockOffset, SeekOrigin.Begin);

                        // Make sure it won't EOF
                        if (bytesToRead > _dataSource.Length - _dataSource.Position)
                        {
                            if (includeDebug) Console.WriteLine($"File out of bounds: {outputPath}");
                            return false;
                        }

                        var buffer = _dataSource.ReadBytes(bytesToRead);

                        // Decompress buffer
                        using (var inputStream = new MemoryStream(buffer))
                        using (var zstdStream = new DecompressionStream(inputStream))
                        using (var outputStream = new MemoryStream())
                        {
                            zstdStream.CopyTo(outputStream);
                            byte[] decompressedBuffer = outputStream.ToArray();
                            if (decompressedBuffer.Length != expectedSize)
                            {
                                if (includeDebug) Console.WriteLine($"Invalid decompressed block size {decompressedBuffer.Length}");
                                return false;
                            }

                            // Write decompressed buffer to output file
                            fs.Write(decompressedBuffer, 0, expectedSize);
                            fs.Flush();
                            readOffset += (ulong)bytesToRead;
                        }
                    }
                }

                return true;
            }
            else
            {
                if (includeDebug) Console.WriteLine("Invalid file node");
                return false;
            }
        }
#endif
    }
}

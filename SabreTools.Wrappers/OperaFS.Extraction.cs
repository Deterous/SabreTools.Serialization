using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SabreTools.Data.Extensions;
using SabreTools.Data.Models.OperaFS;
using SabreTools.IO.Extensions;
using SabreTools.Matching;
using SabreTools.Numerics.Extensions;

namespace SabreTools.Wrappers
{
    public partial class OperaFS : IExtractable
    {
        #region Extraction State

        /// <summary>
        /// List of extracted files by their sector offset
        /// </summary>
        private readonly Dictionary<int, uint> extractedFiles = [];

        /// <summary>
        /// List of extracted directories
        /// </summary>
        private readonly List<DirectoryDescriptor> extractedDirectories = [];

        #endregion

        /// <inheritdoc/>
        public virtual bool Extract(string outputDirectory, bool includeDebug)
        {
            // Clear the extraction state
            extractedFiles.Clear();
            multiExtentFiles.Clear();

            bool allExtracted = true;

            for (int i = 0; i <= VolumeDescriptor.RootDirectoryLastAvatarIndex; i++)
            {
                if (debug) Console.WriteLine($"Inspecting root directory at offset {VolumeDescriptor.RootDirectoryAvatarList[i]}");
                bool alreadyExtracted = false;
                var rootDirectory = Directories[VolumeDescriptor.RootDirectoryAvatarList[i]];
                // foreach (var key in extractedDirectories.Keys)
                // {
                //     if (object.ReferenceEquals(key, childDir))
                //     {
                //         alreadyExtracted = true;
                //         break;
                //     }
                // }
                if (extractedDirectories.Contains(rootDirectory))
                {
                    if (debug) Console.WriteLine($"Root directory duplicate");
                    alreadyExtracted = true;
                    break;
                }

                if (!alreadyExtracted)
                {
                    if (debug) Console.WriteLine($"Extracting from root directory");
                    ExtractDirectory(outputDirectory, includeDebug, rootDirectory);
                    extractedDirectories.Add(rootDirectory);
                }
            }

            return allExtracted;
        }

        public bool ExtractDirectory(string outputDirectory, bool includeDebug, DirectoryDescriptor dir)
        {
            // Create output directory if it does not exist
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            bool success = true;
            foreach (var dr in dir)
            {
                var filename = Encoding.UTF8.GetString(dr.Filename);
                if ((dr.DirectoryRecordFlags & DirectoryRecordFlags.DIRECTORY) == 0)
                {
                    // Skip filesystem only files (e.g. Volume Descriptor "Disc Label")
                    if ((dr.DirectoryRecordFlags & DirectoryRecordFlags.SYSTEM) != 0)
                        continue;

                    for (int i = 0; i <= dr.LastAvatarIndex; i++)
                    {
                        var fileOffset = dr.AvatarList[i];
                        if (!extractedFiles.Contains(fileOffset))
                        {
                            try
                            {
                                if (debug) Console.WriteLine($"Extracting file {filename} at offset {fileOffset}");
                                // TODO: Extract file

                                extractedFiles.Add(fileOffset);
                            }
                            catch
                            {
                                success = false;
                            }
                        }
                    }
                }
                else
                {
                    outputDirectory = Path.Combine(outputDirectory, filename);

                    // Iterate over all avatars, in case they're not identical
                    for (int i = 0; i <= dr.LastAvatarIndex; i++)
                    {
                        if (debug) Console.WriteLine($"Inspecting directory at offset {dr.AvatarList[i]}");
                        // Check whether directory is already extracted
                        bool alreadyExtracted = false;
                        var childDir = Directories[dr.AvatarList[i]];
                        // foreach (var key in extractedDirectories.Keys)
                        // {
                        //     if (object.ReferenceEquals(key, childDir))
                        //     {
                        //         alreadyExtracted = true;
                        //         break;
                        //     }
                        // }
                        if (extractedDirectories.Contains(childDir))
                        {
                            alreadyExtracted = true;
                            break;
                        }

                        // Extract directory if it has not already been done
                        if (!alreadyExtracted)
                        {
                            ExtractDirectory(outputDirectory, includeDebug, childDir);
                            extractedDirectories.Add(childDir);
                        }
                    }
                }
            }
        }
    }
}

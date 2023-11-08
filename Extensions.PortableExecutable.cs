using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using SabreTools.IO;
using SabreTools.Models.PortableExecutable;

namespace SabreTools.Serialization
{
    public static partial class Extensions
    {
        /// <summary>
        /// Convert a relative virtual address to a physical one
        /// </summary>
        /// <param name="rva">Relative virtual address to convert</param>
        /// <param name="sections">Array of sections to check against</param>
        /// <returns>Physical address, 0 on error</returns>
        public static uint ConvertVirtualAddress(this uint rva, SectionHeader?[]? sections)
        {
            // If we have an invalid section table, we can't do anything
            if (sections == null || sections.Length == 0)
                return 0;

            // If the RVA is 0, we just return 0 because it's invalid
            if (rva == 0)
                return 0;

            // If the RVA matches a section start exactly, use that
            var matchingSection = sections.FirstOrDefault(s => s != null && s.VirtualAddress == rva);
            if (matchingSection != null)
                return rva - matchingSection.VirtualAddress + matchingSection.PointerToRawData;

            // Loop through all of the sections
            for (int i = 0; i < sections.Length; i++)
            {
                // If the section is invalid, just skip it
                if (sections[i] == null)
                    continue;

                // If the section "starts" at 0, just skip it
                if (sections[i]!.PointerToRawData == 0)
                    continue;

                // Attempt to derive the physical address from the current section
                var section = sections[i]!;
                if (rva >= section.VirtualAddress && section.VirtualSize != 0 && rva <= section.VirtualAddress + section.VirtualSize)
                    return rva - section.VirtualAddress + section.PointerToRawData;
                else if (rva >= section.VirtualAddress && section.SizeOfRawData != 0 && rva <= section.VirtualAddress + section.SizeOfRawData)
                    return rva - section.VirtualAddress + section.PointerToRawData;
            }

            return 0;
        }

        /// <summary>
        /// Find the section a revlative virtual address lives in
        /// </summary>
        /// <param name="rva">Relative virtual address to convert</param>
        /// <param name="sections">Array of sections to check against</param>
        /// <returns>Section index, null on error</returns>
        public static int ContainingSectionIndex(this uint rva, SectionHeader?[]? sections)
        {
            // If we have an invalid section table, we can't do anything
            if (sections == null || sections.Length == 0)
                return -1;

            // If the RVA is 0, we just return -1 because it's invalid
            if (rva == 0)
                return -1;

            // Loop through all of the sections
            for (int i = 0; i < sections.Length; i++)
            {
                // If the section is invalid, just skip it
                var section = sections[i];
                if (section == null)
                    continue;

                // If the section "starts" at 0, just skip it
                if (section.PointerToRawData == 0)
                    continue;

                // Attempt to derive the physical address from the current section
                if (rva >= section.VirtualAddress && section.VirtualSize != 0 && rva <= section.VirtualAddress + section.VirtualSize)
                    return i;
                else if (rva >= section.VirtualAddress && section.SizeOfRawData != 0 && rva <= section.VirtualAddress + section.SizeOfRawData)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Read overlay data as a SecuROM AddD overlay data
        /// </summary>
        /// <param name="data">Data to parse into overlay data</param>
        /// <param name="offset">Offset into the byte array</param>
        /// <returns>A filled SecuROM AddD overlay data on success, null on error</returns>
        public static SecuROMAddD? AsSecuROMAddD(this byte[]? data, ref int offset)
        {
            // If we have data that's invalid, we can't do anything
            if (data == null)
                return null;

            // Read in the table
            var addD = new SecuROMAddD();

            addD.Signature = data.ReadUInt32(ref offset);
            if (addD.Signature != 0x44646441)
                return null;

            int originalOffset = offset;

            addD.EntryCount = data.ReadUInt32(ref offset);
            addD.Version = data.ReadString(ref offset, Encoding.ASCII);
            if (string.IsNullOrWhiteSpace(addD.Version))
                offset = originalOffset + 0x10;

            addD.Build = data.ReadBytes(ref offset, 4)?.Select(b => (char)b)?.ToArray();

            // Distinguish between v1 and v2
            int bytesToRead = 112; // v2
            if (string.IsNullOrWhiteSpace(addD.Version)
                || addD.Version.StartsWith("3")
                || addD.Version.StartsWith("4.47"))
            {
                bytesToRead = 44;
            }

            addD.Unknown14h = data.ReadBytes(ref offset, bytesToRead);

            addD.Entries = new SecuROMAddDEntry[addD.EntryCount];
            for (int i = 0; i < addD.EntryCount; i++)
            {
                var addDEntry = new SecuROMAddDEntry();

                addDEntry.PhysicalOffset = data.ReadUInt32(ref offset);
                addDEntry.Length = data.ReadUInt32(ref offset);
                addDEntry.Unknown08h = data.ReadUInt32(ref offset);
                addDEntry.Unknown0Ch = data.ReadUInt32(ref offset);
                addDEntry.Unknown10h = data.ReadUInt32(ref offset);
                addDEntry.Unknown14h = data.ReadUInt32(ref offset);
                addDEntry.Unknown18h = data.ReadUInt32(ref offset);
                addDEntry.Unknown1Ch = data.ReadUInt32(ref offset);
                addDEntry.FileName = data.ReadString(ref offset, Encoding.ASCII);
                addDEntry.Unknown2Ch = data.ReadUInt32(ref offset);

                addD.Entries[i] = addDEntry;
            }

            return addD;
        }

        #region Debug

        /// <summary>
        /// Read debug data as an NB10 Program Database
        /// </summary>
        /// <param name="data">Data to parse into a database</param>
        /// <param name="offset">Offset into the byte array</param>
        /// <returns>A filled NB10 Program Database on success, null on error</returns>
        public static NB10ProgramDatabase? AsNB10ProgramDatabase(this byte[] data, ref int offset)
        {
            // If we have data that's invalid, we can't do anything
            if (data == null)
                return null;

            var nb10ProgramDatabase = new NB10ProgramDatabase();

            nb10ProgramDatabase.Signature = data.ReadUInt32(ref offset);
            if (nb10ProgramDatabase.Signature != 0x3031424E)
                return null;

            nb10ProgramDatabase.Offset = data.ReadUInt32(ref offset);
            nb10ProgramDatabase.Timestamp = data.ReadUInt32(ref offset);
            nb10ProgramDatabase.Age = data.ReadUInt32(ref offset);
            nb10ProgramDatabase.PdbFileName = data.ReadString(ref offset, Encoding.ASCII); // TODO: Actually null-terminated UTF-8?

            return nb10ProgramDatabase;
        }

        /// <summary>
        /// Read debug data as an RSDS Program Database
        /// </summary>
        /// <param name="data">Data to parse into a database</param>
        /// <param name="offset">Offset into the byte array</param>
        /// <returns>A filled RSDS Program Database on success, null on error</returns>
        public static RSDSProgramDatabase? AsRSDSProgramDatabase(this byte[]? data, ref int offset)
        {
            // If we have data that's invalid, we can't do anything
            if (data == null)
                return null;

            var rsdsProgramDatabase = new RSDSProgramDatabase();

            rsdsProgramDatabase.Signature = data.ReadUInt32(ref offset);
            if (rsdsProgramDatabase.Signature != 0x53445352)
                return null;

            var guid = data.ReadBytes(ref offset, 0x10);
            if (guid != null)
                rsdsProgramDatabase.GUID = new Guid(guid);
            rsdsProgramDatabase.Age = data.ReadUInt32(ref offset);
            rsdsProgramDatabase.PathAndFileName = data.ReadString(ref offset, Encoding.ASCII); // TODO: Actually null-terminated UTF-8

            return rsdsProgramDatabase;
        }

        #endregion

        // TODO: Implement other resource types from https://learn.microsoft.com/en-us/windows/win32/menurc/resource-file-formats
        #region Resources

        /// <summary>
        /// Read resource data as a resource header
        /// </summary>
        /// <param name="data">Data to parse into a resource header</param>
        /// <param name="offset">Offset into the byte array</param>
        /// <returns>A filled resource header on success, null on error</returns>
        public static ResourceHeader? AsResourceHeader(this byte[]? data, ref int offset)
        {
            // If we have data that's invalid, we can't do anything
            if (data == null)
                return null;

            // Read in the table
            var header = new ResourceHeader();
            header.DataSize = data.ReadUInt32(ref offset);
            header.HeaderSize = data.ReadUInt32(ref offset);
            header.ResourceType = (ResourceType)data.ReadUInt32(ref offset); // TODO: Could be a string too
            header.Name = data.ReadUInt32(ref offset); // TODO: Could be a string too
            header.DataVersion = data.ReadUInt32(ref offset);
            header.MemoryFlags = (MemoryFlags)data.ReadUInt16(ref offset);
            header.LanguageId = data.ReadUInt16(ref offset);
            header.Version = data.ReadUInt32(ref offset);
            header.Characteristics = data.ReadUInt32(ref offset);

            return header;
        }

        /// <summary>
        /// Read resource data as an accelerator table resource
        /// </summary>
        /// <param name="entry">Resource data entry to parse into an accelerator table resource</param>
        /// <returns>A filled accelerator table resource on success, null on error</returns>
        public static AcceleratorTableEntry[]? AsAcceleratorTableResource(this ResourceDataEntry? entry)
        {
            // If we have data that's invalid for this resource type, we can't do anything
            if (entry?.Data == null || entry.Data.Length % 8 != 0)
                return null;

            // Get the number of entries
            int count = entry.Data.Length / 8;

            // Initialize the iterator
            int offset = 0;

            // Create the output object
            var table = new AcceleratorTableEntry[count];

            // Read in the table
            for (int i = 0; i < count; i++)
            {
                var acceleratorTableEntry = new AcceleratorTableEntry();

                acceleratorTableEntry.Flags = (AcceleratorTableFlags)entry.Data.ReadUInt16(ref offset);
                acceleratorTableEntry.Ansi = entry.Data.ReadUInt16(ref offset);
                acceleratorTableEntry.Id = entry.Data.ReadUInt16(ref offset);
                acceleratorTableEntry.Padding = entry.Data.ReadUInt16(ref offset);

                table[i] = acceleratorTableEntry;
            }

            return table;
        }

        /// <summary>
        /// Read resource data as a side-by-side assembly manifest
        /// </summary>
        /// <param name="entry">Resource data entry to parse into a side-by-side assembly manifest</param>
        /// <returns>A filled side-by-side assembly manifest on success, null on error</returns>
        public static AssemblyManifest? AsAssemblyManifest(this ResourceDataEntry? entry)
        {
            // If we have an invalid entry, just skip
            if (entry?.Data == null)
                return null;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AssemblyManifest));
                return serializer.Deserialize(new MemoryStream(entry.Data)) as AssemblyManifest;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Read resource data as a dialog box
        /// </summary>
        /// <param name="entry">Resource data entry to parse into a dialog box</param>
        /// <returns>A filled dialog box on success, null on error</returns>
        public static DialogBoxResource? AsDialogBox(this ResourceDataEntry? entry)
        {
            // If we have an invalid entry, just skip
            if (entry?.Data == null)
                return null;

            // Initialize the iterator
            int offset = 0;

            // Create the output object
            var dialogBoxResource = new DialogBoxResource();

            // Try to read the signature for an extended dialog box template
            int signatureOffset = sizeof(ushort);
            int possibleSignature = entry.Data.ReadUInt16(ref signatureOffset);
            if (possibleSignature == 0xFFFF)
            {
                #region Extended dialog template

                var dialogTemplateExtended = new DialogTemplateExtended();

                dialogTemplateExtended.Version = entry.Data.ReadUInt16(ref offset);
                dialogTemplateExtended.Signature = entry.Data.ReadUInt16(ref offset);
                dialogTemplateExtended.HelpID = entry.Data.ReadUInt32(ref offset);
                dialogTemplateExtended.ExtendedStyle = (ExtendedWindowStyles)entry.Data.ReadUInt32(ref offset);
                dialogTemplateExtended.Style = (WindowStyles)entry.Data.ReadUInt32(ref offset);
                dialogTemplateExtended.DialogItems = entry.Data.ReadUInt16(ref offset);
                dialogTemplateExtended.PositionX = entry.Data.ReadInt16(ref offset);
                dialogTemplateExtended.PositionY = entry.Data.ReadInt16(ref offset);
                dialogTemplateExtended.WidthX = entry.Data.ReadInt16(ref offset);
                dialogTemplateExtended.HeightY = entry.Data.ReadInt16(ref offset);

                #region Menu resource

                int currentOffset = offset;
                ushort menuResourceIdentifier = entry.Data.ReadUInt16(ref offset);
                offset = currentOffset;

                // 0x0000 means no elements
                if (menuResourceIdentifier == 0x0000)
                {
                    // Increment the pointer if it was empty
                    offset += sizeof(ushort);
                }
                else
                {
                    // Flag if there's an ordinal at the end
                    bool menuResourceHasOrdinal = menuResourceIdentifier == 0xFFFF;
                    if (menuResourceHasOrdinal)
                        offset += sizeof(ushort);

                    // Read the menu resource as a string
                    dialogTemplateExtended.MenuResource = entry.Data.ReadString(ref offset, Encoding.Unicode);

                    // Align to the WORD boundary if we're not at the end
                    if (offset != entry.Data.Length)
                    {
                        while ((offset % 2) != 0)
                            _ = entry.Data.ReadByte(ref offset);
                    }

                    // Read the ordinal if we have the flag set
                    if (menuResourceHasOrdinal)
                        dialogTemplateExtended.MenuResourceOrdinal = entry.Data.ReadUInt16(ref offset);
                }

                #endregion

                #region Class resource

                currentOffset = offset;
                ushort classResourceIdentifier = entry.Data.ReadUInt16(ref offset);
                offset = currentOffset;

                // 0x0000 means no elements
                if (classResourceIdentifier == 0x0000)
                {
                    // Increment the pointer if it was empty
                    offset += sizeof(ushort);
                }
                else
                {
                    // Flag if there's an ordinal at the end
                    bool classResourcehasOrdinal = classResourceIdentifier == 0xFFFF;
                    if (classResourcehasOrdinal)
                        offset += sizeof(ushort);

                    // Read the class resource as a string
                    dialogTemplateExtended.ClassResource = entry.Data.ReadString(ref offset, Encoding.Unicode);

                    // Align to the WORD boundary if we're not at the end
                    if (offset != entry.Data.Length)
                    {
                        while ((offset % 2) != 0)
                            _ = entry.Data.ReadByte(ref offset);
                    }

                    // Read the ordinal if we have the flag set
                    if (classResourcehasOrdinal)
                        dialogTemplateExtended.ClassResourceOrdinal = entry.Data.ReadUInt16(ref offset);
                }

                #endregion

                #region Title resource

                currentOffset = offset;
                ushort titleResourceIdentifier = entry.Data.ReadUInt16(ref offset);
                offset = currentOffset;

                // 0x0000 means no elements
                if (titleResourceIdentifier == 0x0000)
                {
                    // Increment the pointer if it was empty
                    offset += sizeof(ushort);
                }
                else
                {
                    // Read the title resource as a string
                    dialogTemplateExtended.TitleResource = entry.Data.ReadString(ref offset, Encoding.Unicode);

                    // Align to the WORD boundary if we're not at the end
                    if (offset != entry.Data.Length)
                    {
                        while ((offset % 2) != 0)
                            _ = entry.Data.ReadByte(ref offset);
                    }
                }

                #endregion

                #region Point size and typeface

                // Only if DS_SETFONT is set are the values here used
                if (dialogTemplateExtended.Style.HasFlag(WindowStyles.DS_SETFONT))
                {
                    dialogTemplateExtended.PointSize = entry.Data.ReadUInt16(ref offset);
                    dialogTemplateExtended.Weight = entry.Data.ReadUInt16(ref offset);
                    dialogTemplateExtended.Italic = entry.Data.ReadByte(ref offset);
                    dialogTemplateExtended.CharSet = entry.Data.ReadByte(ref offset);
                    dialogTemplateExtended.Typeface = entry.Data.ReadString(ref offset, Encoding.Unicode);
                }

                // Align to the DWORD boundary if we're not at the end
                if (offset != entry.Data.Length)
                {
                    while ((offset % 4) != 0)
                        _ = entry.Data.ReadByte(ref offset);
                }

                #endregion

                dialogBoxResource.ExtendedDialogTemplate = dialogTemplateExtended;

                #endregion

                #region Extended dialog item templates

                var dialogItemExtendedTemplates = new List<DialogItemTemplateExtended>();

                for (int i = 0; i < dialogTemplateExtended.DialogItems; i++)
                {
                    var dialogItemTemplate = new DialogItemTemplateExtended();

                    dialogItemTemplate.HelpID = entry.Data.ReadUInt32(ref offset);
                    dialogItemTemplate.ExtendedStyle = (ExtendedWindowStyles)entry.Data.ReadUInt32(ref offset);
                    dialogItemTemplate.Style = (WindowStyles)entry.Data.ReadUInt32(ref offset);
                    dialogItemTemplate.PositionX = entry.Data.ReadInt16(ref offset);
                    dialogItemTemplate.PositionY = entry.Data.ReadInt16(ref offset);
                    dialogItemTemplate.WidthX = entry.Data.ReadInt16(ref offset);
                    dialogItemTemplate.HeightY = entry.Data.ReadInt16(ref offset);
                    dialogItemTemplate.ID = entry.Data.ReadUInt32(ref offset);

                    #region Class resource

                    currentOffset = offset;
                    ushort itemClassResourceIdentifier = entry.Data.ReadUInt16(ref offset);
                    offset = currentOffset;

                    // 0xFFFF means ordinal only
                    if (itemClassResourceIdentifier == 0xFFFF)
                    {
                        // Increment the pointer
                        _ = entry.Data.ReadUInt16(ref offset);

                        // Read the ordinal
                        dialogItemTemplate.ClassResourceOrdinal = (DialogItemTemplateOrdinal)entry.Data.ReadUInt16(ref offset);
                    }
                    else
                    {
                        // Flag if there's an ordinal at the end
                        bool classResourcehasOrdinal = itemClassResourceIdentifier == 0xFFFF;
                        if (classResourcehasOrdinal)
                            offset += sizeof(ushort);

                        // Read the class resource as a string
                        dialogItemTemplate.ClassResource = entry.Data.ReadString(ref offset, Encoding.Unicode);

                        // Align to the WORD boundary if we're not at the end
                        if (offset != entry.Data.Length)
                        {
                            while ((offset % 2) != 0)
                                _ = entry.Data.ReadByte(ref offset);
                        }
                    }

                    #endregion

                    #region Title resource

                    currentOffset = offset;
                    ushort itemTitleResourceIdentifier = entry.Data.ReadUInt16(ref offset);
                    offset = currentOffset;

                    // 0xFFFF means ordinal only
                    if (itemTitleResourceIdentifier == 0xFFFF)
                    {
                        // Increment the pointer
                        _ = entry.Data.ReadUInt16(ref offset);

                        // Read the ordinal
                        dialogItemTemplate.TitleResourceOrdinal = entry.Data.ReadUInt16(ref offset);
                    }
                    else
                    {
                        // Read the title resource as a string
                        dialogItemTemplate.TitleResource = entry.Data.ReadString(ref offset, Encoding.Unicode);

                        // Align to the WORD boundary if we're not at the end
                        if (offset != entry.Data.Length)
                        {
                            while ((offset % 2) != 0)
                                _ = entry.Data.ReadByte(ref offset);
                        }
                    }

                    #endregion

                    #region Creation data

                    dialogItemTemplate.CreationDataSize = entry.Data.ReadUInt16(ref offset);
                    if (dialogItemTemplate.CreationDataSize != 0)
                        dialogItemTemplate.CreationData = entry.Data.ReadBytes(ref offset, dialogItemTemplate.CreationDataSize);

                    #endregion

                    // Align to the DWORD boundary if we're not at the end
                    if (offset != entry.Data.Length)
                    {
                        while ((offset % 4) != 0)
                            _ = entry.Data.ReadByte(ref offset);
                    }

                    dialogItemExtendedTemplates.Add(dialogItemTemplate);
                }

                dialogBoxResource.ExtendedDialogItemTemplates = dialogItemExtendedTemplates.ToArray();

                #endregion
            }
            else
            {
                #region Dialog template

                var dialogTemplate = new DialogTemplate();

                dialogTemplate.Style = (WindowStyles)entry.Data.ReadUInt32(ref offset);
                dialogTemplate.ExtendedStyle = (ExtendedWindowStyles)entry.Data.ReadUInt32(ref offset);
                dialogTemplate.ItemCount = entry.Data.ReadUInt16(ref offset);
                dialogTemplate.PositionX = entry.Data.ReadInt16(ref offset);
                dialogTemplate.PositionY = entry.Data.ReadInt16(ref offset);
                dialogTemplate.WidthX = entry.Data.ReadInt16(ref offset);
                dialogTemplate.HeightY = entry.Data.ReadInt16(ref offset);

                #region Menu resource

                int currentOffset = offset;
                ushort menuResourceIdentifier = entry.Data.ReadUInt16(ref offset);
                offset = currentOffset;

                // 0x0000 means no elements
                if (menuResourceIdentifier == 0x0000)
                {
                    // Increment the pointer if it was empty
                    offset += sizeof(ushort);
                }
                else
                {
                    // Flag if there's an ordinal at the end
                    bool menuResourceHasOrdinal = menuResourceIdentifier == 0xFFFF;
                    if (menuResourceHasOrdinal)
                        offset += sizeof(ushort);

                    // Read the menu resource as a string
                    dialogTemplate.MenuResource = entry.Data.ReadString(ref offset, Encoding.Unicode);

                    // Align to the WORD boundary if we're not at the end
                    if (offset != entry.Data.Length)
                    {
                        while ((offset % 2) != 0)
                            _ = entry.Data.ReadByte(ref offset);
                    }

                    // Read the ordinal if we have the flag set
                    if (menuResourceHasOrdinal)
                        dialogTemplate.MenuResourceOrdinal = entry.Data.ReadUInt16(ref offset);
                }

                #endregion

                #region Class resource

                currentOffset = offset;
                ushort classResourceIdentifier = entry.Data.ReadUInt16(ref offset);
                offset = currentOffset;

                // 0x0000 means no elements
                if (classResourceIdentifier == 0x0000)
                {
                    // Increment the pointer if it was empty
                    offset += sizeof(ushort);
                }
                else
                {
                    // Flag if there's an ordinal at the end
                    bool classResourcehasOrdinal = classResourceIdentifier == 0xFFFF;
                    if (classResourcehasOrdinal)
                        offset += sizeof(ushort);

                    // Read the class resource as a string
                    dialogTemplate.ClassResource = entry.Data.ReadString(ref offset, Encoding.Unicode);

                    // Align to the WORD boundary if we're not at the end
                    if (offset != entry.Data.Length)
                    {
                        while ((offset % 2) != 0)
                            _ = entry.Data.ReadByte(ref offset);
                    }

                    // Read the ordinal if we have the flag set
                    if (classResourcehasOrdinal)
                        dialogTemplate.ClassResourceOrdinal = entry.Data.ReadUInt16(ref offset);
                }

                #endregion

                #region Title resource

                currentOffset = offset;
                ushort titleResourceIdentifier = entry.Data.ReadUInt16(ref offset);
                offset = currentOffset;

                // 0x0000 means no elements
                if (titleResourceIdentifier == 0x0000)
                {
                    // Increment the pointer if it was empty
                    offset += sizeof(ushort);
                }
                else
                {
                    // Read the title resource as a string
                    dialogTemplate.TitleResource = entry.Data.ReadString(ref offset, Encoding.Unicode);

                    // Align to the WORD boundary if we're not at the end
                    if (offset != entry.Data.Length)
                    {
                        while ((offset % 2) != 0)
                            _ = entry.Data.ReadByte(ref offset);
                    }
                }

                #endregion

                #region Point size and typeface

                // Only if DS_SETFONT is set are the values here used
                if (dialogTemplate.Style.HasFlag(WindowStyles.DS_SETFONT))
                {
                    dialogTemplate.PointSizeValue = entry.Data.ReadUInt16(ref offset);

                    // Read the font name as a string
                    dialogTemplate.Typeface = entry.Data.ReadString(ref offset, Encoding.Unicode);
                }

                // Align to the DWORD boundary if we're not at the end
                if (offset != entry.Data.Length)
                {
                    while ((offset % 4) != 0)
                        _ = entry.Data.ReadByte(ref offset);
                }

                #endregion

                dialogBoxResource.DialogTemplate = dialogTemplate;

                #endregion

                #region Dialog item templates

                var dialogItemTemplates = new List<DialogItemTemplate>();

                for (int i = 0; i < dialogTemplate.ItemCount; i++)
                {
                    var dialogItemTemplate = new DialogItemTemplate();

                    dialogItemTemplate.Style = (WindowStyles)entry.Data.ReadUInt32(ref offset);
                    dialogItemTemplate.ExtendedStyle = (ExtendedWindowStyles)entry.Data.ReadUInt32(ref offset);
                    dialogItemTemplate.PositionX = entry.Data.ReadInt16(ref offset);
                    dialogItemTemplate.PositionY = entry.Data.ReadInt16(ref offset);
                    dialogItemTemplate.WidthX = entry.Data.ReadInt16(ref offset);
                    dialogItemTemplate.HeightY = entry.Data.ReadInt16(ref offset);
                    dialogItemTemplate.ID = entry.Data.ReadUInt16(ref offset);

                    #region Class resource

                    currentOffset = offset;
                    ushort itemClassResourceIdentifier = entry.Data.ReadUInt16(ref offset);
                    offset = currentOffset;

                    // 0xFFFF means ordinal only
                    if (itemClassResourceIdentifier == 0xFFFF)
                    {
                        // Increment the pointer
                        _ = entry.Data.ReadUInt16(ref offset);

                        // Read the ordinal
                        dialogItemTemplate.ClassResourceOrdinal = (DialogItemTemplateOrdinal)entry.Data.ReadUInt16(ref offset);
                    }
                    else
                    {
                        // Flag if there's an ordinal at the end
                        bool classResourcehasOrdinal = itemClassResourceIdentifier == 0xFFFF;
                        if (classResourcehasOrdinal)
                            offset += sizeof(ushort);

                        // Read the class resource as a string
                        dialogItemTemplate.ClassResource = entry.Data.ReadString(ref offset, Encoding.Unicode);

                        // Align to the WORD boundary if we're not at the end
                        if (offset != entry.Data.Length)
                        {
                            while ((offset % 2) != 0)
                                _ = entry.Data.ReadByte(ref offset);
                        }
                    }

                    #endregion

                    #region Title resource

                    currentOffset = offset;
                    ushort itemTitleResourceIdentifier = entry.Data.ReadUInt16(ref offset);
                    offset = currentOffset;

                    // 0xFFFF means ordinal only
                    if (itemTitleResourceIdentifier == 0xFFFF)
                    {
                        // Increment the pointer
                        _ = entry.Data.ReadUInt16(ref offset);

                        // Read the ordinal
                        dialogItemTemplate.TitleResourceOrdinal = entry.Data.ReadUInt16(ref offset);
                    }
                    else
                    {
                        // Read the title resource as a string
                        dialogItemTemplate.TitleResource = entry.Data.ReadString(ref offset, Encoding.Unicode);

                        // Align to the WORD boundary if we're not at the end
                        if (offset != entry.Data.Length)
                        {
                            while ((offset % 2) != 0)
                                _ = entry.Data.ReadByte(ref offset);
                        }
                    }

                    #endregion

                    #region Creation data

                    dialogItemTemplate.CreationDataSize = entry.Data.ReadUInt16(ref offset);
                    if (dialogItemTemplate.CreationDataSize != 0)
                        dialogItemTemplate.CreationData = entry.Data.ReadBytes(ref offset, dialogItemTemplate.CreationDataSize);

                    #endregion

                    // Align to the DWORD boundary if we're not at the end
                    if (offset != entry.Data.Length)
                    {
                        while ((offset % 4) != 0)
                            _ = entry.Data.ReadByte(ref offset);
                    }

                    dialogItemTemplates.Add(dialogItemTemplate);
                }

                dialogBoxResource.DialogItemTemplates = dialogItemTemplates.ToArray();

                #endregion
            }

            return dialogBoxResource;
        }

        /// <summary>
        /// Read resource data as a font group
        /// </summary>
        /// <param name="entry">Resource data entry to parse into a font group</param>
        /// <returns>A filled font group on success, null on error</returns>
        public static FontGroupHeader? AsFontGroup(this ResourceDataEntry? entry)
        {
            // If we have an invalid entry, just skip
            if (entry?.Data == null)
                return null;

            // Initialize the iterator
            int offset = 0;

            // Create the output object
            var fontGroupHeader = new FontGroupHeader();

            fontGroupHeader.NumberOfFonts = entry.Data.ReadUInt16(ref offset);
            if (fontGroupHeader.NumberOfFonts > 0)
            {
                fontGroupHeader.DE = new DirEntry[fontGroupHeader.NumberOfFonts];
                for (int i = 0; i < fontGroupHeader.NumberOfFonts; i++)
                {
                    var dirEntry = new DirEntry();

                    dirEntry.FontOrdinal = entry.Data.ReadUInt16(ref offset);

                    dirEntry.Entry = new FontDirEntry();
                    dirEntry.Entry.Version = entry.Data.ReadUInt16(ref offset);
                    dirEntry.Entry.Size = entry.Data.ReadUInt32(ref offset);
                    dirEntry.Entry.Copyright = entry.Data.ReadBytes(ref offset, 60);
                    dirEntry.Entry.Type = entry.Data.ReadUInt16(ref offset);
                    dirEntry.Entry.Points = entry.Data.ReadUInt16(ref offset);
                    dirEntry.Entry.VertRes = entry.Data.ReadUInt16(ref offset);
                    dirEntry.Entry.HorizRes = entry.Data.ReadUInt16(ref offset);
                    dirEntry.Entry.Ascent = entry.Data.ReadUInt16(ref offset);
                    dirEntry.Entry.InternalLeading = entry.Data.ReadUInt16(ref offset);
                    dirEntry.Entry.ExternalLeading = entry.Data.ReadUInt16(ref offset);
                    dirEntry.Entry.Italic = entry.Data.ReadByte(ref offset);
                    dirEntry.Entry.Underline = entry.Data.ReadByte(ref offset);
                    dirEntry.Entry.StrikeOut = entry.Data.ReadByte(ref offset);
                    dirEntry.Entry.Weight = entry.Data.ReadUInt16(ref offset);
                    dirEntry.Entry.CharSet = entry.Data.ReadByte(ref offset);
                    dirEntry.Entry.PixWidth = entry.Data.ReadUInt16(ref offset);
                    dirEntry.Entry.PixHeight = entry.Data.ReadUInt16(ref offset);
                    dirEntry.Entry.PitchAndFamily = entry.Data.ReadByte(ref offset);
                    dirEntry.Entry.AvgWidth = entry.Data.ReadUInt16(ref offset);
                    dirEntry.Entry.MaxWidth = entry.Data.ReadUInt16(ref offset);
                    dirEntry.Entry.FirstChar = entry.Data.ReadByte(ref offset);
                    dirEntry.Entry.LastChar = entry.Data.ReadByte(ref offset);
                    dirEntry.Entry.DefaultChar = entry.Data.ReadByte(ref offset);
                    dirEntry.Entry.BreakChar = entry.Data.ReadByte(ref offset);
                    dirEntry.Entry.WidthBytes = entry.Data.ReadUInt16(ref offset);
                    dirEntry.Entry.Device = entry.Data.ReadUInt32(ref offset);
                    dirEntry.Entry.Face = entry.Data.ReadUInt32(ref offset);
                    dirEntry.Entry.Reserved = entry.Data.ReadUInt32(ref offset);

                    // TODO: Determine how to read these two? Immediately after?
                    dirEntry.Entry.DeviceName = entry.Data.ReadString(ref offset);
                    dirEntry.Entry.FaceName = entry.Data.ReadString(ref offset);

                    fontGroupHeader.DE[i] = dirEntry;
                }
            }

            // TODO: Implement entry parsing
            return fontGroupHeader;
        }

        /// <summary>
        /// Read resource data as a menu
        /// </summary>
        /// <param name="entry">Resource data entry to parse into a menu</param>
        /// <returns>A filled menu on success, null on error</returns>
        public static MenuResource? AsMenu(this ResourceDataEntry? entry)
        {
            // If we have an invalid entry, just skip
            if (entry?.Data == null)
                return null;

            // Initialize the iterator
            int offset = 0;

            // Create the output object
            var menuResource = new MenuResource();

            // Try to read the version for an extended header
            int versionOffset = 0;
            int possibleVersion = entry.Data.ReadUInt16(ref versionOffset);
            if (possibleVersion == 0x0001)
            {
                #region Extended menu header

                var menuHeaderExtended = new MenuHeaderExtended();

                menuHeaderExtended.Version = entry.Data.ReadUInt16(ref offset);
                menuHeaderExtended.Offset = entry.Data.ReadUInt16(ref offset);
                menuHeaderExtended.HelpID = entry.Data.ReadUInt32(ref offset);

                menuResource.ExtendedMenuHeader = menuHeaderExtended;

                #endregion

                #region Extended dialog item templates

                var extendedMenuItems = new List<MenuItemExtended>();

                if (offset != 0)
                {
                    offset = menuHeaderExtended.Offset;

                    while (offset < entry.Data.Length)
                    {
                        var extendedMenuItem = new MenuItemExtended();

                        extendedMenuItem.ItemType = (MenuFlags)entry.Data.ReadUInt32(ref offset);
                        extendedMenuItem.State = (MenuFlags)entry.Data.ReadUInt32(ref offset);
                        extendedMenuItem.ID = entry.Data.ReadUInt32(ref offset);
                        extendedMenuItem.Flags = (MenuFlags)entry.Data.ReadUInt32(ref offset);
                        extendedMenuItem.MenuText = entry.Data.ReadString(ref offset, Encoding.Unicode);

                        // Align to the DWORD boundary if we're not at the end
                        if (offset != entry.Data.Length)
                        {
                            while ((offset % 4) != 0)
                                _ = entry.Data.ReadByte(ref offset);
                        }

                        extendedMenuItems.Add(extendedMenuItem);
                    }
                }

                menuResource.ExtendedMenuItems = extendedMenuItems.ToArray();

                #endregion
            }
            else
            {
                #region Menu header

                var menuHeader = new MenuHeader();

                menuHeader.Version = entry.Data.ReadUInt16(ref offset);
                menuHeader.HeaderSize = entry.Data.ReadUInt16(ref offset);

                menuResource.MenuHeader = menuHeader;

                #endregion

                #region Menu items

                var menuItems = new List<MenuItem>();

                while (offset < entry.Data.Length)
                {
                    var menuItem = new MenuItem();

                    // Determine if this is a popup
                    int flagsOffset = offset;
                    var initialFlags = (MenuFlags)entry.Data.ReadUInt16(ref flagsOffset);
                    if (initialFlags.HasFlag(MenuFlags.MF_POPUP))
                    {
                        menuItem.PopupItemType = (MenuFlags)entry.Data.ReadUInt32(ref offset);
                        menuItem.PopupState = (MenuFlags)entry.Data.ReadUInt32(ref offset);
                        menuItem.PopupID = entry.Data.ReadUInt32(ref offset);
                        menuItem.PopupResInfo = (MenuFlags)entry.Data.ReadUInt32(ref offset);
                        menuItem.PopupMenuText = entry.Data.ReadString(ref offset, Encoding.Unicode);
                    }
                    else
                    {
                        menuItem.NormalResInfo = (MenuFlags)entry.Data.ReadUInt16(ref offset);
                        menuItem.NormalMenuText = entry.Data.ReadString(ref offset, Encoding.Unicode);
                    }

                    // Align to the DWORD boundary if we're not at the end
                    if (offset != entry.Data.Length)
                    {
                        while ((offset % 4) != 0)
                            _ = entry.Data.ReadByte(ref offset);
                    }

                    menuItems.Add(menuItem);
                }

                menuResource.MenuItems = menuItems.ToArray();

                #endregion
            }

            return menuResource;
        }

        /// <summary>
        /// Read resource data as a message table resource
        /// </summary>
        /// <param name="entry">Resource data entry to parse into a message table resource</param>
        /// <returns>A filled message table resource on success, null on error</returns>
        public static MessageResourceData? AsMessageResourceData(this ResourceDataEntry? entry)
        {
            // If we have an invalid entry, just skip
            if (entry?.Data == null)
                return null;

            // Initialize the iterator
            int offset = 0;

            // Create the output object
            var messageResourceData = new MessageResourceData();

            // Message resource blocks
            messageResourceData.NumberOfBlocks = entry.Data.ReadUInt32(ref offset);
            if (messageResourceData.NumberOfBlocks > 0)
            {
                var messageResourceBlocks = new List<MessageResourceBlock>();

                for (int i = 0; i < messageResourceData.NumberOfBlocks; i++)
                {
                    var messageResourceBlock = new MessageResourceBlock();

                    messageResourceBlock.LowId = entry.Data.ReadUInt32(ref offset);
                    messageResourceBlock.HighId = entry.Data.ReadUInt32(ref offset);
                    messageResourceBlock.OffsetToEntries = entry.Data.ReadUInt32(ref offset);

                    messageResourceBlocks.Add(messageResourceBlock);
                }

                messageResourceData.Blocks = messageResourceBlocks.ToArray();
            }

            // Message resource entries
            if (messageResourceData.Blocks != null && messageResourceData.Blocks.Length != 0)
            {
                var messageResourceEntries = new Dictionary<uint, MessageResourceEntry?>();

                for (int i = 0; i < messageResourceData.Blocks.Length; i++)
                {
                    var messageResourceBlock = messageResourceData.Blocks[i];
                    if (messageResourceBlock == null)
                        continue;

                    offset = (int)messageResourceBlock.OffsetToEntries;

                    for (uint j = messageResourceBlock.LowId; j <= messageResourceBlock.HighId; j++)
                    {
                        var messageResourceEntry = new MessageResourceEntry();

                        messageResourceEntry.Length = entry.Data.ReadUInt16(ref offset);
                        messageResourceEntry.Flags = entry.Data.ReadUInt16(ref offset);

                        Encoding textEncoding = messageResourceEntry.Flags == 0x0001 ? Encoding.Unicode : Encoding.ASCII;
                        byte[]? textArray = entry.Data.ReadBytes(ref offset, messageResourceEntry.Length - 4);
                        if (textArray != null)
                            messageResourceEntry.Text = textEncoding.GetString(textArray);

                        messageResourceEntries[j] = messageResourceEntry;
                    }
                }

                messageResourceData.Entries = messageResourceEntries;
            }

            return messageResourceData;
        }

        /// <summary>
        /// Read resource data as a string table resource
        /// </summary>
        /// <param name="entry">Resource data entry to parse into a string table resource</param>
        /// <returns>A filled string table resource on success, null on error</returns>
        public static Dictionary<int, string?>? AsStringTable(this ResourceDataEntry? entry)
        {
            // If we have an invalid entry, just skip
            if (entry?.Data == null)
                return null;

            // Initialize the iterators
            int offset = 0, stringIndex = 0;

            // Create the output table
            var stringTable = new Dictionary<int, string?>();

            // Loop through and add 
            while (offset < entry.Data.Length)
            {
                ushort stringLength = entry.Data.ReadUInt16(ref offset);
                if (stringLength == 0)
                {
                    stringTable[stringIndex++] = "[EMPTY]";
                }
                else
                {
                    if (stringLength * 2 > entry.Data.Length - offset)
                    {
                        Console.WriteLine($"{stringLength * 2} requested but {entry.Data.Length - offset} remains");
                        stringLength = (ushort)((entry.Data.Length - offset) / 2);
                    }

                    string stringValue = Encoding.Unicode.GetString(entry.Data, offset, stringLength * 2);
                    offset += stringLength * 2;
                    stringValue = stringValue.Replace("\n", "\\n").Replace("\r", newValue: "\\r");
                    stringTable[stringIndex++] = stringValue;
                }
            }

            return stringTable;
        }

        /// <summary>
        /// Read resource data as a version info resource
        /// </summary>
        /// <param name="entry">Resource data entry to parse into a version info resource</param>
        /// <returns>A filled version info resource on success, null on error</returns>
        public static VersionInfo? AsVersionInfo(this ResourceDataEntry? entry)
        {
            // If we have an invalid entry, just skip
            if (entry?.Data == null)
                return null;

            // Initialize the iterator
            int offset = 0;

            // Create the output object
            var versionInfo = new VersionInfo();

            versionInfo.Length = entry.Data.ReadUInt16(ref offset);
            versionInfo.ValueLength = entry.Data.ReadUInt16(ref offset);
            versionInfo.ResourceType = (VersionResourceType)entry.Data.ReadUInt16(ref offset);
            versionInfo.Key = entry.Data.ReadString(ref offset, Encoding.Unicode);
            if (versionInfo.Key != "VS_VERSION_INFO")
                return null;

            while ((offset % 4) != 0)
                versionInfo.Padding1 = entry.Data.ReadUInt16(ref offset);

            // Read fixed file info
            if (versionInfo.ValueLength > 0)
            {
                var fixedFileInfo = new FixedFileInfo();
                fixedFileInfo.Signature = entry.Data.ReadUInt32(ref offset);
                if (fixedFileInfo.Signature != 0xFEEF04BD)
                    return null;

                fixedFileInfo.StrucVersion = entry.Data.ReadUInt32(ref offset);
                fixedFileInfo.FileVersionMS = entry.Data.ReadUInt32(ref offset);
                fixedFileInfo.FileVersionLS = entry.Data.ReadUInt32(ref offset);
                fixedFileInfo.ProductVersionMS = entry.Data.ReadUInt32(ref offset);
                fixedFileInfo.ProductVersionLS = entry.Data.ReadUInt32(ref offset);
                fixedFileInfo.FileFlagsMask = entry.Data.ReadUInt32(ref offset);
                fixedFileInfo.FileFlags = (FixedFileInfoFlags)(entry.Data.ReadUInt32(ref offset) & fixedFileInfo.FileFlagsMask);
                fixedFileInfo.FileOS = (FixedFileInfoOS)entry.Data.ReadUInt32(ref offset);
                fixedFileInfo.FileType = (FixedFileInfoFileType)entry.Data.ReadUInt32(ref offset);
                fixedFileInfo.FileSubtype = (FixedFileInfoFileSubtype)entry.Data.ReadUInt32(ref offset);
                fixedFileInfo.FileDateMS = entry.Data.ReadUInt32(ref offset);
                fixedFileInfo.FileDateLS = entry.Data.ReadUInt32(ref offset);
                versionInfo.Value = fixedFileInfo;
            }

            while ((offset % 4) != 0)
                versionInfo.Padding2 = entry.Data.ReadUInt16(ref offset);

            // TODO: Make the following block a private helper method

            // Determine if we have a StringFileInfo or VarFileInfo next
            if (offset < versionInfo.Length)
            {
                // Cache the current offset for reading
                int currentOffset = offset;

                offset += 6;
                string? nextKey = entry.Data.ReadString(ref offset, Encoding.Unicode);
                offset = currentOffset;

                if (nextKey == "StringFileInfo")
                {
                    var stringFileInfo = AsStringFileInfo(entry.Data, ref offset);
                    versionInfo.StringFileInfo = stringFileInfo;
                }
                else if (nextKey == "VarFileInfo")
                {
                    var varFileInfo = AsVarFileInfo(entry.Data, ref offset);
                    versionInfo.VarFileInfo = varFileInfo;
                }
            }

            // And again
            if (offset < versionInfo.Length)
            {
                // Cache the current offset for reading
                int currentOffset = offset;

                offset += 6;
                string? nextKey = entry.Data.ReadString(ref offset, Encoding.Unicode);
                offset = currentOffset;

                if (nextKey == "StringFileInfo")
                {
                    var stringFileInfo = AsStringFileInfo(entry.Data, ref offset);
                    versionInfo.StringFileInfo = stringFileInfo;
                }
                else if (nextKey == "VarFileInfo")
                {
                    var varFileInfo = AsVarFileInfo(entry.Data, ref offset);
                    versionInfo.VarFileInfo = varFileInfo;
                }
            }

            return versionInfo;
        }

        /// <summary>
        ///  Read byte data as a string file info resource
        /// </summary>
        /// <param name="data">Data to parse into a string file info</param>
        /// <param name="offset">Offset into the byte array</param>
        /// <returns>A filled string file info resource on success, null on error</returns>
        private static StringFileInfo? AsStringFileInfo(byte[] data, ref int offset)
        {
            var stringFileInfo = new StringFileInfo();

            // Cache the initial offset
            int currentOffset = offset;

            stringFileInfo.Length = data.ReadUInt16(ref offset);
            stringFileInfo.ValueLength = data.ReadUInt16(ref offset);
            stringFileInfo.ResourceType = (VersionResourceType)data.ReadUInt16(ref offset);
            stringFileInfo.Key = data.ReadString(ref offset, Encoding.Unicode);
            if (stringFileInfo.Key != "StringFileInfo")
            {
                offset -= 6 + ((stringFileInfo.Key?.Length ?? 0 + 1) * 2);
                return null;
            }

            // Align to the DWORD boundary if we're not at the end
            if (offset != data.Length)
            {
                while ((offset % 4) != 0)
                    stringFileInfo.Padding = data.ReadByte(ref offset);
            }

            var stringFileInfoChildren = new List<StringTable>();
            while ((offset - currentOffset) < stringFileInfo.Length)
            {
                var stringTable = new StringTable();

                stringTable.Length = data.ReadUInt16(ref offset);
                stringTable.ValueLength = data.ReadUInt16(ref offset);
                stringTable.ResourceType = (VersionResourceType)data.ReadUInt16(ref offset);
                stringTable.Key = data.ReadString(ref offset, Encoding.Unicode);

                // Align to the DWORD boundary if we're not at the end
                if (offset != data.Length)
                {
                    while ((offset % 4) != 0)
                        stringTable.Padding = data.ReadByte(ref offset);
                }

                var stringTableChildren = new List<StringData>();
                while ((offset - currentOffset) < stringTable.Length)
                {
                    var stringData = new StringData();

                    stringData.Length = data.ReadUInt16(ref offset);
                    stringData.ValueLength = data.ReadUInt16(ref offset);
                    stringData.ResourceType = (VersionResourceType)data.ReadUInt16(ref offset);
                    stringData.Key = data.ReadString(ref offset, Encoding.Unicode);

                    // Align to the DWORD boundary if we're not at the end
                    if (offset != data.Length)
                    {
                        while ((offset % 4) != 0)
                            stringData.Padding = data.ReadByte(ref offset);
                    }

                    if (stringData.ValueLength > 0)
                    {
                        byte[]? valueBytes = data.ReadBytes(ref offset, stringData.ValueLength * sizeof(ushort));
                        if (valueBytes != null)
                            stringData.Value = Encoding.Unicode.GetString(valueBytes);
                    }

                    // Align to the DWORD boundary if we're not at the end
                    if (offset != data.Length)
                    {
                        while ((offset % 4) != 0)
                            _ = data.ReadByte(ref offset);
                    }

                    stringTableChildren.Add(stringData);
                }

                stringTable.Children = stringTableChildren.ToArray();

                stringFileInfoChildren.Add(stringTable);
            }

            stringFileInfo.Children = stringFileInfoChildren.ToArray();

            return stringFileInfo;
        }

        /// <summary>
        ///  Read byte data as a var file info resource
        /// </summary>
        /// <param name="data">Data to parse into a var file info</param>
        /// <param name="offset">Offset into the byte array</param>
        /// <returns>A filled var file info resource on success, null on error</returns>
        private static VarFileInfo? AsVarFileInfo(byte[] data, ref int offset)
        {
            var varFileInfo = new VarFileInfo();

            // Cache the initial offset
            int initialOffset = offset;

            varFileInfo.Length = data.ReadUInt16(ref offset);
            varFileInfo.ValueLength = data.ReadUInt16(ref offset);
            varFileInfo.ResourceType = (VersionResourceType)data.ReadUInt16(ref offset);
            varFileInfo.Key = data.ReadString(ref offset, Encoding.Unicode);
            if (varFileInfo.Key != "VarFileInfo")
                return null;

            // Align to the DWORD boundary if we're not at the end
            if (offset != data.Length)
            {
                while ((offset % 4) != 0)
                    varFileInfo.Padding = data.ReadByte(ref offset);
            }

            var varFileInfoChildren = new List<VarData>();
            while ((offset - initialOffset) < varFileInfo.Length)
            {
                var varData = new VarData();

                varData.Length = data.ReadUInt16(ref offset);
                varData.ValueLength = data.ReadUInt16(ref offset);
                varData.ResourceType = (VersionResourceType)data.ReadUInt16(ref offset);
                varData.Key = data.ReadString(ref offset, Encoding.Unicode);
                if (varData.Key != "Translation")
                {
                    offset -= 6 + ((varData.Key?.Length ?? 0 + 1) * 2);
                    return null;
                }

                // Align to the DWORD boundary if we're not at the end
                if (offset != data.Length)
                {
                    while ((offset % 4) != 0)
                        varData.Padding = data.ReadByte(ref offset);
                }

                // Cache the current offset
                int currentOffset = offset;

                var varDataValue = new List<uint>();
                while ((offset - currentOffset) < varData.ValueLength)
                {
                    uint languageAndCodeIdentifierPair = data.ReadUInt32(ref offset);
                    varDataValue.Add(languageAndCodeIdentifierPair);
                }

                varData.Value = varDataValue.ToArray();

                varFileInfoChildren.Add(varData);
            }

            varFileInfo.Children = varFileInfoChildren.ToArray();

            return varFileInfo;
        }

        #endregion
    }
}
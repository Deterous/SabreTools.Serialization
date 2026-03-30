using SabreTools.Data.Models.ZArchive;
using SabreTools.Numerics;
using Xunit;

namespace SabreTools.Data.Extensions.Test
{
    public class ZArchiveExtensionsTests
    {
        [Fact]
        public void EntryAtOffset_Invalid()
        {
            NameTable nt = new NameTable();
            uint offset = 0;
            NameEntry? expected = null;
            short actual = nt.EntryAtOffset(sectorLength);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsFile_False()
        {
            FileDirectoryEntry fde = new FileDirectoryEntry();
            bool expected = false;
            short actual = fde.IsFile();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsDirectory_True()
        {
            FileDirectoryEntry fde = new FileDirectoryEntry();
            bool expected = true;
            short actual = fde.IsDirectory();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetName_Invalid()
        {
            FileDirectoryEntry fde = new FileDirectoryEntry();
            NameTable nt = new NameTable();
            string? expected = null;
            short actual = fde.GetName(nt);
            Assert.Equal(expected, actual);
        }
    }
}

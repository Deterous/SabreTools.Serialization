using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SabreTools.Serialization.Test.Deserializers
{
    public class ListxmlTests
    {
        [Fact]
        public void NullArray_Null()
        {
            byte[]? data = null;
            int offset = 0;
            var deserializer = new Serialization.Deserializers.Listxml();

            var actual = deserializer.Deserialize(data, offset);
            Assert.Null(actual);
        }

        [Fact]
        public void EmptyArray_Null()
        {
            byte[]? data = [];
            int offset = 0;
            var deserializer = new Serialization.Deserializers.Listxml();

            var actual = deserializer.Deserialize(data, offset);
            Assert.Null(actual);
        }

        [Fact]
        public void InvalidArray_Null()
        {
            byte[]? data = [.. Enumerable.Repeat<byte>(0xFF, 1024)];
            int offset = 0;
            var deserializer = new Serialization.Deserializers.Listxml();

            var actual = deserializer.Deserialize(data, offset);
            Assert.Null(actual);
        }

        [Fact]
        public void NullStream_Null()
        {
            Stream? data = null;
            var deserializer = new Serialization.Deserializers.Listxml();

            var actual = deserializer.Deserialize(data);
            Assert.Null(actual);
        }

        [Fact]
        public void EmptyStream_Null()
        {
            Stream? data = new MemoryStream([]);
            var deserializer = new Serialization.Deserializers.Listxml();

            var actual = deserializer.Deserialize(data);
            Assert.Null(actual);
        }

        [Fact]
        public void InvalidStream_Null()
        {
            Stream? data = new MemoryStream([.. Enumerable.Repeat<byte>(0xFF, 1024)]);
            var deserializer = new Serialization.Deserializers.Listxml();

            var actual = deserializer.Deserialize(data);
            Assert.Null(actual);
        }

        [Theory]
        [InlineData("test-listxml-files1.xml.gz", 45861)]
        [InlineData("test-listxml-files2.xml", 3998)]
        public void ValidFile_NonNull(string path, long count)
        {
            // Open the file for reading
            string filename = Path.Combine(Environment.CurrentDirectory, "TestData", path);

            // Deserialize the file
            var dat = Serialization.Deserializers.Listxml.DeserializeFile(filename);

            // Validate the values
            Assert.NotNull(dat?.Game);
            Assert.Equal(count, dat.Game.Length);
        }
    }
}
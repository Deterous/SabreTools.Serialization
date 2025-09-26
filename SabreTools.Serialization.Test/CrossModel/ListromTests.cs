using SabreTools.Serialization.CrossModel;
using Xunit;

namespace SabreTools.Serialization.Test.CrossModel
{
    public class ListromTests
    {
        [Fact]
        public void RoundTripTest()
        {
            // Get the cross-model serializer
            var serializer = new Listrom();

            // Build the data
            Models.Listrom.MetadataFile mf = Build();

            // Serialize to generic model
            Models.Metadata.MetadataFile? metadata = serializer.Serialize(mf);
            Assert.NotNull(metadata);

            // Serialize back to original model
            Models.Listrom.MetadataFile? newMf = serializer.Deserialize(metadata);

            // Validate the data
            Assert.NotNull(newMf);
            Assert.NotNull(newMf.Set);
            Assert.Equal(2, newMf.Set.Length);

            ValidateDevice(newMf.Set[0]);
            ValidateDriver(newMf.Set[1]);
        }

        /// <summary>
        /// Build model for serialization and deserialization
        /// </summary>
        private static Models.Listrom.MetadataFile Build()
        {
            var rom = new Models.Listrom.Row
            {
                Name = "XXXXXX",
                Size = "XXXXXX",
                Bad = true,
                CRC = "XXXXXX",
                SHA1 = "XXXXXX",
                NoGoodDumpKnown = false,
            };

            var disk = new Models.Listrom.Row
            {
                Name = "XXXXXX",
                Bad = false,
                MD5 = "XXXXXX",
                SHA1 = "XXXXXX",
                NoGoodDumpKnown = true,
            };

            var device = new Models.Listrom.Set()
            {
                Device = "XXXXXX",
                Row = [rom],
            };

            var driver = new Models.Listrom.Set()
            {
                Driver = "XXXXXX",
                Row = [disk],
            };

            return new Models.Listrom.MetadataFile
            {
                Set = [device, driver],
            };
        }

        /// <summary>
        /// Validate a Set
        /// </summary>
        private static void ValidateDevice(Models.Listrom.Set? set)
        {
            Assert.NotNull(set);
            Assert.Equal("XXXXXX", set.Device);

            Assert.NotNull(set.Row);
            var row = Assert.Single(set.Row);
            ValidateRom(row);
        }

        /// <summary>
        /// Validate a Set
        /// </summary>
        private static void ValidateDriver(Models.Listrom.Set? set)
        {
            Assert.NotNull(set);
            Assert.Equal("XXXXXX", set.Driver);

            Assert.NotNull(set.Row);
            var row = Assert.Single(set.Row);
            ValidateDisk(row);
        }

        /// <summary>
        /// Validate a Row
        /// </summary>
        private static void ValidateRom(Models.Listrom.Row? row)
        {
            Assert.NotNull(row);
            Assert.Equal("XXXXXX", row.Name);
            Assert.Equal("XXXXXX", row.Size);
            Assert.True(row.Bad);
            Assert.Equal("XXXXXX", row.CRC);
            Assert.Equal("XXXXXX", row.SHA1);
            Assert.False(row.NoGoodDumpKnown);
        }

        /// <summary>
        /// Validate a Row
        /// </summary>
        private static void ValidateDisk(Models.Listrom.Row? row)
        {
            Assert.NotNull(row);
            Assert.Equal("XXXXXX", row.Name);
            Assert.False(row.Bad);
            Assert.Equal("XXXXXX", row.MD5);
            Assert.Equal("XXXXXX", row.SHA1);
            Assert.True(row.NoGoodDumpKnown);
        }
    }
}
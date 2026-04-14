using System.IO;
using Xunit;

namespace SabreTools.Serialization.Writers.Test
{
    public class XDVDFSTests
    {
        [Fact]
        public void SerializeArray_Null_Null()
        {
            var serializer = new XDVDFS();
            byte[]? actual = serializer.SerializeArray(null);
            Assert.Null(actual);
        }

        [Fact]
        public void SerializeStream_Null_Null()
        {
            var serializer = new XDVDFS();
            Stream? actual = serializer.SerializeStream(null);
            Assert.Null(actual);
        }
    }
}

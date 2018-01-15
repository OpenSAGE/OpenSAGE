using OpenSage.Data.Tga;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.Tga
{
    public class TgaFileTests
    {
        private readonly ITestOutputHelper _output;

        public TgaFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanReadTgaTextures()
        {
            InstalledFilesTestData.ReadFiles(".tga", _output, entry =>
            {
                var tgaFile = TgaFile.FromFileSystemEntry(entry);

                var imagePixelSize = tgaFile.Header.ImagePixelSize;
                Assert.True(imagePixelSize == 8 || imagePixelSize == 16 || imagePixelSize == 24 || imagePixelSize == 32);
            });
        }
    }
}

using OpenSage.Data.Tga;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Data.Tests.Tga
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

                Assert.True(tgaFile.Header.ImagePixelSize == 24 || tgaFile.Header.ImagePixelSize == 32);
            });
        }
    }
}

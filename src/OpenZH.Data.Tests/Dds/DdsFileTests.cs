using OpenZH.Data.Dds;
using Xunit;
using Xunit.Abstractions;

namespace OpenZH.Data.Tests.Dds
{
    public class DdsFileTests
    {
        private readonly ITestOutputHelper _output;

        public DdsFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanReadDdsTextures()
        {
            InstalledFilesTestData.ReadFiles(".dds", _output, (fileName, openFileStream) =>
            {
                using (var fileStream = openFileStream())
                {
                    var ddsFile = DdsFile.FromStream(fileStream);

                    Assert.True(ddsFile.MipMaps.Length > 1);
                }
            });
        }
    }
}

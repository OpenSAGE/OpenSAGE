using OpenSage.Data.Dds;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.Dds
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
            InstalledFilesTestData.ReadFiles(".dds", _output, entry =>
            {
                if (!DdsFile.IsDdsFile(entry))
                {
                    return;
                }

                var ddsFile = DdsFile.FromFileSystemEntry(entry);

                Assert.True(ddsFile.MipMaps.Length > 0);
            });
        }
    }
}

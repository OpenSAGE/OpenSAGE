using OpenSage.Data.Bik;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.Bik
{
    public class BinkFileTests
    {
        private readonly ITestOutputHelper _output;

        public BinkFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanReadBinkVideos()
        {
            InstalledFilesTestData.ReadFiles(".bik", _output, entry =>
            {
                using (var binkFile = BinkFile.FromFileSystemEntry(entry))
                {
                    Assert.NotNull(binkFile);

                    for (var i = 0; i < binkFile.Header.NumFrames; i++)
                    {
                        var frame = binkFile.ReadFrame();
                    }
                }
            });
        }
    }
}

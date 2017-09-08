using OpenSage.Data.Ani;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Data.Tests.Ani
{
    public class AniFileTests
    {
        private readonly ITestOutputHelper _output;

        public AniFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanReadAniCursors()
        {
            InstalledFilesTestData.ReadFiles(".ani", _output, entry =>
            {
                var aniFile = AniFile.FromFileSystemEntry(entry);

                Assert.NotNull(aniFile);

                Assert.True(aniFile.ArtistName == null || aniFile.ArtistName == "PRobb");
            });
        }
    }
}

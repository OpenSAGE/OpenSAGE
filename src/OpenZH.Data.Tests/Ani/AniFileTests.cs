using OpenZH.Data.Ani;
using Xunit;
using Xunit.Abstractions;

namespace OpenZH.Data.Tests.Ani
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
            InstalledFilesTestData.ReadFiles(".ani", _output, (fileName, openFileStream) =>
            {
                AniFile aniFile;
                using (var fileStream = openFileStream())
                    aniFile = AniFile.Parse(fileStream);

                Assert.NotNull(aniFile);

                Assert.True(aniFile.ArtistName == null || aniFile.ArtistName == "PRobb");
            });
        }
    }
}

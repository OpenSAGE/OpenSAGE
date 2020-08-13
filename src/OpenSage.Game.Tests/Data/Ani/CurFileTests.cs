using OpenSage.Data.Ani;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.Ani
{
    public class CurFileTests
    {
        private readonly ITestOutputHelper _output;

        public CurFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanReadAniCursors()
        {
            InstalledFilesTestData.ReadFiles(".cur", _output, entry =>
            {
                var curFile = CurFile.FromFileSystemEntry(entry);

                Assert.NotNull(curFile);
                Assert.True(curFile.Image.Width > 0);
            });
        }
    }
}

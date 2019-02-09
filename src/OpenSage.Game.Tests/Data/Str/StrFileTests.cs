using OpenSage.Data.Str;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.Str
{
    public class StrFileTests
    {
        private readonly ITestOutputHelper _output;

        public StrFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanReadStrFiles()
        {
            InstalledFilesTestData.ReadFiles(".str", _output, entry =>
            {
                var strFile = StrFile.FromFileSystemEntry(entry);

                Assert.NotNull(strFile);
            });
        }
    }
}

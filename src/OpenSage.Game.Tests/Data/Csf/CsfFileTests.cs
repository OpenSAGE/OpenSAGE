using OpenSage.Data.Csf;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Data.Tests.Csf
{
    public class CsfFileTests
    {
        private readonly ITestOutputHelper _output;

        public CsfFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanReadCsfFiles()
        {
            InstalledFilesTestData.ReadFiles(".csf", _output, entry =>
            {
                var csfFile = CsfFile.FromFileSystemEntry(entry);

                Assert.NotNull(csfFile);
            });
        }
    }
}

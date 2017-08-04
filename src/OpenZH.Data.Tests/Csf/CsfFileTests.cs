using OpenZH.Data.Csf;
using Xunit;
using Xunit.Abstractions;

namespace OpenZH.Data.Tests.Csf
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
            InstalledFilesTestData.ReadFiles(".csf", _output, (fileName, openFileStream) =>
            {
                CsfFile csfFile;
                using (var fileStream = openFileStream())
                    csfFile = CsfFile.Parse(fileStream);

                Assert.NotNull(csfFile);
            });
        }
    }
}

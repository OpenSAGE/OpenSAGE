using OpenZH.Data.Map;
using OpenZH.Data.Scb;
using Xunit;
using Xunit.Abstractions;

namespace OpenZH.Data.Tests.Scb
{
    public class ScbFileTests
    {
        private readonly ITestOutputHelper _output;

        public ScbFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanRoundtripScbFiles()
        {
            InstalledFilesTestData.ReadFiles(".scb", _output, (fileName, openFileStream) =>
            {
                using (var fileStream = openFileStream())
                {
                    TestUtility.DoRoundtripTest(
                        () => MapFile.Decompress(fileStream),
                        stream => ScbFile.FromStream(stream),
                        (scbFile, stream) => scbFile.WriteTo(stream));
                }
            });
        }
    }
}

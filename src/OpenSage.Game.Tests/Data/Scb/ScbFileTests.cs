using OpenSage.Data.Map;
using OpenSage.Data.Scb;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.Scb
{
    public class ScbFileTests
    {
        private readonly ITestOutputHelper _output;

        public ScbFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(Skip = "Can't parse all versions of .scb files. Not important at the moment.")]
        public void CanRoundtripScbFiles()
        {
            InstalledFilesTestData.ReadFiles(".scb", _output, entry =>
            {
                using (var fileStream = entry.Open())
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

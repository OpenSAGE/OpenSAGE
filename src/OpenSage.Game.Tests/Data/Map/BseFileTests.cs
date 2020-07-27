using OpenSage.Data.Map;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.Map
{
    public class BseFileTests
    {
        private readonly ITestOutputHelper _output;

        public BseFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanRoundtripBases()
        {
            InstalledFilesTestData.ReadFiles(".bse", _output, entry =>
            {
                using (var entryStream = entry.Open())
                {
                    TestUtility.DoRoundtripTest(
                        () => MapFile.Decompress(entryStream),
                        stream => MapFile.FromStream(stream),
                        (mapFile, stream) => mapFile.WriteTo(stream));
                }
            });
        }
    }
}

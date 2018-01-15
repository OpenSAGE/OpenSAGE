using OpenSage.Data.StreamFS;
using OpenSage.Data.Tests;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Game.Tests.StreamFS
{
    public class ManifestFileTests
    {
        private readonly ITestOutputHelper _output;

        public ManifestFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [GameFact(SageGame.Cnc3, SageGame.Cnc3KanesWrath)]
        public void CanReadManifestFiles()
        {
            InstalledFilesTestData.ReadFiles(".manifest", _output, entry =>
            {
                var gameStream = new GameStream(entry);

                Assert.NotNull(gameStream);
            });
        }
    }
}

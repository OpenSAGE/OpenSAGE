using OpenSage.Data.StreamFS;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.StreamFS
{
    public class ManifestFileTests
    {
        private readonly ITestOutputHelper _output;

        public ManifestFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [GameFact(SageGame.Cnc3, SageGame.Cnc3KanesWrath, SageGame.Ra3, SageGame.Ra3Uprising, SageGame.Cnc4)]
        public void CanReadManifestFiles()
        {
            InstalledFilesTestData.ReadFiles(".manifest", _output, (game, entry) =>
            {
                var gameStream = new GameStream(entry, game);

                Assert.NotNull(gameStream);
            });
        }
    }
}

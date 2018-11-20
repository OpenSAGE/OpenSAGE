using System.Linq;
using OpenSage.Data;
using OpenSage.Mods.Generals;
using OpenSage.Tests.Data;
using Veldrid;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Content
{
    public class LoadMapsTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public LoadMapsTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [GameFact(SageGame.CncGenerals, Skip = "Can take up to 30 minutes to run")]
        public void LoadGeneralsMaps()
        {
            var rootFolder = InstalledFilesTestData.GetInstallationDirectory(SageGame.CncGenerals);
            var installation = new GameInstallation(new GeneralsDefinition(), rootFolder);
            var fileSystem = installation.CreateFileSystem();

            var maps = fileSystem.GetFiles("maps").Where(x => x.FilePath.EndsWith(".map")).ToList();

            Platform.Start();

            using (var window = new GameWindow("OpenSAGE test runner", 100, 100, 800, 600, GraphicsBackend.Direct3D11))
            {
                using (var game = GameFactory.CreateGame(installation, fileSystem, GamePanel.FromGameWindow(window)))
                {
                    foreach (var map in maps)
                    {
                        _testOutputHelper.WriteLine($"Loading {map.FilePath}...");

                        var scene = game.ContentManager.Load<Scene3D>(map.FilePath);
                        Assert.NotNull(scene);

                        game.ContentManager.Unload();
                    }
                }
            }

            Platform.Stop();
        }
    }
}

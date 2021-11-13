using System;
using OpenSage.Data;
using OpenSage.Mods.Generals;
using OpenSage.Tests.Data;

namespace OpenSage.Tests
{
    public class GameFixture : IDisposable
    {
        public readonly Game Game;

        public GameFixture()
        {
            var rootFolder = InstalledFilesTestData.GetInstallationDirectory(SageGame.CncGenerals);
            var installation = new GameInstallation(new GeneralsDefinition(), rootFolder);

            Platform.Start();

            Game = new Game(installation);
        }

        public void Dispose()
        {
            Game.Dispose();

            Platform.Stop();
        }
    }
}

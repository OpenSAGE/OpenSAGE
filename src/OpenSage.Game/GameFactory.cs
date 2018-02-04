using System;
using System.Linq;
using OpenSage.Data;
using Veldrid;

namespace OpenSage
{
    public static class GameFactory
    {
        public static Game CreateGame(IGameDefinition definition,
            IInstallationLocator installationLocator,
            Func<GameWindow> createWindow)
        {
            var installation = installationLocator
                .FindInstallations(definition)
                .FirstOrDefault();

            if (installation == null)
            {
                throw new Exception($"No installations for {definition.Game} could be found.");
            }

            return CreateGame(
                installation,
                installation.CreateFileSystem(),
                createWindow);
        }

        public static Game CreateGame(
            GameInstallation installation,
            FileSystem fileSystem,
            Func<GameWindow> createWindow)
        {
            var definition = installation.Game;

            return new Game(
                fileSystem,
                definition.Game,
                definition.WndCallbackType,
                createWindow);
        }
    }
}

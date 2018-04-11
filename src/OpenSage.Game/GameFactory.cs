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
            Func<GameWindow> createWindow,
            GraphicsBackend? preferredBackend = null)
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
                createWindow,
                preferredBackend);
        }

        public static Game CreateGame(
            GameInstallation installation,
            FileSystem fileSystem,
            Func<GameWindow> createWindow,
            GraphicsBackend? preferredBackend = null)
        {
            var definition = installation.Game;
            return new Game(
                definition, 
                fileSystem, 
                createWindow,
                preferredBackend);
        }
    }
}

using System;
using System.Linq;
using OpenSage.Data;
using OpenSage.LowLevel;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage
{
    public static class GameFactory
    {
        public static Game CreateGame(IGameDefinition definition,
            IInstallationLocator installationLocator,
            GraphicsDevice graphicsDevice = null)
        {
            var installation = installationLocator
                .FindInstallations(definition)
                .FirstOrDefault();

            if (installation == null)
            {
                throw new Exception($"No installations for {definition.Game} could be found.");
            }

            return CreateGame(installation, graphicsDevice);
        }

        public static Game CreateGame(GameInstallation installation,
            GraphicsDevice graphicsDevice = null)
        {
            var fileSystem = installation.CreateFileSystem();

            if (graphicsDevice == null)
            {
                graphicsDevice = HostPlatform.GraphicsDevice;
            }

            var definition = installation.Game;

            return new Game(
                graphicsDevice,
                fileSystem,
                definition.Game,
                definition.WndCallbackType);
        }
    }
}

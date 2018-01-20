using System;
using System.Linq;
using OpenSage.Data;
using OpenSage.LowLevel;
using OpenSage.LowLevel.Graphics2D;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage
{
    public static class GameFactory
    {
        public static Game CreateGame(IGameDefinition definition,
            IInstallationLocator installationLocator,
            GraphicsDevice graphicsDevice = null,
            GraphicsDevice2D graphicsDevice2D = null)
        {
            var installation = installationLocator
                .FindInstallations(definition)
                .FirstOrDefault();

            if (installation == null)
            {
                throw new Exception($"No installations for {definition.Game} could be found.");
            }

            return CreateGame(installation, graphicsDevice, graphicsDevice2D);
        }

        public static Game CreateGame(GameInstallation installation,
            GraphicsDevice graphicsDevice = null,
            GraphicsDevice2D graphicsDevice2D = null)
        {
            var fileSystem = installation.CreateFileSystem();

            if (graphicsDevice == null)
            {
                graphicsDevice = HostPlatform.GraphicsDevice;
            }

            if (graphicsDevice2D == null)
            {
                graphicsDevice2D = HostPlatform.GraphicsDevice2D;
            }

            var definition = installation.Game;

            return new Game(graphicsDevice, graphicsDevice2D, fileSystem, definition.Game, definition.WndCallbackType);
        }
    }
}

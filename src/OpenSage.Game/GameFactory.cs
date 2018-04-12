using System;
using System.Linq;
using OpenSage.Data;

namespace OpenSage
{
    public static class GameFactory
    {
        public static Game CreateGame(
            IGameDefinition definition,
            GamePanel panel)
        {
            var installation = GameInstallation
                .FindAll(new[] { definition })
                .FirstOrDefault();

            if (installation == null)
            {
                throw new Exception($"No installations for {definition.Game} could be found.");
            }

            return CreateGame(
                installation, 
                installation.CreateFileSystem(), 
                panel);
        }

        public static Game CreateGame(
            GameInstallation installation,
            FileSystem fileSystem,
            GamePanel panel)
        {
            var definition = installation.Game;
            return new Game(
                definition, 
                fileSystem, 
                panel);
        }
    }
}

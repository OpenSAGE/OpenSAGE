using System.IO;
using OpenSage.Network;

namespace OpenSage.Data.Sav
{
    internal static class GameStateMap
    {
        internal static void Load(SaveFileReader reader, Game game)
        {
            reader.ReadVersion(2);

            var mapPath1 = reader.ReadAsciiString();
            var mapPath2 = reader.ReadAsciiString();

            var gameType = GameType.SinglePlayer;
            reader.ReadEnum(ref gameType);

            var mapSize = reader.BeginSegment("EmbeddedMap");

            if (reader.SageGame >= SageGame.Bfme)
            {
                var unknown4 = reader.ReadUInt32();
                var unknown5 = reader.ReadUInt32();

                mapSize -= 8;
            }

            // TODO: Delete this temporary map when ending the game.
            var mapPathInSaveFolder = Path.Combine(
                game.ContentManager.UserDataFileSystem.RootDirectory,
                mapPath1);
            var saveFolder = Path.GetDirectoryName(mapPathInSaveFolder);
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }

            using (var mapOutputStream = File.OpenWrite(mapPathInSaveFolder))
            {
                reader.ReadBytesIntoStream(mapOutputStream, (int)mapSize);
            }

            reader.EndSegment();

            var nextObjectId = reader.ReadUInt32();
            var nextDrawableId = reader.ReadUInt32();

            if (reader.SageGame >= SageGame.Bfme)
            {
                var unknown6 = reader.ReadBoolean();
            }

            if (gameType == GameType.Skirmish)
            {
                game.SkirmishManager = new LocalSkirmishManager(game);
                game.SkirmishManager.Settings.Load(reader);

                game.SkirmishManager.Settings.MapName = mapPath1;

                game.SkirmishManager.HandleStartButtonClickAsync().Wait();

                game.SkirmishManager.StartGame();
            }
            else
            {
                game.StartSinglePlayerGame(mapPath1);
            }
        }
    }
}

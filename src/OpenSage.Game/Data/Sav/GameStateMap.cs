using System.IO;
using OpenSage.Network;

namespace OpenSage.Data.Sav
{
    internal static class GameStateMap
    {
        internal static void Load(StatePersister reader, Game game)
        {
            reader.PersistVersion(2);

            var mapPath1 = "";
            reader.PersistAsciiString(ref mapPath1);

            var mapPath2 = "";
            reader.PersistAsciiString(ref mapPath2);

            var gameType = GameType.SinglePlayer;
            reader.PersistEnum(ref gameType);

            var mapSize = reader.BeginSegment("EmbeddedMap");

            if (reader.SageGame >= SageGame.Bfme)
            {
                var unknown4 = 0u;
                reader.PersistUInt32(ref unknown4);

                var unknown5 = 0u;
                reader.PersistUInt32(ref unknown5);

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

            var nextObjectId = 0u;
            reader.PersistUInt32(ref nextObjectId);

            var nextDrawableId = 0u;
            reader.PersistUInt32(ref nextDrawableId);

            if (reader.SageGame >= SageGame.Bfme)
            {
                var unknown6 = false;
                reader.PersistBoolean(ref unknown6);
            }

            if (reader.Mode == StatePersistMode.Read)
            {
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
}

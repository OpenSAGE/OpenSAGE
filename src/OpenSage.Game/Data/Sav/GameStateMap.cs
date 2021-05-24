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
            var gameType = reader.ReadEnum<GameType>();

            var mapSize = reader.BeginSegment();

            // TODO: Delete this temporary map when ending the game.
            var mapPathInSaveFolder = Path.Combine(
                game.ContentManager.UserMapsFileSystem.RootDirectory,
                mapPath1);
            using (var mapOutputStream = File.OpenWrite(mapPathInSaveFolder))
            {
                reader.ReadBytesIntoStream(mapOutputStream, (int)mapSize);
            }

            reader.EndSegment();

            var unknown2 = reader.ReadUInt32(); // 586
            var unknown3 = reader.ReadUInt32(); // 3220

            if (gameType == GameType.Skirmish)
            {
                game.SkirmishManager = new LocalSkirmishManager(game);
                game.SkirmishManager.Settings.Load(reader);

                game.SkirmishManager.Settings.MapName = mapPath1;

                game.SkirmishManager.StartGame();
            }
            else
            {
                game.StartSinglePlayerGame(mapPath1);
            }
        }
    }
}

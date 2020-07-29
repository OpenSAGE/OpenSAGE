using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;
using OpenSage.Scripting;

namespace OpenSage.Data.Map
{
    internal class UserMapCache
    {
        private const string MapCacheIniPath = @"Maps\MapCache.ini";

        private readonly ContentManager _contentManager;

        public UserMapCache(ContentManager contentManager)
        {
            _contentManager = contentManager;
        }
        
        internal void Initialize(AssetStore assetStore)
        {
            // We need to check if there is an existing MapCache.ini file and if yes,
            // load it so that we can figure out which entries are outdated.
            // Then we need to update these entries, update the file, and parse it
            // again. In order to do that we first load it in a temporary scope.
            assetStore.PushScope();

            var mapCacheIniEntry = _contentManager.UserDataFileSystem.GetFile(MapCacheIniPath);
            if (mapCacheIniEntry != null)
            {
                _contentManager.LoadIniFile(mapCacheIniEntry);
            }

            // for each map, check if it was modified and the MapCache needs to be updated.
            var mapCacheEntries = new Dictionary<string, MapCache>();

            foreach (var mapEntry in EnumerateMaps())
            {
                var buildMapCache = false;
                var mapCache = assetStore.MapCaches.GetByName(mapEntry.FullFilePath);
                var fileInfo = new FileInfo(mapEntry.FullFilePath);

                if (mapCache == null)
                {
                    // new map
                    buildMapCache = true;
                }
                else
                {
                    var timestamp = DateTime.FromFileTime((((long)mapCache.TimestampHi) << 32) | (uint)mapCache.TimestampLo);

                    // TODO: Should we check the CRC here as well?
                    // If yes, which implementation should we use?
                    if (fileInfo.LastWriteTime != timestamp &&
                        fileInfo.Length != mapCache.FileSize)
                    {
                        // existing map modified
                        buildMapCache = true;
                    }
                }

                if (buildMapCache)
                {
                    mapCache = BuildMapCache(mapEntry, fileInfo, assetStore);
                }

                mapCacheEntries.Add(mapEntry.FullFilePath, mapCache);
            }

            // Get rid of the old MapCaches, generate the file based on
            // the updated ones and load it again, this time for real.
            assetStore.PopScope();

            var fullMapCacheIniPath = Path.Combine(_contentManager.UserDataFileSystem.RootDirectory, MapCacheIniPath);

            // Create the full path, user directory should already exist from the content manager but
            // maps folder may not
            var mapsPath = Path.GetDirectoryName(fullMapCacheIniPath);

            if (!Directory.Exists(mapsPath))
                Directory.CreateDirectory(mapsPath);

            GenerateMapCacheIniFile(fullMapCacheIniPath, mapCacheEntries);

            mapCacheIniEntry = new FileSystemEntry(
                _contentManager.UserDataFileSystem,
                MapCacheIniPath,
                (uint)new FileInfo(fullMapCacheIniPath).Length,
                () => File.OpenRead(fullMapCacheIniPath));

            _contentManager.UserDataFileSystem.Update(mapCacheIniEntry);
            _contentManager.LoadIniFile(mapCacheIniEntry);
        }

        private IEnumerable<FileSystemEntry> EnumerateMaps()
        {
            var maps = from file in _contentManager.UserDataFileSystem.Files
                       where Path.GetExtension(file.FilePath) == ".map"
                       let parts = file.FilePath.Split(Path.DirectorySeparatorChar)
                       where parts.Length == 3
                           && parts[0] == "maps"
                           && parts[1] == Path.GetFileNameWithoutExtension(parts[2])
                       select file;
            return maps;
        }

        private MapCache BuildMapCache(FileSystemEntry fileSystemEntry, FileInfo fileInfo, AssetStore assetStore)
        {
            var timestamp = fileInfo.LastWriteTime.ToFileTime();

            var mapCache = new MapCache()
            {
                FileSize = (int) fileInfo.Length,
                TimestampLo = (int) timestamp,
                TimestampHi = (int) (timestamp >> 32),
                IsOfficial = false,
                NameLookupTag = null
            };

            var mapFile = MapFile.FromFileSystemEntry(fileSystemEntry);

            var border = mapFile.HeightMapData.Borders[0];
            mapCache.ExtentMin = new Vector3(border.Corner1X, border.Corner1Y, 0f);
            mapCache.ExtentMax = new Vector3(border.Corner2X, border.Corner2Y, 0f);

            byte startingPositionsFound = 0;
            foreach (var mapObject in mapFile.ObjectsList.Objects)
            {
                // ignore roads
                if ((mapObject.RoadType & RoadType.PrimaryType) == RoadType.None)
                {
                    // handle special waypoints
                    if (mapObject.TypeName == Waypoint.ObjectTypeName)
                    {
                        var waypointName = (string) mapObject.Properties["waypointName"].Value;
                        switch (waypointName)
                        {
                            case "InitialCameraPosition":
                                mapCache.InitialCameraPosition = mapObject.Position;
                                break;
                            case "Player_1_Start":
                                mapCache.Player1Start = mapObject.Position;
                                startingPositionsFound |= 1 << 0;
                                break;
                            case "Player_2_Start":
                                mapCache.Player2Start = mapObject.Position;
                                startingPositionsFound |= 1 << 1;
                                break;
                            case "Player_3_Start":
                                mapCache.Player3Start = mapObject.Position;
                                startingPositionsFound |= 1 << 2;
                                break;
                            case "Player_4_Start":
                                mapCache.Player4Start = mapObject.Position;
                                startingPositionsFound |= 1 << 3;
                                break;
                            case "Player_5_Start":
                                mapCache.Player5Start = mapObject.Position;
                                startingPositionsFound |= 1 << 4;
                                break;
                            case "Player_6_Start":
                                mapCache.Player6Start = mapObject.Position;
                                startingPositionsFound |= 1 << 5;
                                break;
                            case "Player_7_Start":
                                mapCache.Player7Start = mapObject.Position;
                                startingPositionsFound |= 1 << 6;
                                break;
                            case "Player_8_Start":
                                mapCache.Player8Start = mapObject.Position;
                                startingPositionsFound |= 1 << 7;
                                break;
                        }
                    }
                    else
                    {
                        // check "normal" objects
                        var definition = assetStore.ObjectDefinitions.GetByName(mapObject.TypeName);
                        if (definition.KindOf.Get(ObjectKinds.SupplySourceOnPreview))
                        {
                            mapCache.SupplyPositions.Add(mapObject.Position);
                        }

                        if (definition.KindOf.Get(ObjectKinds.TechBuilding))
                        {
                            mapCache.TechPositions.Add(mapObject.Position);
                        }
                    }
                }
            }

            // Only consecutive numbers are recognized, so if there are
            // starting positions 1, 2 and 4, the game treats it as a
            // 2 player map.
            // There is always at least one player though, even if there
            // are no starting positions (single player maps).
            mapCache.NumPlayers = 1;
            while ((startingPositionsFound & (1 << mapCache.NumPlayers)) > 0)
            {
                mapCache.NumPlayers++;
            }

            mapCache.IsMultiplayer = mapCache.NumPlayers > 1;

            // TODO
            // mapCache.DisplayName
            // mapCache.Description
            // mapCache.FileCrc
            // mapCache.IsScenarioMP
            // mapCache.PlayerPositions

            return mapCache;
        }

        private void GenerateMapCacheIniFile(string path, IReadOnlyDictionary<string, MapCache> mapCacheEntries)
        {
            using (var writer = new StreamWriter(File.Create(path)))
            {
                writer.WriteLine($"; FILE: {path} /////////////////////////////////////////////////////////////");
                writer.WriteLine("; This INI file is auto-generated - do not modify");
                writer.WriteLine("; /////////////////////////////////////////////////////////////////////////////");

                foreach (var entry in mapCacheEntries)
                {
                    var mapCache = entry.Value;
                    writer.WriteLine();
                    writer.Write("MapCache ");
                    WriteEncodedPath(entry.Key);
                    Write("fileSize", mapCache.FileSize);
                    Write("fileCRC", mapCache.FileCrc);
                    Write("timestampLo", mapCache.TimestampLo);
                    Write("timestampHi", mapCache.TimestampHi);
                    WriteBoolean("isOfficial", mapCache.IsOfficial);
                    WriteBoolean("isMultiplayer", mapCache.IsMultiplayer);
                    Write("numPlayers", mapCache.NumPlayers);
                    WriteVector("extentMin", mapCache.ExtentMin);
                    WriteVector("extentMax", mapCache.ExtentMax);
                    Write("nameLookupTag", mapCache.NameLookupTag);
                    WriteVector("InitialCameraPosition", mapCache.InitialCameraPosition);

                    if (mapCache.Player1Start != Vector3.Zero)
                        WriteVector("Player_1_Start", mapCache.Player1Start);
                    if (mapCache.Player2Start != Vector3.Zero)
                        WriteVector("Player_2_Start", mapCache.Player2Start);
                    if (mapCache.Player3Start != Vector3.Zero)
                        WriteVector("Player_3_Start", mapCache.Player3Start);
                    if (mapCache.Player4Start != Vector3.Zero)
                        WriteVector("Player_4_Start", mapCache.Player4Start);
                    if (mapCache.Player5Start != Vector3.Zero)
                        WriteVector("Player_5_Start", mapCache.Player5Start);
                    if (mapCache.Player6Start != Vector3.Zero)
                        WriteVector("Player_6_Start", mapCache.Player6Start);
                    if (mapCache.Player7Start != Vector3.Zero)
                        WriteVector("Player_7_Start", mapCache.Player7Start);
                    if (mapCache.Player8Start != Vector3.Zero)
                        WriteVector("Player_8_Start", mapCache.Player8Start);

                    foreach (var techPosition in mapCache.TechPositions)
                        WriteVector("techPosition", techPosition);

                    foreach (var supplyPosition in mapCache.SupplyPositions)
                        WriteVector("supplyPosition", supplyPosition);

                    writer.WriteLine("END");
                    writer.WriteLine();
                }

                void WriteEncodedPath(string path)
                {
                    for (int i = 0; i < path.Length; i++)
                    {
                        switch (path[i])
                        {
                            case char l when l >= 'a' && l <= 'z':
                            case char u when u >= 'A' && u <= 'Z':
                            case char d when d >= '0' && d <= '9':
                                writer.Write(path[i]);
                                break;
                            default:
                                writer.Write('_');
                                writer.Write(((short) path[i]).ToString("X2"));
                                break;
                        }
                    }

                    writer.WriteLine();
                }

                void Write(string key, object value)
                {
                    writer.Write("  ");
                    writer.Write(key);
                    writer.Write(" = ");
                    writer.WriteLine(value);
                }

                void WriteVector(string key, in Vector3 vector)
                {
                    Write(key, $"X:{vector.X:F2} Y:{vector.Y:F2} Z:{vector.Z:F2}");
                }

                void WriteBoolean(string key, bool value)
                {
                    Write(key, value ? "yes" : "no");
                }
            }
        }
    }
}

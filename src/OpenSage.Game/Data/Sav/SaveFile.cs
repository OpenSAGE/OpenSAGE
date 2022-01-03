using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenSage.FileFormats;
using OpenSage.Graphics.Cameras;
using OpenSage.IO;

namespace OpenSage.Data.Sav
{
    public static class SaveFile
    {
        public static GameState GetGameState(FileSystemEntry entry, Game game)
        {
            using (var stream = entry.Open())
            {
                var reader = new StateReader(stream, game);

                while (true)
                {
                    var chunkName = "";
                    reader.PersistAsciiString(ref chunkName);

                    reader.BeginSegment(chunkName);

                    if (chunkName == "CHUNK_GameState")
                    {
                        var gameState = new GameState();
                        gameState.Load(reader);
                        return gameState;
                    }

                    reader.EndSegment();
                }
            }

            throw new InvalidStateException();
        }

        public static void Load(FileSystemEntry entry, Game game)
        {
            using var stream = entry.Open();
            LoadFromStream(stream, game);
        }

        public static void LoadFromStream(Stream stream, Game game)
        {
            using var statePersister = new StateReader(stream, game);

            Persist(statePersister);
        }

        private record struct ChunkDefinition(string ChunkName, Action<StatePersister, Game> PersistCallback);

        private static readonly List<ChunkDefinition> ChunkDefinitions = new List<ChunkDefinition>
        {
            new ChunkDefinition("CHUNK_GameState", (persister, game) => game.GameState.Load(persister)),
            new ChunkDefinition("CHUNK_Campaign", (persister, game) => game.CampaignManager.Load(persister)),
            new ChunkDefinition("CHUNK_GameStateMap", (persister, game) => game.GameStateMap.Load(persister, game)),
            new ChunkDefinition("CHUNK_TerrainLogic", (persister, game) => game.TerrainLogic.Load(persister)),
            new ChunkDefinition("CHUNK_TeamFactory", (persister, game) => game.Scene3D.TeamFactory.Load(persister, game.Scene3D.PlayerManager)),
            new ChunkDefinition("CHUNK_Players", (persister, game) => game.Scene3D.PlayerManager.Load(persister, game)),
            new ChunkDefinition("CHUNK_GameLogic", (persister, game) => game.Scene3D.GameLogic.Load(persister)),
            new ChunkDefinition("CHUNK_ParticleSystem", (persister, game) => game.Scene3D.ParticleSystemManager.Load(persister)),
            new ChunkDefinition("CHUNK_Radar", (persister, game) => game.Scene3D.Radar.Load(persister)),
            new ChunkDefinition("CHUNK_ScriptEngine", (persister, game) => game.Scripting.Load(persister)),
            new ChunkDefinition("CHUNK_SidesList", (persister, game) => game.Scene3D.PlayerScripts.Load(persister)),
            new ChunkDefinition("CHUNK_TacticalView", (persister, game) => ((RtsCameraController) game.Scene3D.CameraController).Load(persister)),
            new ChunkDefinition("CHUNK_GameClient", (persister, game) => game.Scene3D.GameClient.Load(persister)),
            new ChunkDefinition("CHUNK_InGameUI", (persister, game) => game.AssetStore.InGameUI.Current.Load(persister)),
            new ChunkDefinition("CHUNK_Partition", (persister, game) => game.Scene3D.PartitionCellManager.Load(persister)),
            new ChunkDefinition("CHUNK_TerrainVisual", (persister, game) => game.TerrainVisual.Load(persister, game)),
            new ChunkDefinition("CHUNK_GhostObject", (persister, game) => game.GhostObjectManager.Load(persister, game.Scene3D.GameLogic, game)),

        };

        public static void Persist(StatePersister persister)
        {
            //using var binaryReader = new BinaryReader(stream, Encoding.Unicode, true);

            //if (game.SageGame >= SageGame.Bfme)
            //{
            //    var header1 = binaryReader.ReadFourCc(bigEndian: true);
            //    if (header1 != "EALA")
            //    {
            //        throw new InvalidStateException();
            //    }

            //    var header2 = binaryReader.ReadFourCc(bigEndian: true);
            //    if (header2 != "RTS1")
            //    {
            //        throw new InvalidStateException();
            //    }

            //    var header3 = binaryReader.ReadUInt32();
            //    if (header3 != 0)
            //    {
            //        throw new InvalidStateException();
            //    }
            //}

            if (persister.Mode == StatePersistMode.Read)
            {
                while (true)
                {
                    var chunkName = "";
                    persister.PersistAsciiString(ref chunkName);

                    if (chunkName == "SG_EOF")
                    {
                        //if (stream.Position != stream.Length)
                        //{
                        //    throw new InvalidStateException();
                        //}
                        break;
                    }

                    var chunkLength = persister.BeginSegment(chunkName);

                    var chunkDefinition = ChunkDefinitions.Find(x => x.ChunkName == chunkName);

                    if (chunkDefinition == default)
                    {
                        throw new InvalidDataException($"Unknown chunk type '{chunkName}'.");
                    }

                    chunkDefinition.PersistCallback(persister, persister.Game);

                    persister.EndSegment();
                }

                // If we haven't started a game yet (which will be the case for
                // "mission start" save files), then start it now.
                if (!persister.Game.InGame)
                {
                    persister.Game.StartCampaign(
                        persister.Game.CampaignManager.CampaignName,
                        persister.Game.CampaignManager.MissionName);
                }
            }
            else
            {
                foreach (var chunkDefinition in ChunkDefinitions)
                {
                    var chunkName = chunkDefinition.ChunkName;
                    persister.PersistAsciiString(ref chunkName);

                    persister.BeginSegment(chunkName);

                    chunkDefinition.PersistCallback(persister, persister.Game);

                    persister.EndSegment();
                }

                var endChunkName = "SG_EOF";
                persister.PersistAsciiString(ref endChunkName);
            }
        }
    }

    public enum SaveGameType : uint
    {
        Skirmish,
        SinglePlayer
    }
}

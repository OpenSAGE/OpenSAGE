using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.IO;
using OpenSage.Data.StreamFS;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Content
{
    public interface ISubsystemLoader
    {
        void Load(Subsystem subsystem);
    }

    public class GeneralsSubsystemLoader : ISubsystemLoader
    {
        private readonly ContentManager _contentManager;

        public GeneralsSubsystemLoader(ContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public void Load(Subsystem subsystem)
        {
            switch (subsystem)
            {
                case Subsystem.Core:
                    LoadFiles(
                        "/game/Data/INI/DefaultGameData.ini",
                        "/game/Data/INI/GameData.ini",
                        "/game/Data/INI/Mouse.ini",
                        "/game/Data/INI/Water.ini",
                        "/game/Data/INI/AudioSettings.ini",
                        $"/game/Data/{_contentManager.Language}/HeaderTemplate.ini",
                        "/game/Maps/MapCache.ini");
                    break;
                case Subsystem.Audio:
                    LoadFiles(
                        "/game/Data/INI/AudioSettings.ini",
                        "/game/Data/INI/SoundEffects.ini",
                        "/game/Data/INI/MiscAudio.ini",
                        "/game/Data/INI/Voice.ini");
                    break;
                case Subsystem.ObjectCreation:
                    LoadFiles(
                        "/game/Data/INI/Default/Object.ini",
                        "/game/Data/INI/Locomotor.ini",
                        "/game/Data/INI/Upgrade.ini");
                    _contentManager.LoadIniFiles("/game/Data/INI/Object");
                    break;
                case Subsystem.Players:
                    LoadFiles(
                        "/game/Data/INI/Default/PlayerTemplate.ini",
                        "/game/Data/INI/PlayerTemplate.ini",
                        "/game/Data/INI/ControlBarScheme.ini",
                        "/game/Data/INI/CommandSet.ini",
                        "/game/Data/INI/CommandButton.ini");
                    break;
                case Subsystem.Terrain:
                    LoadFiles(
                        "/game/Data/INI/Default/Terrain.ini",
                        "/game/Data/INI/Terrain.ini",
                        "/game/Data/INI/Default/Roads.ini",
                        "/game/Data/INI/Roads.ini");
                    break;
                case Subsystem.ParticleSystems:
                    LoadFiles("/game/Data/INI/ParticleSystem.ini");
                    break;
                case Subsystem.Wnd:
                    LoadFiles(
                        "/game/Data/INI/WindowTransitions.ini",
                        "/game/Data/INI/ControlBarScheme.ini");
                    _contentManager.LoadIniFiles("/game/Data/INI/MappedImages/HandCreated/");
                    _contentManager.LoadIniFiles("/game/Data/INI/MappedImages/TextureSize_512/");
                    break;
                case Subsystem.Multiplayer:
                    LoadFiles("/game/Data/INI/Multiplayer.ini");
                    break;
                case Subsystem.LinearCampaign:
                    LoadFiles("/game/Data/INI/Campaign.ini");
                    break;
                case Subsystem.Credits:
                    LoadFiles("/game/Data/INI/Credits.ini");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(subsystem), subsystem, null);
            }
        }

        private void LoadFiles(params string[] files)
        {
            foreach (var file in files)
            {
                _contentManager.LoadIniFile(file);
            }
        }
    }

    public class ConfiguredSubsystemLoader : ISubsystemLoader
    {
        private readonly ContentManager _contentManager;
        private readonly Game _game;
        private readonly IGameDefinition _gameDefinition;
        private readonly ScopedAssetCollection<LoadSubsystem> _subsystems;

        public ConfiguredSubsystemLoader(IGameDefinition gameDefinition, Game game, ContentManager contentManager)
        {
            _gameDefinition = gameDefinition;
            _contentManager = contentManager;
            _game = game;

            _contentManager.LoadIniFile("/game/Data/INI/Default/subsystemlegend.ini");
            _subsystems = game.AssetStore.Subsystems;
        }

        public void Load(Subsystem subsystem)
        {
            foreach (var entry in GetFilesForSubsystem(subsystem))
            {
                _contentManager.LoadIniFile(entry);
            }

            // Load hardcoded files
            switch (subsystem)
            {
                case Subsystem.Core:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            _contentManager.LoadIniFile("/game/Data/INI/Mouse.ini");
                            _contentManager.LoadIniFile("/game/Data/INI/Water.ini");
                            _contentManager.LoadIniFile("/game/Maps/MapCache.ini");
                            break;

                        case SageGame.Cnc3:
                            // TODO: Use .version file.
                            // Remark (Qibbi): using the csf file should be done via a asset stream provider, it should only the bases version be loaded (ex "/game/data/global.manifest")
                            var gameStream = new GameStream("/game/Data/global_common.manifest", _game);
                            var gameStream2 = new GameStream("/game/Data/static_common.manifest", _game);
                            break;
                    }
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                            _contentManager.LoadIniFile($"/game/Lang/{_contentManager.Language}/HeaderTemplate.ini");
                            break;

                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            _contentManager.LoadIniFile("/game/HeaderTemplate.ini");
                            break;
                    }
                    break;

                case Subsystem.Players:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            _contentManager.LoadIniFile("/game/Data/INI/ControlBarScheme.ini");
                            _contentManager.LoadIniFile("/game/Data/INI/CommandSet.ini");
                            _contentManager.LoadIniFile("/game/Data/INI/CommandButton.ini");
                            break;
                    }
                    break;

                case Subsystem.Audio:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            _contentManager.LoadIniFile("/game/Data/INI/AudioSettings.ini");
                            _contentManager.LoadIniFile("/game/Data/INI/SoundEffects.ini");
                            _contentManager.LoadIniFile("/game/Data/INI/MiscAudio.ini");
                            _contentManager.LoadIniFile("/game/Data/INI/Voice.ini");
                            break;
                    }
                    break;

                case Subsystem.Wnd:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            _contentManager.LoadIniFiles("/game/Data/INI/MappedImages/HandCreated/");
                            _contentManager.LoadIniFiles("/game/Data/INI/MappedImages/TextureSize_512/");
                            break;
                    }
                    break;
            }
        }

        private IEnumerable<string> GetSubsystemEntryName(Subsystem subsystem)
        {
            // TODO: This should be stored in IGameDefinition
            switch (subsystem)
            {
                case Subsystem.Core:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            yield return "TheWritableGlobalData";
                            yield break;
                        case SageGame.Cnc3:
                        case SageGame.Cnc3KanesWrath:
                            yield return "GameEngine";
                            yield return "TheGameClient";
                            yield return "TheWritableGlobalData";
                            yield break;
                    }
                    break;
                case Subsystem.Audio:
                    yield break;
                case Subsystem.ObjectCreation:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            yield return "TheThingFactory";
                            yield return "TheUpgradeCenter";
                            yield break;
                        // TODO: Figure out how to load object config for C&C3 and later
                        case SageGame.Cnc3:
                        case SageGame.Cnc3KanesWrath:
                            yield break;
                    }
                    break;
                case Subsystem.Players:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                        case SageGame.Cnc3:
                        case SageGame.Cnc3KanesWrath:
                            yield return "ThePlayerTemplateStore";
                            yield break;
                    }
                    break;
                case Subsystem.Terrain:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                        case SageGame.Cnc3:
                        case SageGame.Cnc3KanesWrath:
                            yield return "TheTerrainTypes";
                            yield return "TheTerrainRoads";
                            yield break;
                    }
                    break;
                case Subsystem.ParticleSystems:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            yield return "TheParticleSystemManager";
                            yield return "TheFXParticleSystemManager";
                            yield break;
                        // TODO: Figure out how to load particle systems for C&C3 and later
                        case SageGame.Cnc3:
                        case SageGame.Cnc3KanesWrath:
                            yield break;
                    }
                    break;
                case Subsystem.Wnd:
                    break;
                case Subsystem.Multiplayer:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            yield return "TheMultiplayerSettings";
                            yield break;
                    }
                    break;
                case Subsystem.LinearCampaign:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                        case SageGame.Cnc3:
                        case SageGame.Cnc3KanesWrath:
                            yield return "TheLinearCampaignManager";
                            yield break;
                            // TODO: Figure out how to load campaigns for RA3 and later
                    }
                    break;
                case Subsystem.Credits:
                    yield return "Credits";
                    yield break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(subsystem), subsystem, null);
            }
        }

        private IEnumerable<string> GetFilesForSubsystem(Subsystem abstractSubsystem)
        {
            var subsystems = GetSubsystemEntryName(abstractSubsystem).Select(entryName => _subsystems.GetByName(entryName)).ToList();

            foreach (var subsystem in subsystems)
            {
                foreach (var entry in subsystem.Entries)
                {
                    // TODO: Handle other entries.
                    switch (entry)
                    {
                        case InitFile file:
                            yield return file.Value;
                            break;
                        case InitPath folder:
                            // TODO: Validate that exclusions work.
                            var entries = FileSystem.ListFiles(folder.Value, "*", SearchOption.AllDirectories).WhereNot(c => subsystem.ExcludePath.Contains(c));
                            foreach (var file in entries)
                            {
                                yield return file;
                            }
                            break;
                    }
                }
            }
        }
    }

    public static class SubsystemLoader
    {
        public static ISubsystemLoader Create(IGameDefinition gameDefinition, Game game, ContentManager contentManager)
        {
            switch (gameDefinition.Game)
            {
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                    return new GeneralsSubsystemLoader(contentManager);

                case SageGame.Bfme:
                case SageGame.Bfme2:
                case SageGame.Bfme2Rotwk:
                case SageGame.Cnc3:
                case SageGame.Cnc3KanesWrath:
                    return new ConfiguredSubsystemLoader(gameDefinition, game, contentManager);

                default:
                    // TODO: Implement subsystem loader for new XML-based format used in RA3 and beyond.
                    return null;
            }
        }
    }
}

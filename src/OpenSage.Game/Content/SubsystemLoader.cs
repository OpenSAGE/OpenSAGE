using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data;
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
                        @"Data\INI\Default\GameData.ini",
                        @"Data\INI\GameData.ini",
                        @"Data\INI\Mouse.ini",
                        @"Data\INI\Water.ini",
                        @"Data\INI\AudioSettings.ini",
                        $@"Data\{_contentManager.Language}\HeaderTemplate.ini",
                        @"Maps\MapCache.ini");

                    break;
                case Subsystem.Audio:
                    LoadFiles(
                        @"Data\INI\AudioSettings.ini",
                        @"Data\INI\SoundEffects.ini",
                        @"Data\INI\MiscAudio.ini",
                        @"Data\INI\Voice.ini",
                        @"Data\Ini\Music.ini");
                    break;
                case Subsystem.ObjectCreation:
                    LoadFiles(
                        @"Data\INI\Default\Object.ini",
                        @"Data\INI\Upgrade.ini",
                        @"Data\INI\Crate.ini");
                    _contentManager.LoadIniFiles(@"Data\INI\Object");
                    break;
                case Subsystem.Locomotors:
                    LoadFiles(
                        @"Data\INI\Locomotor.ini");
                    break;
                case Subsystem.Sciences:
                    LoadFiles(
                        @"Data\INI\Science.ini");
                    break;
                case Subsystem.Weapons:
                    LoadFiles(
                        @"Data\INI\Weapon.ini");
                    break;
                case Subsystem.FXList:
                    LoadFiles(
                        @"Data\INI\FXList.ini");
                    break;
                case Subsystem.Players:
                    LoadFiles(
                        @"Data\INI\Default\PlayerTemplate.ini",
                        @"Data\INI\PlayerTemplate.ini",
                        @"Data\INI\ControlBarScheme.ini",
                        @"Data\INI\CommandSet.ini",
                        @"Data\INI\CommandButton.ini");
                    break;
                case Subsystem.Terrain:
                    LoadFiles(
                        @"Data\INI\Default\Terrain.ini",
                        @"Data\INI\Terrain.ini",
                        @"Data\INI\Default\Roads.ini",
                        @"Data\INI\Roads.ini");
                    break;
                case Subsystem.ParticleSystems:
                    LoadFiles(@"Data\INI\ParticleSystem.ini");
                    break;
                case Subsystem.Wnd:
                    LoadFiles(
                        @"Data\INI\WindowTransitions.ini",
                        @"Data\INI\ControlBarScheme.ini");
                    _contentManager.LoadIniFiles(@"Data\INI\MappedImages\HandCreated\");
                    _contentManager.LoadIniFiles(@"Data\INI\MappedImages\TextureSize_512\");
                    break;
                case Subsystem.Multiplayer:
                    LoadFiles(@"Data\INI\Multiplayer.ini");
                    break;
                case Subsystem.LinearCampaign:
                    LoadFiles(@"Data\INI\Campaign.ini");
                    break;
                case Subsystem.Credits:
                    LoadFiles(@"Data\INI\Credits.ini");
                    break;
                case Subsystem.Damage:
                    LoadFiles(@"Data\INI\DamageFX.ini");
                    break;
                case Subsystem.SpecialPower:
                    LoadFiles(@"Data\INI\Default\SpecialPower.ini");
                    LoadFiles(@"Data\INI\SpecialPower.ini");
                    break;
                case Subsystem.InGameUI:
                    LoadFiles(@"Data\INI\InGameUI.ini");
                    break;
                case Subsystem.Rank:
                    LoadFiles(@"Data\INI\Rank.ini");
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
        private readonly FileSystem _fileSystem;
        private readonly ScopedAssetCollection<LoadSubsystem> _subsystems;

        public ConfiguredSubsystemLoader(IGameDefinition gameDefinition, FileSystem fileSystem, Game game, ContentManager contentManager)
        {
            _gameDefinition = gameDefinition;
            _contentManager = contentManager;
            _game = game;
            _fileSystem = fileSystem;

            _contentManager.LoadIniFile(@"Data\INI\Default\subsystemlegend.ini");
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
                            _contentManager.LoadIniFile(@"Data\INI\Mouse.ini");
                            _contentManager.LoadIniFile(@"Data\INI\Water.ini");
                            _contentManager.LoadIniFile(@"Data\INI\WaterTextures.ini");
                            _contentManager.LoadIniFile(@"Maps\MapCache.ini");
                            break;

                        case SageGame.Cnc3:
                            // TODO: Use .version file.
                            var manifestFileEntry = _fileSystem.GetFile(@"Data\global_common.manifest");
                            var gameStream = new GameStream(manifestFileEntry, _game);
                            var manifestFileEntry2 = _fileSystem.GetFile(@"Data\static_common.manifest");
                            var gameStream2 = new GameStream(manifestFileEntry2, _game);
                            break;
                    }
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                            _contentManager.LoadIniFile($@"Lang\{_contentManager.Language}\HeaderTemplate.ini");
                            break;

                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            _contentManager.LoadIniFile($@"HeaderTemplate.ini");
                            break;
                    }
                    break;

                case Subsystem.Players:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            _contentManager.LoadIniFile(@"Data\INI\ControlBarScheme.ini");
                            _contentManager.LoadIniFile(@"Data\INI\CommandSet.ini");
                            _contentManager.LoadIniFile(@"Data\INI\CommandButton.ini");
                            break;
                    }
                    break;

                case Subsystem.Audio:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            _contentManager.LoadIniFile(@"Data\INI\AudioSettings.ini");
                            _contentManager.LoadIniFile(@"Data\INI\SoundEffects.ini");
                            _contentManager.LoadIniFile(@"Data\INI\MiscAudio.ini");
                            _contentManager.LoadIniFile(@"Data\INI\Voice.ini");
                            break;
                    }
                    break;

                case Subsystem.Wnd:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            _contentManager.LoadIniFiles(@"Data\INI\MappedImages\HandCreated\");
                            _contentManager.LoadIniFiles(@"Data\INI\MappedImages\TextureSize_512\");
                            _contentManager.LoadIniFiles(@"Data\INI\MappedImages\AptImages\");
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
                case Subsystem.Locomotors:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                        case SageGame.Cnc3:
                        case SageGame.Cnc3KanesWrath:
                            yield return "TheLocomotorStore";
                            yield break;
                    }
                    break;
                case Subsystem.Sciences:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                        case SageGame.Cnc3:
                        case SageGame.Cnc3KanesWrath:
                            yield return "TheScienceStore";
                            yield break;
                    }
                    break;
                case Subsystem.Weapons:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                        case SageGame.Cnc3:
                        case SageGame.Cnc3KanesWrath:
                            yield return "TheWeaponStore";
                            yield break;
                    }
                    break;
                case Subsystem.FXList:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            yield return "TheFXListStore";
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
                case Subsystem.Damage:
                    yield return "TheDamageFXStore";
                    yield break;
                case Subsystem.SpecialPower:
                    yield return "TheSpecialPowerStore";
                    yield break;
                case Subsystem.InGameUI:
                    yield return "InGameUI";
                    yield break;
                case Subsystem.ExperienceLevels:
                    yield return "TheExperienceLevelSystem";
                    yield break;
                case Subsystem.AttributeModifiers:
                    yield return "TheAttributeModifierStore";
                    yield break;
                case Subsystem.Rank:
                    yield return "TheRankInfoStore";
                    yield break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(subsystem), subsystem, null);
            }
        }

        private IEnumerable<FileSystemEntry> GetFilesForSubsystem(Subsystem abstractSubsystem)
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
                            yield return _fileSystem.GetFile(file.Value);
                            break;
                        case InitPath folder:
                            // TODO: Validate that exclusions work.
                            var entries = _fileSystem.GetFiles(folder.Value).WhereNot(c => subsystem.ExcludePath.Contains(c.FilePath));
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
        public static ISubsystemLoader Create(IGameDefinition gameDefinition, FileSystem fileSystem, Game game, ContentManager contentManager)
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
                    return new ConfiguredSubsystemLoader(gameDefinition, fileSystem, game, contentManager);

                default:
                    // TODO: Implement subsystem loader for new XML-based format used in RA3 and beyond.
                    return null;
            }
        }
    }
}

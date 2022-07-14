using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenSage.IO;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Content
{
    public sealed class SubsystemLoader
    {
        private const string GeneralsSubsystemLegendIni =
            """
            LoadSubsystem TheWritableGlobalData
                Loader = INI
                InitFile = Data\INI\Default\GameData.ini
                InitFile = Data\INI\GameData.ini
            End

            LoadSubsystem TheThingFactory
                Loader = INI
                InitFile = Data\INI\Default\Object.ini
                InitPath = Data\INI\Object
            End

            LoadSubsystem TheUpgradeCenter
                Loader = INI
                InitFile = Data\INI\Default\Upgrade.ini
                InitFile = Data\INI\Upgrade.ini
            End

            LoadSubsystem TheObjectCreationListStore
                Loader = INI
                InitFile = Data\INI\Default\ObjectCreationList.ini
                InitFile = Data\INI\ObjectCreationList.ini
            End

            LoadSubsystem TheCrateSystem
                Loader = INI
                InitFile = Data\INI\Default\Crate.ini
                InitFile = Data\INI\Crate.ini
            End

            LoadSubsystem TheLocomotorStore
                Loader = INI
                InitFile = Data\INI\Locomotor.ini
            End

            LoadSubsystem TheScienceStore
                Loader = INI
                InitFile = Data\INI\Science.ini
            End

            LoadSubsystem TheWeaponStore
                Loader = INI
                InitFile = Data\INI\Weapon.ini
            End

            LoadSubsystem TheFXListStore
                Loader = INI
                InitFile = Data\INI\Default\FXList.ini
                InitFile = Data\INI\FXList.ini
            End

            LoadSubsystem TheTerrainTypes
                Loader = INI
                InitFile = Data\INI\Default\Terrain.ini
                InitFile = Data\INI\Terrain.ini
            End

            LoadSubsystem TheTerrainRoads
                Loader = INI
                InitFile = Data\INI\Default\Roads.ini
                InitFile = Data\INI\Roads.ini
            End

            LoadSubsystem ThePlayerTemplateStore
                Loader = INI
                InitFile = Data\INI\Default\PlayerTemplate.ini
                InitFile = Data\INI\PlayerTemplate.ini
            End

            LoadSubsystem TheParticleSystemManager
                Loader = INI
                InitFile = Data\INI\ParticleSystem.ini
            End

            LoadSubsystem TheMultiplayerSettings
                Loader = INI
                InitFile = Data\INI\Default\Multiplayer.ini
                InitFile = Data\INI\Multiplayer.ini
            End

            LoadSubsystem TheSpecialPowerStore
                Loader = INI
                InitFile = Data\INI\Default\SpecialPower.ini
                InitFile = Data\INI\SpecialPower.ini
            End

            LoadSubsystem TheLinearCampaignManager
                Loader = INI
                InitFile = Data\INI\Campaign.ini
            End

            LoadSubsystem Credits
                Loader = INI
                InitFile = Data\INI\Credits.ini
            End

            LoadSubsystem TheDamageFXStore
                Loader = INI
                InitFile = Data\INI\DamageFX.ini
            End

            LoadSubsystem InGameUI
                Loader = INI
                InitFile = Data\INI\InGameUI.ini
            End

            LoadSubsystem TheRankInfoStore
                Loader = INI
                InitFile = Data\INI\Rank.ini
            End

            LoadSubsystem Animation2D
                Loader = INI
                InitFile = Data\INI\Animation2D.ini
            End
            """;

        private readonly ContentManager _contentManager;
        private readonly IGameDefinition _gameDefinition;
        private readonly FileSystem _fileSystem;
        private readonly ScopedAssetCollection<LoadSubsystem> _subsystems;

        public SubsystemLoader(IGameDefinition gameDefinition, FileSystem fileSystem, Game game, ContentManager contentManager)
        {
            _gameDefinition = gameDefinition;
            _contentManager = contentManager;
            _fileSystem = fileSystem;

            switch (gameDefinition.Game)
            {
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                    // These games didn't use subsystemlegend.ini so we use our own retro-fitted one.
                    _contentManager.LoadIniFile(new FileSystemEntry(fileSystem, "subsystemlegend.ini", 0, () => new MemoryStream(Encoding.ASCII.GetBytes(GeneralsSubsystemLegendIni))));
                    break;

                default:
                    _contentManager.LoadIniFile(@"Data\INI\Default\subsystemlegend.ini");
                    break;
            }

            
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
                    _contentManager.LoadIniFile(@"Data\INI\Mouse.ini");
                    _contentManager.LoadIniFile(@"Data\INI\Water.ini");
                    _contentManager.LoadIniFile(@"Maps\MapCache.ini");
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            _contentManager.LoadIniFile(@"Data\INI\WaterTextures.ini");
                            break;
                    }
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.CncGenerals:
                        case SageGame.CncGeneralsZeroHour:
                            _contentManager.LoadIniFile($@"Data\{_contentManager.Language}\HeaderTemplate.ini");
                            break;

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
                    _contentManager.LoadIniFile(@"Data\INI\ControlBarScheme.ini");
                    _contentManager.LoadIniFile(@"Data\INI\CommandSet.ini");
                    _contentManager.LoadIniFile(@"Data\INI\CommandButton.ini");
                    break;

                case Subsystem.Audio:
                    _contentManager.LoadIniFile(@"Data\INI\AudioSettings.ini");
                    _contentManager.LoadIniFile(@"Data\INI\SoundEffects.ini");
                    _contentManager.LoadIniFile(@"Data\INI\MiscAudio.ini");
                    _contentManager.LoadIniFile(@"Data\INI\Voice.ini");
                    _contentManager.LoadIniFile(@"Data\INI\Music.ini");
                    break;

                case Subsystem.Wnd:
                    _contentManager.LoadIniFiles(@"Data\INI\MappedImages\HandCreated");
                    _contentManager.LoadIniFiles(@"Data\INI\MappedImages\TextureSize_512");
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.CncGenerals:
                        case SageGame.CncGeneralsZeroHour:
                            _contentManager.LoadIniFile(@"Data\INI\WindowTransitions.ini");
                            _contentManager.LoadIniFile(@"Data\INI\ControlBarScheme.ini");
                            break;

                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            _contentManager.LoadIniFiles(@"Data\INI\MappedImages\AptImages");
                            break;
                    }
                    break;
            }
        }

        private IEnumerable<string> GetSubsystemEntryName(Subsystem subsystem)
        {
            switch (subsystem)
            {
                case Subsystem.Core:
                    yield return "TheWritableGlobalData";
                    break;

                case Subsystem.Audio:
                    break;

                case Subsystem.ObjectCreation:
                    yield return "TheThingFactory";
                    yield return "TheUpgradeCenter";
                    yield return "TheObjectCreationListStore";
                    yield return "TheCrateSystem";
                    break;

                case Subsystem.Players:
                    yield return "ThePlayerTemplateStore";
                    break;

                case Subsystem.Locomotors:
                    yield return "TheLocomotorStore";
                    break;

                case Subsystem.Sciences:
                    yield return "TheScienceStore";
                    break;

                case Subsystem.Weapons:
                    yield return "TheWeaponStore";
                    break;

                case Subsystem.FXList:
                    yield return "TheFXListStore";
                    break;

                case Subsystem.Terrain:
                    yield return "TheTerrainTypes";
                    yield return "TheTerrainRoads";
                    break;

                case Subsystem.ParticleSystems:
                    yield return "TheParticleSystemManager";
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            yield return "TheFXParticleSystemManager";
                            break;
                    }
                    break;

                case Subsystem.Wnd:
                    break;

                case Subsystem.Multiplayer:
                    yield return "TheMultiplayerSettings";
                    break;

                case Subsystem.LinearCampaign:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.CncGenerals:
                        case SageGame.CncGeneralsZeroHour:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            yield return "TheLinearCampaignManager";
                            break;
                    }
                    break;

                case Subsystem.Credits:
                    yield return "Credits";
                    break;

                case Subsystem.Damage:
                    yield return "TheDamageFXStore";
                    break;

                case Subsystem.SpecialPower:
                    yield return "TheSpecialPowerStore";
                    break;

                case Subsystem.InGameUI:
                    yield return "InGameUI";
                    break;

                case Subsystem.ExperienceLevels:
                    yield return "TheExperienceLevelSystem";
                    break;

                case Subsystem.AttributeModifiers:
                    yield return "TheAttributeModifierStore";
                    break;

                case Subsystem.Rank:
                    yield return "TheRankInfoStore";
                    break;

                case Subsystem.Animation2D:
                    yield return "Animation2D";
                    break;

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
                            var entries = _fileSystem.GetFilesInDirectory(folder.Value, "*.ini", System.IO.SearchOption.AllDirectories).WhereNot(c => subsystem.ExcludePath.Contains(c.FilePath));
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
}

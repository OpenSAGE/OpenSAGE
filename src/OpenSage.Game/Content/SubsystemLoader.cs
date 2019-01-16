using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Content
{
    public interface ISubsystemLoader
    {
        void Load(Subsystem subsystem);
    }

    public class GeneralsSubsystemLoader : ISubsystemLoader
    {
        private readonly IniDataContext _iniDataContext;

        public GeneralsSubsystemLoader(IniDataContext iniDataContext)
        {
            _iniDataContext = iniDataContext;
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
                        @"Maps\MapCache.ini");
                    break;
                case Subsystem.ObjectCreation:
                    LoadFiles(@"Data\INI\Default\Object.ini");
                    _iniDataContext.LoadIniFiles(@"Data\INI\Object");
                    break;
                case Subsystem.Players:
                    LoadFiles(
                        @"Data\INI\Default\PlayerTemplate.ini",
                        @"Data\INI\PlayerTemplate.ini");
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
                        @"Data\English\HeaderTemplate.ini",
                        @"Data\INI\ControlBarScheme.ini");
                    break;
                case Subsystem.Multiplayer:
                    LoadFiles(@"Data\INI\Multiplayer.ini");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(subsystem), subsystem, null);
            }
        }

        private void LoadFiles(params string[] files)
        {
            foreach (var file in files)
            {
                _iniDataContext.LoadIniFile(file);
            }
        }
    }

    public class ConfiguredSubsystemLoader : ISubsystemLoader
    {
        private readonly IniDataContext _iniDataContext;
        private readonly IGameDefinition _gameDefinition;
        private readonly FileSystem _fileSystem;
        private readonly Dictionary<string, LoadSubsystem> _subsystems;

        public ConfiguredSubsystemLoader(IGameDefinition gameDefinition, FileSystem fileSystem, IniDataContext iniDataContext)
        {
            _gameDefinition = gameDefinition;
            _iniDataContext = iniDataContext;
            _fileSystem = fileSystem;

            _iniDataContext.LoadIniFile(@"Data\INI\Default\subsystemlegend.ini");
            _subsystems = _iniDataContext.Subsystems.ToDictionary(subsystem => subsystem.Name);
        }

        public void Load(Subsystem subsystem)
        {
            foreach (var entry in GetFilesForSubsystem(subsystem))
            {
                _iniDataContext.LoadIniFile(entry);
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
                case Subsystem.ObjectCreation:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            yield return "TheThingFactory";
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(subsystem), subsystem, null);
            }
        }

        private void LoadFiles(params string[] files)
        {
            foreach (var file in files)
            {
                _iniDataContext.LoadIniFile(file);
            }
        }

        private IEnumerable<FileSystemEntry> GetFilesForSubsystem(Subsystem abstractSubsystem)
        {
            var subsystems = GetSubsystemEntryName(abstractSubsystem).Select(entryName => _subsystems[entryName]).ToList();

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

            // Load hardcoded files
            switch(abstractSubsystem)
            {
                case Subsystem.Core:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            yield return _fileSystem.GetFile(@"Maps\MapCache.ini");
                            break;
                    }
                    break;
            }
        }
    }

    public static class SubsystemLoader
    {
        public static ISubsystemLoader Create(IGameDefinition gameDefinition, FileSystem fileSystem, IniDataContext iniDataContext)
        {
            switch (gameDefinition.Game)
            {
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                    return new GeneralsSubsystemLoader(iniDataContext);

                case SageGame.Bfme:
                case SageGame.Bfme2:
                case SageGame.Bfme2Rotwk:
                case SageGame.Cnc3:
                case SageGame.Cnc3KanesWrath:
                    return new ConfiguredSubsystemLoader(gameDefinition, fileSystem, iniDataContext);

                default:
                    // TODO: Implement subsystem loader for new XML-based format used in RA3 and beyond.
                    return null;
            }
        }
    }
}

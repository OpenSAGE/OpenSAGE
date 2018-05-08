using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenSage.Data.Ini;

namespace OpenSage.Data
{
    public class SubsystemConfigurationLoader
    {
        private readonly IGameDefinition _gameDefinition;
        private readonly FileSystem _fileSystem;
        private readonly IniDataContext _iniDataContext;
        private readonly Dictionary<string, LoadSubsystem> _subsystems;

        public SubsystemConfigurationLoader(IGameDefinition gameDefinition, FileSystem fileSystem, IniDataContext iniDataContext)
        {
            _gameDefinition = gameDefinition;
            _fileSystem = fileSystem;
            _iniDataContext = iniDataContext;

            _subsystems = GetSubsystems().ToDictionary(subsystem => subsystem.Name);
        }


        public Task Load(Subsystem subsystem)
        {
            return Task.WhenAll(GetFilesForSubsystem(subsystem).Select(_iniDataContext.LoadIniFileAsync));
        }

        public void Preload(Subsystem subsystem)
        {
            foreach (var entry in GetFilesForSubsystem(subsystem))
            {
                _iniDataContext.PreloadIniFile(entry);
            }
        }

        public void EnsureLoaded(Subsystem subsystem)
        {
            Load(subsystem).Wait();
        }

        private IEnumerable<LoadSubsystem> GetSubsystems()
        {
            switch (_gameDefinition.Game)
            {
                case SageGame.CncGenerals:
                    return GeneralsSubsystems;
                case SageGame.CncGeneralsZeroHour:
                    return ZeroHourSubsystems;
                default:
                    _iniDataContext.LoadIniFile(@"Data\INI\Default\subsystemlegend.ini");
                    return _iniDataContext.Subsystems;
            }
        }

        // TODO: Parse this from the LoadSubsystem entry for BFME and later games
        private IEnumerable<FileSystemEntry> GetFilesForSubsystem(Subsystem subsystem)
        {
            var entryName = GetSubsystemEntryName(subsystem);
            if (entryName == null || !_subsystems.TryGetValue(entryName, out var loadSubsystem))
            {
                yield break;
            }

            foreach (var file in loadSubsystem.InitFiles)
            {
                yield return _fileSystem.GetFile(file);
            }

            foreach (var pathRoots in loadSubsystem.InitPaths)
            {
                foreach (var file in _fileSystem.GetFiles(pathRoots))
                {
                    yield return file;
                }
            }
        }

        // TODO: This might need to able to return multiple entries
        private string GetSubsystemEntryName(Subsystem subsystem)
        {
            switch (subsystem)
            {
                case Subsystem.Core:
                    // TODO: This should be stored in the game definition, somehow.
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.CncGenerals:
                        case SageGame.CncGeneralsZeroHour: return "Core";

                        case SageGame.Bfme2: return "TheWritableGlobalData";
                    }
                    break;
                case Subsystem.ObjectCreation:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.CncGenerals:
                        case SageGame.CncGeneralsZeroHour: return "ObjectCreation";

                        case SageGame.Bfme2: return "TheThingFactory";
                    }
                    break;
                case Subsystem.PlayerCreation:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.CncGenerals:
                        case SageGame.CncGeneralsZeroHour: return "PlayerCreation";

                        case SageGame.Bfme2: return "ThePlayerTemplateStore";
                    }
                    break;
                case Subsystem.TerrainTypes:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.CncGenerals:
                        case SageGame.CncGeneralsZeroHour: return "Terrain";

                        case SageGame.Bfme2: return "TheTerrainTypes";
                    }
                    break;
                case Subsystem.RoadTypes:
                    break;
                case Subsystem.ParticleSystems:
                    switch (_gameDefinition.Game)
                    {
                        case SageGame.CncGenerals:
                        case SageGame.CncGeneralsZeroHour: return "ParticleSystems";

                        case SageGame.Bfme2: return "TheParticleSystemManager";
                    }
                    break;
                case Subsystem.Wnd:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(subsystem), subsystem, null);
            }

            return null;
        }

        // TODO: Move these to mod DLLs?
        private static readonly LoadSubsystem[] GeneralsSubsystems;
        private static readonly LoadSubsystem[] ZeroHourSubsystems;

        static SubsystemConfigurationLoader()
        {
            GeneralsSubsystems = new[]
            {
                new LoadSubsystem
                {
                    Name = "Core",
                    InitFiles =
                    {
                        @"Data\INI\Default\GameData.ini",
                        @"Data\INI\GameData.ini",
                        @"Data\INI\Mouse.ini"
                    }
                },
                new LoadSubsystem
                {
                    Name = "Terrain",
                    InitFiles =
                    {
                        @"Data\INI\Default\Terrain.ini",
                        @"Data\INI\Terrain.ini"
                    }
                },
                new LoadSubsystem
                {
                    Name = "ObjectCreation",
                    InitFiles =
                    {
                        @"Data\INI\Default\Object.ini"
                    },
                    InitPaths =
                    {
                        @"Data\INI\Object"
                    }
                },
                new LoadSubsystem
                {
                    Name = "ParticleSystems",
                    InitFiles =
                    {
                        @"Data\INI\ParticleSystem.ini"
                    }
                },
                new LoadSubsystem
                {
                    Name = "PlayerCreation",
                    InitFiles =
                    {
                        @"Data\INI\Default\PlayerTemplate.ini",
                        @"Data\INI\PlayerTemplate.ini"
                    }
                },
                new LoadSubsystem
                {
                    Name = "Wnd",
                    InitFiles =
                    {
                        @"Data\INI\WindowTransitions.ini",
                        @"Data\English\HeaderTemplate.ini",
                        @"Data\INI\ControlBarScheme.ini"
                    }
                }
            };

            ZeroHourSubsystems = GeneralsSubsystems;
        }
    }

    public enum Subsystem
    {
        Core,
        ObjectCreation,
        PlayerCreation,
        TerrainTypes,
        RoadTypes,
        ParticleSystems,
        Wnd
    }
}

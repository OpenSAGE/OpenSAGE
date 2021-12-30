using System;
using System.Collections.Generic;
using OpenSage.Client;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public abstract class ClientUpdateModule : ModuleBase
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);
            
            base.Load(reader);
        }
    }

    public abstract class ClientUpdateModuleData : ModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.ClientUpdate;

        internal static ModuleDataContainer ParseClientUpdate(IniParser parser, ModuleInheritanceMode inheritanceMode) => ParseModule(parser, ClientUpdateParseTable, inheritanceMode);

        private static readonly Dictionary<string, Func<IniParser, ClientUpdateModuleData>> ClientUpdateParseTable = new Dictionary<string, Func<IniParser, ClientUpdateModuleData>>
        {
            { "AnimatedParticleSysBoneClientUpdate", AnimatedParticleSysBoneClientUpdateModuleData.Parse },
            { "BeaconClientUpdate", BeaconClientUpdateModuleData.Parse },
            { "EvaAnnounceClientCreate", EvaAnnounceClientCreateModuleData.Parse },
            { "LaserUpdate", LaserUpdateModuleData.Parse },
            { "RadarMarkerClientUpdate", RadarMarkerClientUpdateModuleData.Parse },
            { "SwayClientUpdate", SwayClientUpdateModuleData.Parse },
        };

        internal virtual ClientUpdateModule CreateModule(Drawable drawable, GameContext context) => null; // TODO: Make this abstract.
    }
}

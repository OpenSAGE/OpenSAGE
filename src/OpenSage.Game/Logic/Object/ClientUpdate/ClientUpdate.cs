using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public abstract class ClientUpdateModuleData : BehaviorModuleData
    {
        internal static ClientUpdateModuleData ParseClientUpdate(IniParser parser) => ParseModule(parser, ClientUpdateParseTable);

        private static readonly Dictionary<string, Func<IniParser, ClientUpdateModuleData>> ClientUpdateParseTable = new Dictionary<string, Func<IniParser, ClientUpdateModuleData>>
        {
            { "AnimatedParticleSysBoneClientUpdate", AnimatedParticleSysBoneClientUpdateModuleData.Parse },
            { "BeaconClientUpdate", BeaconClientUpdateModuleData.Parse },
            { "EvaAnnounceClientCreate", EvaAnnounceClientCreateModuleData.Parse },
            { "LaserUpdate", LaserUpdateModuleData.Parse },
            { "RadarMarkerClientUpdate", RadarMarkerClientUpdateModuleData.Parse },
            { "SwayClientUpdate", SwayClientUpdateModuleData.Parse },
        };
    }
}

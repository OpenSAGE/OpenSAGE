using System;
using System.Collections.Generic;
using OpenSage.Client;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public abstract class ClientUpdateModule : ModuleBase
{
    // TODO(Port): Make this abstract after all modules have been ported.
    public virtual void ClientUpdate(in TimeInterval gameTime) { }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
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

    internal virtual ClientUpdateModule CreateModule(Drawable drawable, GameEngine gameEngine) => null; // TODO: Make this abstract.
}

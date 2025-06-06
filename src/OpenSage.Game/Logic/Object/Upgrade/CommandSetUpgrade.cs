﻿using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Gui.ControlBar;

namespace OpenSage.Logic.Object;

internal class CommandSetUpgrade : UpgradeModule
{
    private readonly CommandSetUpgradeModuleData _moduleData;

    public CommandSetUpgrade(GameObject gameObject, IGameEngine gameEngine, CommandSetUpgradeModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        _moduleData = moduleData;
    }

    protected override void OnUpgrade()
    {
        GameObject.Definition.CommandSet = _moduleData.CommandSet;
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}

/// <summary>
/// Gives the object the ability to change commandsets back and forth when this module exists multiple times.
/// </summary>
public sealed class CommandSetUpgradeModuleData : UpgradeModuleData
{
    internal static CommandSetUpgradeModuleData Parse(IniParser parser)
    {
        return parser.ParseBlock(FieldParseTable);
    }

    private static new readonly IniParseTable<CommandSetUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
        .Concat(new IniParseTable<CommandSetUpgradeModuleData>
        {
            { "RemovesUpgrades", (parser, x) => x.RemovesUpgrades = parser.ParseUpgradeReferenceArray() },
            { "CommandSet", (parser, x) => x.CommandSet = parser.ParseCommandSetReference() },
            { "CommandSetAlt", (parser, x) => x.CommandSetAlt = parser.ParseCommandSetReference() },
            { "TriggerAlt", (parser, x) => x.TriggerAlt = parser.ParseAssetReference() }
        });

    public LazyAssetReference<UpgradeTemplate>[] RemovesUpgrades { get; private set; }
    public LazyAssetReference<CommandSet> CommandSet { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public LazyAssetReference<CommandSet> CommandSetAlt { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public string TriggerAlt { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new CommandSetUpgrade(gameObject, gameEngine, this);
    }
}

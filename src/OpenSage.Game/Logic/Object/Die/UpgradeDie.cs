﻿using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public class UpgradeDieModule : DieModule
{
    private readonly UpgradeDieModuleData _moduleData;

    internal UpgradeDieModule(GameObject gameObject, IGameEngine gameEngine, UpgradeDieModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        _moduleData = moduleData;
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }

    protected override void Die(in DamageInfoInput damageInput)
    {
        var parent = GameEngine.GameLogic.GetObjectById(GameObject.CreatedByObjectID);

        parent?.RemoveUpgrade(_moduleData.UpgradeToRemove.UpgradeName.Value);
    }
}

/// <summary>
/// Frees the object-based upgrade for the producer object.
/// </summary>
public sealed class UpgradeDieModuleData : DieModuleData
{
    internal static UpgradeDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<UpgradeDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
        .Concat(new IniParseTable<UpgradeDieModuleData>
        {
            { "UpgradeToRemove", (parser, x) => x.UpgradeToRemove = UpgradeToRemove.Parse(parser) }
        });

    public UpgradeToRemove UpgradeToRemove { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new UpgradeDieModule(gameObject, gameEngine, this);
    }
}

public struct UpgradeToRemove
{
    internal static UpgradeToRemove Parse(IniParser parser)
    {
        return new UpgradeToRemove
        {
            UpgradeName = parser.ParseUpgradeReference(),
            ModuleTag = parser.ParseIdentifier(),
        };
    }

    public LazyAssetReference<UpgradeTemplate> UpgradeName { get; private set; }
    public string ModuleTag { get; private set; }
}

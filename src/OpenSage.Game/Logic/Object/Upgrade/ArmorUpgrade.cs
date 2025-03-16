using OpenSage.Client;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

internal sealed class ArmorUpgrade : UpgradeModule
{
    private readonly ArmorUpgradeModuleData _moduleData;

    internal ArmorUpgrade(GameObject gameObject, GameEngine context, ArmorUpgradeModuleData moduleData)
        : base(gameObject, context, moduleData)
    {
        _moduleData = moduleData;
    }

    protected override void OnUpgrade()
    {
        GameObject.BodyModule?.SetArmorSetFlag(ArmorSetCondition.PlayerUpgrade);

        // Added in Zero Hour. Seems like quite a big hack.
        // Unique case for AMERICA to test for upgrade to set flag
        if (IsTriggeredBy("Upgrade_AmericaChemicalSuits"))
        {
            GameObject.Drawable.SetTerrainDecal(ObjectDecalType.ChemSuit);
        }
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
/// Triggers use of PLAYER_UPGRADE ArmorSet on this object.
/// </summary>
public sealed class ArmorUpgradeModuleData : UpgradeModuleData
{
    internal static ArmorUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<ArmorUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
        .Concat(new IniParseTable<ArmorUpgradeModuleData>()
        {
            { "ArmorSetFlag", (parser, x) => x.ArmorSetFlag = parser.ParseEnum<ArmorSetCondition>() },
            { "IgnoreArmorUpgrade", (parser, x) => x.IgnoreArmorUpgrade = parser.ParseBoolean() }
        });

    [AddedIn(SageGame.Bfme)]
    public ArmorSetCondition ArmorSetFlag { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public bool IgnoreArmorUpgrade { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine context)
    {
        return new ArmorUpgrade(gameObject, context, this);
    }
}

using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class SpecialAbilityModule : SpecialPowerModule
{
    // TODO

    internal SpecialAbilityModule(GameObject gameObject, GameEngine context, SpecialAbilityModuleData moduleData) : base(gameObject, context, moduleData)
    {
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}

public sealed class SpecialAbilityModuleData : SpecialPowerModuleData
{
    internal static new SpecialAbilityModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<SpecialAbilityModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
        .Concat(new IniParseTable<SpecialAbilityModuleData>());

    internal override SpecialAbilityModule CreateModule(GameObject gameObject, GameEngine context)
    {
        return new SpecialAbilityModule(gameObject, context, this);
    }
}

using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class HijackerUpdate : UpdateModule
{
    public HijackerUpdate(GameObject gameObject, GameEngine context) : base(gameObject, context)
    {
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.SkipUnknownBytes(19);
    }
}

public sealed class HijackerUpdateModuleData : UpdateModuleData
{
    internal static HijackerUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<HijackerUpdateModuleData> FieldParseTable = new IniParseTable<HijackerUpdateModuleData>
    {
        { "ParachuteName", (parser, x) => x.ParachuteName = parser.ParseAssetReference() }
    };

    public string ParachuteName { get; private set; }

    internal override HijackerUpdate CreateModule(GameObject gameObject, GameEngine context)
    {
        return new HijackerUpdate(gameObject, context);
    }
}

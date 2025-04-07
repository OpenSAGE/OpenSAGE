using System;
using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class FireSpreadUpdate : UpdateModule
{
    // TODO
    public FireSpreadUpdate(GameObject gameObject, IGameEngine gameEngine) : base(gameObject, gameEngine)
    {
    }

    public override UpdateSleepTime Update()
    {
        // TODO(Port): Use correct value.
        return UpdateSleepTime.None;
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}

public sealed class FireSpreadUpdateModuleData : UpdateModuleData
{
    internal static FireSpreadUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<FireSpreadUpdateModuleData> FieldParseTable = new IniParseTable<FireSpreadUpdateModuleData>
    {
        { "OCLEmbers", (parser, x) => x.OCLEmbers = parser.ParseObjectCreationListReference() },
        { "MinSpreadDelay", (parser, x) => x.MinSpreadDelay = parser.ParseTimeMilliseconds() },
        { "MaxSpreadDelay", (parser, x) => x.MaxSpreadDelay = parser.ParseTimeMilliseconds() },
        { "SpreadTryRange", (parser, x) => x.SpreadTryRange = parser.ParseInteger() }
    };

    public LazyAssetReference<ObjectCreationList> OCLEmbers { get; private set; }
    public TimeSpan MinSpreadDelay { get; private set; }
    public TimeSpan MaxSpreadDelay { get; private set; }
    public int SpreadTryRange { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new FireSpreadUpdate(gameObject, gameEngine);
    }
}

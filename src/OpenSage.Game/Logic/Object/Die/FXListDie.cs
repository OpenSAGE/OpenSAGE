using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;

namespace OpenSage.Logic.Object;

public sealed class FXListDie : DieModule
{
    private readonly FXListDieModuleData _moduleData;

    internal FXListDie(GameObject gameObject, GameEngine gameEngine, FXListDieModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        _moduleData = moduleData;
    }

    protected override void Die(in DamageInfoInput damageInput)
    {
        _moduleData.DeathFX.Value.Execute(new FXListExecutionContext(
            GameObject.Rotation,
            GameObject.Translation,
            GameEngine));
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}

public sealed class FXListDieModuleData : DieModuleData
{
    internal static FXListDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<FXListDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
        .Concat(new IniParseTable<FXListDieModuleData>
        {
            { "DeathFX", (parser, x) => x.DeathFX = parser.ParseFXListReference() },
            { "OrientToObject", (parser, x) => x.OrientToObject = parser.ParseBoolean() },
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
            { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseAssetReferenceArray() },
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() }
        });

    public LazyAssetReference<FXList> DeathFX { get; private set; }
    public bool OrientToObject { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public bool StartsActive { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public string[] ConflictsWith { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public string[] TriggeredBy { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new FXListDie(gameObject, gameEngine, this);
    }
}

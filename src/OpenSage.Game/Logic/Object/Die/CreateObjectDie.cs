using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class CreateObjectDie : DieModule
{
    private readonly CreateObjectDieModuleData _moduleData;

    internal CreateObjectDie(GameObject gameObject, IGameEngine gameEngine, CreateObjectDieModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        _moduleData = moduleData;
    }

    protected override void Die(in DamageInfoInput damageInput)
    {
        GameEngine.ObjectCreationLists.Create(
            _moduleData.CreationList.Value,
            GameObject,
            GameEngine);
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}

public sealed class CreateObjectDieModuleData : DieModuleData
{
    internal static CreateObjectDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<CreateObjectDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
        .Concat(new IniParseTable<CreateObjectDieModuleData>
        {
            { "CreationList", (parser, x) => x.CreationList = parser.ParseObjectCreationListReference() },
            { "TransferPreviousHealth", (parser, x) => x.TransferPreviousHealth = parser.ParseBoolean() },
            { "DebrisPortionOfSelf", (parser, x) => x.DebrisPortionOfSelf = parser.ParseAssetReference() },
            { "UpgradeRequired", (parser, x) => x.UpgradeRequired = parser.ParseAssetReferenceArray() }
        });

    public LazyAssetReference<ObjectCreationList> CreationList { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public bool TransferPreviousHealth { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public string DebrisPortionOfSelf { get; private set; }

    [AddedIn(SageGame.Bfme2)]
    public string[] UpgradeRequired { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new CreateObjectDie(gameObject, gameEngine, this);
    }
}

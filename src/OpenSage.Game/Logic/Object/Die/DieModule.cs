#nullable enable

using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public abstract class DieModule : BehaviorModule
{
    private readonly DieModuleData _moduleData;

    protected DieModule(DieModuleData moduleData)
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

    internal sealed override void OnDie(BehaviorUpdateContext context, DeathType deathType, BitArray<ObjectStatus> status)
    {
        if (!_moduleData.DieData.IsApplicable(deathType, status))
        {
            return;
        }

        Die(context, deathType);
    }

    private protected virtual void Die(BehaviorUpdateContext context, DeathType deathType) { }
}

public abstract class DieModuleData : BehaviorModuleData
{
    public override ModuleKinds ModuleKinds => ModuleKinds.Die;

    internal static readonly IniParseTableChild<DieModuleData, DieLogicData> FieldParseTable = new IniParseTableChild<DieModuleData, DieLogicData>(x => x.DieData, DieLogicData.FieldParseTable);

    public DieLogicData DieData { get; } = new();
}

public sealed class DieLogicData
{
    internal static readonly IniParseTable<DieLogicData> FieldParseTable = new IniParseTable<DieLogicData>
    {
        { "RequiredStatus", (parser, x) => x.RequiredStatus = parser.ParseEnum<ObjectStatus>() },
        { "ExemptStatus", (parser, x) => x.ExemptStatus = parser.ParseEnum<ObjectStatus>() },
        { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
    };

    public BitArray<DeathType>? DeathTypes { get; private set; }
    public ObjectStatus? RequiredStatus { get; private set; }
    public ObjectStatus? ExemptStatus { get; internal set; }

    public bool IsApplicable(DeathType deathType, BitArray<ObjectStatus> status) =>
        (DeathTypes?.Get(deathType) ?? true) && IsCorrectStatus(status);

    private bool IsCorrectStatus(BitArray<ObjectStatus> status)
    {
        var required = !RequiredStatus.HasValue || // if nothing is required, we pass
            status.Get(RequiredStatus.Value);      // or if we are the one of the required statuses, we pass
        var notExempt = !ExemptStatus.HasValue ||  // if nothing is exempt, we pass
            !status.Get(ExemptStatus.Value);       // or if we are not one of the exempt statuses, we pass
        return required && notExempt;
    }
}

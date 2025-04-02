#nullable enable

using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public abstract class DieModule : BehaviorModule, IDieModule
{
    private readonly DieModuleData _moduleData;

    protected DieModule(GameObject gameObject, IGameEngine gameEngine, DieModuleData moduleData)
        : base(gameObject, gameEngine)
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

    void IDieModule.OnDie(in DamageInfoInput damageInput)
    {
        if (!_moduleData.DieData.IsDieApplicable(damageInput, GameObject))
        {
            return;
        }

        Die(damageInput);
    }

    // TODO(Port): Make this abstract.
    protected virtual void Die(in DamageInfoInput damageInput) { }
}

public interface IDieModule
{
    void OnDie(in DamageInfoInput damageInput);
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

    public bool IsDieApplicable(in DamageInfoInput damageInput, GameObject obj) =>
        (DeathTypes?.Get(damageInput.DeathType) ?? true) && IsCorrectStatus(obj);

    private bool IsCorrectStatus(GameObject obj)
    {
        var required = !RequiredStatus.HasValue || // if nothing is required, we pass
            obj.TestStatus(RequiredStatus.Value);  // or if we are the one of the required statuses, we pass
        var notExempt = !ExemptStatus.HasValue ||  // if nothing is exempt, we pass
            !obj.TestStatus(ExemptStatus.Value);   // or if we are not one of the exempt statuses, we pass
        return required && notExempt;
    }
}

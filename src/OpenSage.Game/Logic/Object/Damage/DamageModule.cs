namespace OpenSage.Logic.Object;

public abstract class DamageModule : BehaviorModule
{
    protected DamageModule(GameObject gameObject, GameEngine gameEngine) : base(gameObject, gameEngine)
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

internal interface IDamageModule
{
    void OnDamage(in DamageData damageData);
}

public abstract class DamageModuleData : ContainModuleData
{
    public override ModuleKinds ModuleKinds => ModuleKinds.Damage;
}

public static class DamageConstants
{
    public const float HugeDamageAmount = 999999.0f;
}

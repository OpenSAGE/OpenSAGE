#nullable enable

using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

/// <summary>
/// An inactive body module. They are indestructible and largely cannot be
/// affected by things in the world. Does not have data storage for health and
/// damage etc. It's an "inactive" object that isn't affected by matters of the
/// body... it's all in the mind!
/// </summary>
public sealed class InactiveBody : BodyModule
{
    private bool _dieCalled;

    public override float Health => 0.0f; // Inactive bodies have no health to get.

    public override BodyDamageType DamageState
    {
        get => BodyDamageType.Pristine;
        set { }
    }

    internal InactiveBody(GameObject gameObject, IGameEngine gameEngine)
        : base(gameObject, gameEngine)
    {
        gameObject.IsEffectivelyDead = true;
    }

    public override DamageInfoOutput AttemptDamage(in DamageInfoInput damageInput)
    {
        if (damageInput.DamageType == DamageType.Healing)
        {
            // Healing and damage are separate, so this shouldn't happen.
            return AttemptHealing(damageInput);
        }

        // Inactive bodies have no health so no damage can really be done.
        var damageOutput = new DamageInfoOutput
        {
            ActualDamageDealt = 0.0f,
            ActualDamageClipped = 0.0f,
            NoEffect = true,
        };

        // ... except damage type UNRESISTABLE always wipes us out.
        if (damageInput.DamageType == DamageType.Unresistable)
        {
            DebugUtility.AssertCrash(!GameObject.Definition.IsPrerequisite, "Prerequisites should not have InactiveBody");

            damageOutput.NoEffect = false;

            // Since we have no health, we do not call DamageModules, nor do
            // DamageFX. However, we DO process DieModules.
            if (!_dieCalled)
            {
                GameObject.OnDie(damageInput);
                _dieCalled = true;
            }
        }

        return damageOutput;
    }

    public override DamageInfoOutput AttemptHealing(in DamageInfoInput damageInput)
    {
        if (damageInput.DamageType != DamageType.Healing)
        {
            // Healing and damage are separate, so this shouldn't happen.
            return AttemptDamage(damageInput);
        }

        // Inactive bodies have no health so no healing can really be done.
        return new DamageInfoOutput
        {
            ActualDamageDealt = 0.0f,
            ActualDamageClipped = 0.0f,
            NoEffect = true,
        };
    }

    public override float EstimateDamage(in DamageInfoInput damageInfo)
    {
        // Inactive bodies have no health so no damage can really be done.
        var amount = 0.0f;

        // ... with this exception.
        if (damageInfo.DamageType == DamageType.Unresistable)
        {
            amount = damageInfo.Amount;
        }

        return amount;
    }

    public override void SetAflame(bool setting) { }

    public override void OnVeterancyLevelChanged(VeterancyLevel oldLevel, VeterancyLevel newLevel, bool provideFeedback) { }

    public override void SetArmorSetFlag(ArmorSetCondition armorSetCondition) { }

    public override void ClearArmorSetFlag(ArmorSetCondition armorSetType) { }

    public override bool TestArmorSetFlag(ArmorSetCondition armorSetType) => false;

    public override void InternalChangeHealth(float delta)
    {
        // Inactive bodies have no health to increase or decrease.
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
/// Prevents normal interaction with other objects.
/// </summary>
public sealed class InactiveBodyModuleData : BodyModuleData
{
    internal static InactiveBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<InactiveBodyModuleData> FieldParseTable = [];

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new InactiveBody(gameObject, gameEngine);
    }
}

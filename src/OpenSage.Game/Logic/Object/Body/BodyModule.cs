#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImGuiNET;
using OpenSage.Data.Ini;
using OpenSage.Diagnostics.Util;

namespace OpenSage.Logic.Object;

public abstract class BodyModule : BehaviorModule
{
    private float _damageScalar = 1.0f;

    protected BodyModule(GameObject gameObject, IGameEngine gameEngine)
        : base(gameObject, gameEngine)
    {
    }

    /// <summary>
    /// Try to damage this object. The module's Armor will be taken into account,
    /// so the actual damage done may vary considerably from what you requested.
    /// Also note that (if damage is done) the DamageFX will be invoked to
    /// provide audio/video effects as appropriate.
    /// </summary>
    public abstract DamageInfoOutput AttemptDamage(in DamageInfoInput damageInput);

    /// <summary>
    /// Instead of having negative damage count as healing, or allowing access
    /// to the private ChangeHealth method, we will use this parallel to
    /// <see cref="AttemptDamage"/> to do healing without hack.
    /// </summary>
    public abstract DamageInfoOutput AttemptHealing(in DamageInfoInput damageInput);

    /// <summary>
    /// Estimates the (unclipped) damage that would be done to this object by
    /// the given damage (taking bonuses, armor, etc. into account), but DO NOT
    /// alter the body in any way. (This is used by the AI system to choose
    /// weapons.)
    /// </summary>
    public abstract float EstimateDamage(in DamageInfoInput damageInput);

    /// <summary>
    /// Gets the current health.
    /// </summary>
    public abstract float Health { get; }

    /// <summary>
    /// Gets the maximum health.
    /// </summary>
    public virtual float MaxHealth => 0.0f;

    /// <summary>
    /// Sets the max health.
    /// </summary>
    public virtual void SetMaxHealth(float maxHealth, MaxHealthChangeType healthChangeType = MaxHealthChangeType.SameCurrentHealth) { }

    /// <summary>
    /// Gets the initial health.
    /// </summary>
    public virtual float InitialHealth => 0.0f;

    /// <summary>
    /// Sets the initial health percentage.
    /// </summary>
    public virtual void SetInitialHealth(int initialPercent) { }

    /// <summary>
    /// Gets the previous health.
    /// </summary>
    public virtual float PreviousHealth => 0.0f;

    public virtual LogicFrameSpan SubdualDamageHealRate => LogicFrameSpan.Zero;

    public virtual float SubdualDamageHealAmount => 0.0f;

    public virtual bool HasAnySubdualDamage => false;

    public virtual float CurrentSubdualDamageAmount => 0.0f;

    /// <summary>
    /// Setting this controls damage state directly. Will adjust hitpoints.
    /// </summary>
    public abstract BodyDamageType DamageState { get; set; }

    /// <summary>
    /// This is a major change like a damage state.
    /// </summary>
    public abstract void SetAflame(bool setting);

    /// <summary>
    /// Called immediately upon a new level being achieved.
    /// </summary>
    public abstract void OnVeterancyLevelChanged(VeterancyLevel oldLevel, VeterancyLevel newLevel, bool provideFeedback = false);

    public abstract void SetArmorSetFlag(ArmorSetCondition armorSetType);

    public abstract void ClearArmorSetFlag(ArmorSetCondition armorSetType);

    public abstract bool TestArmorSetFlag(ArmorSetCondition armorSetType);

    /// <summary>
    /// Returns info on last damage dealt to this object.
    /// </summary>
    public virtual DamageInfo? LastDamageInfo => null;

    /// <summary>
    /// Returns frame of last damage dealt to this object.
    /// </summary>
    public virtual LogicFrame LastDamageFrame => LogicFrame.Zero;

    /// <summary>
    /// Returns frame of last healing dealt to this object.
    /// </summary>
    public virtual LogicFrame LastHealingFrame => LogicFrame.Zero;

    public virtual ObjectId ClearableLastAttacker => ObjectId.Invalid;

    public virtual void ClearLastAttacker() { }

    public virtual bool FrontCrushed
    {
        get => false;
        set => Debug.Fail("You should never call this for generic Bodys");
    }

    public virtual bool BackCrushed
    {
        get => false;
        set => Debug.Fail("You should never call this for generic Bodys");
    }

    public float DamageScalar => _damageScalar;

    /// <summary>
    /// Allows outside systems to apply defensive bonus of penalties (they all
    /// stack as a multiplier).
    /// </summary>
    public void ApplyDamageScalar(float scalar)
    {
        _damageScalar *= scalar;
    }

    /// <summary>
    /// Changes the module's health by the given delta. Note that the module's
    /// DamageFX and Armor are NOT taken into account, so you should think
    /// about what you're bypassing when you call this directly (especially
    /// when decreasing health, since you probably want
    /// <see cref="AttemptDamage"/> or
    /// <see cref="AttemptHealing"/>.
    /// </summary>
    public abstract void InternalChangeHealth(float delta);

    public virtual bool IsIndestructible
    {
        get => true;
        set { }
    }

    public virtual void EvaluateVisualCondition() { }

    // Original comment says that this was made public for topple and building
    // collapse updates.
    public virtual void UpdateBodyParticleSystems() { }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistSingle(ref _damageScalar); // was roughly 0.9 after changing to hold the line
    }

    private DamageType _inspectorDamageType = DamageType.Explosion;
    private float _inspectorDamageAmount;
    private DeathType _inspectorDeathType = DeathType.Normal;

    internal override void DrawInspector()
    {
        ImGui.InputFloat("Damage scalar", ref _damageScalar);

        ImGui.LabelText("Health", Health.ToString());

        var maxHealth = (float)MaxHealth;
        if (ImGui.InputFloat("Max health", ref maxHealth))
        {
            SetMaxHealth(maxHealth);
        }

        ImGui.LabelText("Initial health", InitialHealth.ToString());
        ImGui.LabelText("Previous health", PreviousHealth.ToString());
        ImGui.LabelText("Subdual damage heal rate", SubdualDamageHealAmount.ToString());
        ImGui.LabelText("Has any subdual damage", HasAnySubdualDamage.ToString());
        ImGui.LabelText("Current subdual damage amount", CurrentSubdualDamageAmount.ToString());

        var damageState = DamageState;
        if (ImGuiUtility.ComboEnum("Damage state", ref damageState))
        {
            DamageState = damageState;
        }

        ImGui.LabelText("Last damage frame", LastDamageFrame.ToString());
        ImGui.LabelText("Last healing frame", LastHealingFrame.ToString());
        ImGui.LabelText("Front crushed", FrontCrushed.ToString());
        ImGui.LabelText("Back crushed", BackCrushed.ToString());
        ImGui.LabelText("Is indestructible", IsIndestructible.ToString());

        ImGui.Separator();

        ImGuiUtility.ComboEnum("Damage Type", ref _inspectorDamageType);
        ImGui.InputFloat("Damage Amount", ref _inspectorDamageAmount);
        ImGuiUtility.ComboEnum("Death Type", ref _inspectorDeathType);
        if (ImGui.Button("Apply Damage"))
        {
            AttemptDamage(new DamageInfoInput
            {
                DamageType = _inspectorDamageType,
                DeathType = _inspectorDeathType,
                Amount = _inspectorDamageAmount,
            });
        }
    }
}

public abstract class BodyModuleData : BehaviorModuleData
{
    public override ModuleKinds ModuleKinds => ModuleKinds.Body;

    internal static ModuleDataContainer ParseBody(IniParser parser, ModuleInheritanceMode inheritanceMode) => ParseModule(parser, BodyParseTable, inheritanceMode);

    private static readonly Dictionary<string, Func<IniParser, BodyModuleData>> BodyParseTable = new()
    {
        { "ActiveBody", ActiveBodyModuleData.Parse },
        { "DelayedDeathBody", DelayedDeathBodyModuleData.Parse },
        { "DetachableRiderBody", DetachableRiderBodyModuleData.Parse },
        { "FreeLifeBody", FreeLifeBodyModuleData.Parse },
        { "HighlanderBody", HighlanderBodyModuleData.Parse },
        { "HiveStructureBody", HiveStructureBodyModuleData.Parse },
        { "ImmortalBody", ImmortalBodyModuleData.Parse },
        { "InactiveBody", InactiveBodyModuleData.Parse },
        { "PorcupineFormationBodyModule", PorcupineFormationBodyModuleData.Parse },
        { "RespawnBody", RespawnBodyModuleData.Parse },
        { "StructureBody", StructureBodyModuleData.Parse },
        { "SymbioticStructuresBody", SymbioticStructuresBodyModuleData.Parse },
        { "UndeadBody", UndeadBodyModuleData.Parse },
    };
}

using FixedMath.NET;
using ImGuiNET;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public sealed class OverchargeBehavior : UpdateModule
{
    private readonly OverchargeBehaviorModuleData _moduleData;
    private bool _enabled;
    public bool Enabled => _enabled;

    public OverchargeBehavior(GameObject gameObject, GameEngine gameEngine, OverchargeBehaviorModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;
    }

    public void Activate()
    {
        _enabled = true;
        GameObject.EnergyProduction += GameObject.Definition.EnergyBonus;

        foreach (var powerPlantUpdate in GameObject.FindBehaviors<PowerPlantUpdate>())
        {
            powerPlantUpdate.ExtendRods();
        }

        // todo: this is fine for now, but generals seems to have some way of making sure it doesn't immediately sap health on subsequent toggles
        SetNextUpdateFrame(GameEngine.GameLogic.CurrentFrame);
    }

    public void Deactivate()
    {
        _enabled = false;
        GameObject.EnergyProduction = GameObject.Definition.EnergyProduction;

        foreach (var powerPlantUpdate in GameObject.FindBehaviors<PowerPlantUpdate>())
        {
            powerPlantUpdate.RetractRods();
            SetNextUpdateFrame(new LogicFrame(uint.MaxValue));
        }
    }

    internal override void Update(BehaviorUpdateContext context)
    {
        if (!_enabled || // nothing to do if we aren't currently overcharging
            GameObject.BodyModule.Health < GameObject.BodyModule.MaxHealth * _moduleData.NotAllowedWhenHealthBelowPercent || // must be above min health percent
            context.LogicFrame.Value < NextUpdateFrame.Frame) // must be ready for us to do more damage
        {
            return;
        }

        GameObject.AttemptDamage(new DamageInfoInput(GameObject)
        {
            DamageType = DamageType.Penalty,
            Amount = GameObject.BodyModule.MaxHealth * _moduleData.HealthPercentToDrainPerSecond / GameEngine.LogicFramesPerSecond,
        });
        SetNextUpdateFrame(new LogicFrame((uint)GameEngine.LogicFramesPerSecond));
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistBoolean(ref _enabled);
    }

    internal override void DrawInspector()
    {
        ImGui.LabelText("Overcharge", _enabled.ToString());
    }
}

/// <summary>
/// Displays GUI:OverchargeExhausted when object's health is below specified percentage.
/// Allows use of the <see cref="ObjectDefinition.EnergyBonus"/> parameter.
/// Allows this object to turn on or off the <see cref="ModelConditionFlag.PowerPlantUpgrading"/>
/// and <see cref="ModelConditionFlag.PowerPlantUpgraded"/> condition states.
/// </summary>
public sealed class OverchargeBehaviorModuleData : UpdateModuleData
{
    internal static OverchargeBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<OverchargeBehaviorModuleData> FieldParseTable = new IniParseTable<OverchargeBehaviorModuleData>
    {
        { "HealthPercentToDrainPerSecond", (parser, x) => x.HealthPercentToDrainPerSecond = parser.ParsePercentage() },
        { "NotAllowedWhenHealthBelowPercent", (parser, x) => x.NotAllowedWhenHealthBelowPercent = parser.ParsePercentage() }
    };

    /// <summary>
    /// Percentage of max health to drain per second when in overcharge mode.
    /// </summary>
    public Percentage HealthPercentToDrainPerSecond { get; private set; }

    /// <summary>
    /// Turn off overcharge bonus when object's current health is below this value.
    /// </summary>
    public Percentage NotAllowedWhenHealthBelowPercent { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new OverchargeBehavior(gameObject, gameEngine, this);
    }
}

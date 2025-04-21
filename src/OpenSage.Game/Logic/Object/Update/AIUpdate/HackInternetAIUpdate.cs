#nullable enable

using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Logic.AI;
using OpenSage.Logic.AI.AIStates;

namespace OpenSage.Logic.Object;

public class HackInternetAIUpdate : AIUpdate
{
    internal override HackInternetAIUpdateModuleData ModuleData { get; }

    private UnknownStateData? _packingUpData;

    internal HackInternetAIUpdate(GameObject gameObject, IGameEngine gameEngine, HackInternetAIUpdateModuleData moduleData) : base(gameObject, gameEngine, moduleData)
    {
        ModuleData = moduleData;
    }

    private protected override HackInternetAIUpdateStateMachine CreateStateMachine() => new(GameObject, GameEngine, this);

    public override UpdateSleepTime Update()
    {
        // Have to call our parent's IsIdle, because we override it to never
        // return true when we have a pending command...
        if (base.IsIdle)
        {
            if (_packingUpData != null && _packingUpData.TargetPosition != default)
            {
                SetTargetPoint(_packingUpData.TargetPosition);
            }
            _packingUpData = null;
        }

        // TODO(Port): Use correct value.
        return UpdateSleepTime.None;
    }

    public void StartHackingInternet()
    {
        Stop();

        StateMachine.SetState(HackInternetStateIds.Unpacking);
    }

    internal override void SetTargetPoint(Vector3 targetPoint)
    {
        Stop();

        if (StateMachine.CurrentStateId == HackInternetStateIds.Packing)
        {
            // we can't move just yet
            _packingUpData = new UnknownStateData { TargetPosition = targetPoint };
        }
        else
        {
            base.SetTargetPoint(targetPoint);
        }
    }

    internal override void Stop()
    {
        if (StateMachine.CurrentStateId == HackInternetStateIds.Unpacking)
        {
            // this takes effect immediately
            StateMachine.SetState(AIStateIds.Idle);
        }
        else if (StateMachine.CurrentStateId == HackInternetStateIds.HackInternet)
        {
            StateMachine.SetState(HackInternetStateIds.Packing);
        }

        // If we're in StopHackingInternetState, we need to see that through

        base.Stop();
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        var hasPackingUpData = _packingUpData != null;
        reader.PersistBoolean(ref hasPackingUpData);
        if (hasPackingUpData)
        {
            _packingUpData ??= new UnknownStateData();
            reader.PersistObject(_packingUpData);
        }
    }
}

internal sealed class HackInternetAIUpdateStateMachine : AIUpdateStateMachine
{
    public override HackInternetAIUpdate AIUpdate { get; }

    public HackInternetAIUpdateStateMachine(GameObject gameObject, IGameEngine gameEngine, HackInternetAIUpdate aiUpdate)
        : base(gameObject, gameEngine, aiUpdate)
    {
        AIUpdate = aiUpdate;

        DefineState(
            HackInternetStateIds.Unpacking,
            new StartHackingInternetState(this),
            HackInternetStateIds.HackInternet,
            HackInternetStateIds.HackInternet);

        DefineState(
            HackInternetStateIds.HackInternet,
            new HackInternetState(this),
            HackInternetStateIds.Packing,
            HackInternetStateIds.Packing);

        DefineState(
            HackInternetStateIds.Packing,
            new StopHackingInternetState(this),
            AIStateIds.Idle,
            AIStateIds.Idle);
    }

    internal LogicFrameSpan GetVariableFrames(LogicFrameSpan time, IGameEngine gameEngine)
    {
        var variationFactor = AIUpdate.ModuleData.PackUnpackVariationFactor;
        var variation = gameEngine.GameLogic.Random.NextSingle(
            1.0f - variationFactor,
            1.0f + variationFactor);
        return new LogicFrameSpan((uint)(time.Value * variation));
    }
}

internal static class HackInternetStateIds
{
    public static readonly StateId Unpacking = new(1000);
    public static readonly StateId HackInternet = new(1001);
    public static readonly StateId Packing = new(1002);
}

/// <summary>
/// Allows use of UnitPack, UnitUnpack, and UnitCashPing within the UnitSpecificSounds section
/// of the object.
/// Also allows use of PACKING and UNPACKING condition states.
/// </summary>
public sealed class HackInternetAIUpdateModuleData : AIUpdateModuleData
{
    internal new static HackInternetAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private new static readonly IniParseTable<HackInternetAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
        .Concat(new IniParseTable<HackInternetAIUpdateModuleData>
        {
            { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "PackTime", (parser, x) => x.PackTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "CashUpdateDelay", (parser, x) => x.CashUpdateDelay = parser.ParseTimeMillisecondsToLogicFrames() },
            { "CashUpdateDelayFast", (parser, x) => x.CashUpdateDelayFast = parser.ParseTimeMillisecondsToLogicFrames() },
            { "RegularCashAmount", (parser, x) => x.RegularCashAmount = parser.ParseInteger() },
            { "VeteranCashAmount", (parser, x) => x.VeteranCashAmount = parser.ParseInteger() },
            { "EliteCashAmount", (parser, x) => x.EliteCashAmount = parser.ParseInteger() },
            { "HeroicCashAmount", (parser, x) => x.HeroicCashAmount = parser.ParseInteger() },
            { "XpPerCashUpdate", (parser, x) => x.XpPerCashUpdate = parser.ParseInteger() },
            { "PackUnpackVariationFactor", (parser, x) => x.PackUnpackVariationFactor = parser.ParseFloat() },
        });

    public LogicFrameSpan UnpackTime { get; private set; }
    public LogicFrameSpan PackTime { get; private set; }
    public LogicFrameSpan CashUpdateDelay { get; private set; }

    /// <summary>
    /// Hack speed when in a container (presumably with <see cref="InternetHackContainModuleData"/>).
    /// </summary>
    /// <remarks>
    /// The ini comments say "Fast speed used inside a container (can only hack inside an Internet Center)", however
    /// other mods will use this inside of e.g. listening outposts ("hacker vans"), so this can definitely be used
    /// in <i>any</i> container, not just internet centers.
    /// </remarks>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public LogicFrameSpan CashUpdateDelayFast { get; private set; }

    public int RegularCashAmount { get; private set; }
    public int VeteranCashAmount { get; private set; }
    public int EliteCashAmount { get; private set; }
    public int HeroicCashAmount { get; private set; }
    public int XpPerCashUpdate { get; private set; }

    /// <summary>
    /// Adds +/- the factor to the pack and unpack time, randomly.
    /// </summary>
    /// <example>
    /// If this is 0.5 and the unpack time is 1000ms, the actual unpack time may be anywhere between 500 and 1500ms.
    /// </example>
    public float PackUnpackVariationFactor { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new HackInternetAIUpdate(gameObject, gameEngine, this);
    }
}

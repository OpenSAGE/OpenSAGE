#nullable enable

using OpenSage.Logic.AI.AIStates;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI;

internal sealed class SupplyAIUpdateStateMachine : StateMachineBase
{
    public override SupplyAIUpdate AIUpdate { get; }

    public SupplyAIUpdateStateMachine(GameObject gameObject, IGameEngine gameEngine, SupplyAIUpdate aiUpdate) : base(gameObject, gameEngine, aiUpdate)
    {
        AIUpdate = aiUpdate;

        // TODO(Port): This configuration is incomplete.

        DefineState(
            SupplyTruckStateIds.Busy,
            new SupplyUnknown1State(this),
            SupplyTruckStateIds.Busy,
            SupplyTruckStateIds.Busy);

        DefineState(
            SupplyTruckStateIds.Idle,
            new SupplyUnknown0State(this),
            SupplyTruckStateIds.Busy,
            SupplyTruckStateIds.Busy);

        DefineState(
            SupplyTruckStateIds.Wanting,
            new SupplyUnknown2State(this),
            SupplyTruckStateIds.Regrouping,
            SupplyTruckStateIds.Busy);

        DefineState(
            SupplyTruckStateIds.Regrouping,
            new SupplyUnknown3State(this),
            SupplyTruckStateIds.Wanting,
            SupplyTruckStateIds.Busy);

        DefineState(
            SupplyTruckStateIds.Docking,
            new SupplyUnknown4State(this),
            SupplyTruckStateIds.Busy,
            SupplyTruckStateIds.Busy);
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Persist(reader);
        reader.EndObject();
    }
}

internal static class SupplyTruckStateIds
{
    /// <summary>
    /// Not doing anything. Should I autopilot?
    /// </summary>
    public static readonly StateId Idle = new(0);

    /// <summary>
    /// Direct player involvement (move) has taken me off autopilot.
    /// </summary>
    public static readonly StateId Busy = new(1);

    /// <summary>
    /// Search for warehouse or center and dock with it.
    /// </summary>
    public static readonly StateId Wanting = new(2);

    /// <summary>
    /// Wanting failed, so hanging out at base until something changes.
    /// Autopilot will turn off.
    /// </summary>
    public static readonly StateId Regrouping = new(3);

    /// <summary>
    /// Docking substates are running; wait for them to finish.
    /// </summary>
    public static readonly StateId Docking = new(4);
}

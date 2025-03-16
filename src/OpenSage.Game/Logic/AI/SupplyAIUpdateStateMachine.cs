#nullable enable

using OpenSage.Logic.AI.AIStates;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI;

internal sealed class SupplyAIUpdateStateMachine : StateMachineBase
{
    public override SupplyAIUpdate AIUpdate { get; }

    public SupplyAIUpdateStateMachine(GameObject gameObject, GameEngine gameEngine, SupplyAIUpdate aiUpdate) : base(gameObject, gameEngine, aiUpdate)
    {
        AIUpdate = aiUpdate;

        AddState(0, new SupplyUnknown0State(this));
        AddState(1, new SupplyUnknown1State(this));
        AddState(2, new SupplyUnknown2State(this)); // occurred when loaded chinook was flying over war factory (only remaining building) attempting to drop off supplies
        AddState(3, new SupplyUnknown3State(this)); // occurred when loaded chinook was flying over war factory (only remaining building) attempting to drop off supplies
        AddState(4, new SupplyUnknown4State(this));
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Persist(reader);
        reader.EndObject();
    }
}

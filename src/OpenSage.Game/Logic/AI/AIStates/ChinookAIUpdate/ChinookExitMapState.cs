#nullable enable

using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class ChinookExitMapState : State
{
    internal ChinookExitMapState(ChinookAIUpdateStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader) { }
}

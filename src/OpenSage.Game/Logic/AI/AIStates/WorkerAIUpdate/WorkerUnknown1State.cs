#nullable enable

using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class WorkerUnknown1State : State
{
    internal WorkerUnknown1State(WorkerAIUpdate.WorkerAIUpdateStateMachine3 stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {
        // No version?
    }
}

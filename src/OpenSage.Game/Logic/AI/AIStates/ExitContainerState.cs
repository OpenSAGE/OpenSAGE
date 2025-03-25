#nullable enable

using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class ExitContainerState : State
{
    private ObjectId _containerObjectId;

    internal ExitContainerState(AIUpdateStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistObjectId(ref _containerObjectId);
    }
}

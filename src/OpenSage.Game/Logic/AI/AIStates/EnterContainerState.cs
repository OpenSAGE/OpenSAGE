#nullable enable

using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal class EnterContainerState : MoveTowardsState
{
    private ObjectId _containerObjectId;

    internal EnterContainerState(StateMachineBase stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Persist(reader);
        reader.EndObject();

        reader.PersistObjectId(ref _containerObjectId);
    }
}

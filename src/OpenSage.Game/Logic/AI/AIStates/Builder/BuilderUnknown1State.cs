#nullable enable

using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class BuilderUnknown1State : State
{
    internal BuilderUnknown1State(DozerAndWorkerState.BuilderStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.SkipUnknownBytes(4);

        var unknown2 = 1;
        reader.PersistInt32(ref unknown2);
        if (unknown2 != 1)
        {
            throw new InvalidStateException();
        }

        reader.SkipUnknownBytes(1);
    }
}

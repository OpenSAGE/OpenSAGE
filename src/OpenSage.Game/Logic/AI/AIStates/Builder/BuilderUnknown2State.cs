#nullable enable

using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class BuilderUnknown2State : State
{
    private int _unknown1;
    private int _unknown2;
    private bool _unknown3;

    internal BuilderUnknown2State(DozerAndWorkerState.BuilderStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistInt32(ref _unknown1);
        reader.PersistInt32(ref _unknown2);
        reader.PersistBoolean(ref _unknown3);
    }
}

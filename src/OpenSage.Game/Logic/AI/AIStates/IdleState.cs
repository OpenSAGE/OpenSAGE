﻿#nullable enable

namespace OpenSage.Logic.AI.AIStates;

internal sealed class IdleState : State
{
    private ushort _unknownShort;
    private bool _unknownBool1;
    private bool _unknownBool2;

    public override bool IsIdle => true;

    internal IdleState(StateMachineBase stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistUInt16(ref _unknownShort);
        reader.PersistBoolean(ref _unknownBool1);
        reader.PersistBoolean(ref _unknownBool2);
    }
}

﻿#nullable enable

using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class ChinookMoveToCombatDropState : State
{
    private float _unknownFloat1;
    private float _unknownFloat2;
    private float _unknownFloat3;

    internal ChinookMoveToCombatDropState(ChinookAIUpdateStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistSingle(ref _unknownFloat1);
        reader.PersistSingle(ref _unknownFloat2);
        reader.PersistSingle(ref _unknownFloat3);
    }
}

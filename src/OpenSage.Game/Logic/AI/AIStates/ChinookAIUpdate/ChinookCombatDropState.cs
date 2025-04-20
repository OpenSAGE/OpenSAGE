#nullable enable

using System.Collections.Generic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class ChinookCombatDropState : State
{
    private readonly ChinookAIUpdateStateMachine _stateMachine;
    private readonly List<Rope> _ropes = new();

    internal ChinookCombatDropState(ChinookAIUpdateStateMachine stateMachine) : base(stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public override StateReturnType OnEnter()
    {
        _ropes.Clear();

        var numRopes = _stateMachine.AIUpdate.ModuleData.NumRopes;
        for (var i = 0; i < numRopes; i++)
        {
            _ropes.Add(new Rope());
        }

        return StateReturnType.Continue;
    }

    public override void OnExit(StateExitType status)
    {
        _ropes.Clear();
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(2);

        reader.PersistListWithUInt32Count(_ropes, (StatePersister persister, ref Rope item) =>
        {
            item ??= new Rope();
            persister.PersistObjectValue(item);
        });
    }

    private sealed class Rope : IPersistableObject
    {
        public uint DrawableID;
        public Matrix4x3 Transform;
        public float UnknownFloat1;
        public float UnknownFloat2;
        public float UnknownFloat3;
        public uint UnknownInt;
        public readonly ObjectIdSet ObjectIds = new();

        public void Persist(StatePersister persister)
        {
            persister.PersistUInt32(ref DrawableID);
            persister.PersistMatrix4x3(ref Transform);
            persister.PersistSingle(ref UnknownFloat1);
            persister.PersistSingle(ref UnknownFloat2);
            persister.PersistSingle(ref UnknownFloat3);
            persister.PersistUInt32(ref UnknownInt);
            persister.PersistObject(ObjectIds);
        }
    }
}

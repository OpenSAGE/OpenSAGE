#nullable enable

using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class GuardState : State
{
    private readonly GuardStateMachine _stateMachine;

    public GuardState(AIUpdateStateMachine stateMachine) : base(stateMachine)
    {
        _stateMachine = new GuardStateMachine(stateMachine);
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        var unknownBool1 = true;
        reader.PersistBoolean(ref unknownBool1);

        reader.PersistObject(_stateMachine);
    }
}

internal static class GuardStateIds
{
    public static readonly StateId Inner = new(5000);
    public static readonly StateId Idle = new(5001);
    public static readonly StateId Outer = new(5002);
    public static readonly StateId Return = new(5003);
    public static readonly StateId GetCrate = new(5004);
    public static readonly StateId AttackAggressor = new(5005);
}

internal sealed class GuardStateMachine : StateMachineBase
{
    private ObjectId _guardObjectId;
    private ObjectId _guardObjectId2;
    private Vector3 _guardPosition;
    private string _guardPolygonTriggerName = "";

    public GuardStateMachine(AIUpdateStateMachine parentStateMachine) : base(parentStateMachine)
    {
        // TODO(Port): This configuration is incomplete.
        DefineState(GuardStateIds.Idle, new GuardIdleState(this), GuardStateIds.Inner, GuardStateIds.Return);
        DefineState(GuardStateIds.Outer, new GuardUnknown5002State(this), GuardStateIds.GetCrate, GuardStateIds.GetCrate);
        DefineState(GuardStateIds.Return, new GuardMoveState(this), GuardStateIds.Idle, GuardStateIds.Inner);
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(2);

        reader.BeginObject("Base");
        base.Persist(reader);
        reader.EndObject();

        reader.PersistObjectId(ref _guardObjectId);
        reader.PersistObjectId(ref _guardObjectId2);
        reader.PersistVector3(ref _guardPosition);
        reader.PersistAsciiString(ref _guardPolygonTriggerName);
    }

    private sealed class GuardIdleState : State
    {
        private uint _unknownInt;

        internal GuardIdleState(GuardStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistUInt32(ref _unknownInt);
        }
    }

    private sealed class GuardUnknown5002State : State
    {
        internal GuardUnknown5002State(GuardStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);
        }
    }

    private sealed class GuardMoveState : State
    {
        private uint _unknownInt;

        internal GuardMoveState(GuardStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistUInt32(ref _unknownInt);
        }
    }
}

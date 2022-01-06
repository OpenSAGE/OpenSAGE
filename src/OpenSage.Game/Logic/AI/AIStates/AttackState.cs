using System.Numerics;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackState : State
    {
        private readonly AttackStateMachine _stateMachine;

        private bool _unknownBool;
        private Vector3 _unknownPosition;

        public AttackState()
        {
            _stateMachine = new AttackStateMachine();
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistBoolean("UnknownBool", ref _unknownBool);
            reader.PersistVector3("UnknownPosition", ref _unknownPosition);
            reader.PersistObject("StateMachine", _stateMachine);
        }
    }

    internal sealed class AttackStateMachine : StateMachineBase
    {
        public AttackStateMachine()
        {
            AddState(0, new AttackMoveTowardsTargetState());
            AddState(1, new AttackMoveTowardsTargetState());
            AddState(2, new AttackAimWeaponState());
            AddState(3, new AttackFireWeaponState());
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();
        }

        private sealed class AttackMoveTowardsTargetState : MoveTowardsState
        {
            private Vector3 _unknownPosition;
            private uint _unknownFrame;
            private bool _unknownBool1;
            private bool _unknownBool2;
            private bool _unknownBool3;
            private bool _unknownBool4;

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.BeginObject("Base");
                base.Persist(reader);
                reader.EndObject();

                reader.PersistVector3("UnknownPosition", ref _unknownPosition);
                reader.PersistFrame("UnknownFrame", ref _unknownFrame);
                reader.PersistBoolean("UnknownBool1", ref _unknownBool1);
                reader.PersistBoolean("UnknownBool2", ref _unknownBool2);
                reader.PersistBoolean("UnknownBool3", ref _unknownBool3);
                reader.PersistBoolean("UnknownBool4", ref _unknownBool4);
            }
        }

        private sealed class AttackAimWeaponState : State
        {
            private bool _unknownBool1;
            private bool _unknownBool2;

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.PersistBoolean("UnknownBool1", ref _unknownBool1);
                reader.PersistBoolean("UnknownBool2", ref _unknownBool2);
            }
        }

        private sealed class AttackFireWeaponState : State
        {
            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);
            }
        }
    }
}

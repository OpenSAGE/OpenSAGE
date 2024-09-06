using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackState : State
    {
        private readonly AttackStateMachine _stateMachine;

        private bool _unknownBool;
        private Vector3 _unknownPosition;

        public AttackState(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context)
        {
            _stateMachine = new AttackStateMachine(gameObject, context, aiUpdate);
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistBoolean(ref _unknownBool);
            reader.PersistVector3(ref _unknownPosition);
            reader.PersistObject(_stateMachine);
        }
    }

    internal sealed class AttackStateMachine : StateMachineBase
    {
        public AttackStateMachine(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context)
        {
            AddState(0, new AttackMoveTowardsTargetState(gameObject, context, aiUpdate));
            AddState(1, new AttackMoveTowardsTargetState(gameObject, context, aiUpdate));
            AddState(2, new AttackAimWeaponState(gameObject, context));
            AddState(3, new AttackFireWeaponState(gameObject, context));
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

            internal AttackMoveTowardsTargetState(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context, aiUpdate)
            {
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.BeginObject("Base");
                base.Persist(reader);
                reader.EndObject();

                reader.PersistVector3(ref _unknownPosition);
                reader.PersistFrame(ref _unknownFrame);
                reader.PersistBoolean(ref _unknownBool1);
                reader.PersistBoolean(ref _unknownBool2);
                reader.PersistBoolean(ref _unknownBool3);
                reader.PersistBoolean(ref _unknownBool4);
            }
        }

        private sealed class AttackAimWeaponState : State
        {
            private bool _unknownBool1;
            private bool _unknownBool2;

            internal AttackAimWeaponState(GameObject gameObject, GameContext context) : base(gameObject, context)
            {
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.PersistBoolean(ref _unknownBool1);
                reader.PersistBoolean(ref _unknownBool2);
            }
        }

        private sealed class AttackFireWeaponState : State
        {
            internal AttackFireWeaponState(GameObject gameObject, GameContext context) : base(gameObject, context)
            {
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);
            }
        }
    }
}

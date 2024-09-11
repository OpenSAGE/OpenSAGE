#nullable enable

using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackState : State
    {
        private readonly AttackStateMachine _stateMachine;

        private bool _unknownBool;
        private Vector3 _unknownPosition;

        public AttackState(StateMachineBase stateMachine) : base(stateMachine)
        {
            _stateMachine = new AttackStateMachine(stateMachine.GameObject, stateMachine.Context, stateMachine.ModuleData);
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
        public AttackStateMachine(GameObject gameObject, GameContext context, AIUpdateModuleData moduleData) : base(gameObject, context, moduleData)
        {
            AddState(0, new AttackMoveTowardsTargetState(this));
            AddState(1, new AttackMoveTowardsTargetState(this));
            AddState(2, new AttackAimWeaponState(this));
            AddState(3, new AttackFireWeaponState(this));
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

            internal AttackMoveTowardsTargetState(AttackStateMachine stateMachine) : base(stateMachine)
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

            internal AttackAimWeaponState(AttackStateMachine stateMachine) : base(stateMachine)
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
            internal AttackFireWeaponState(AttackStateMachine stateMachine) : base(stateMachine)
            {
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);
            }
        }
    }
}

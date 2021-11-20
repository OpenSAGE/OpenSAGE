using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackState : State
    {
        private readonly AttackStateMachine _stateMachine;

        public AttackState()
        {
            _stateMachine = new AttackStateMachine();
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownBool2 = reader.ReadBoolean();
            var positionSomething = reader.ReadVector3();

            _stateMachine.Load(reader);
        }
    }

    internal sealed class AttackStateMachine : StateMachineBase
    {
        public AttackStateMachine()
        {
            AddState(1, new AttackMoveTowardsTargetState());
            AddState(2, new AttackAimWeaponState());
            AddState(3, new AttackFireWeaponState());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }

        private sealed class AttackMoveTowardsTargetState : MoveTowardsState
        {
            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                base.Load(reader);

                var positionSomething2 = reader.ReadVector3();
                var frameSomething = reader.ReadUInt32();
                var unknownBool1 = reader.ReadBoolean();
                var unknownBool2 = reader.ReadBoolean();
                var unknownBool3 = reader.ReadBoolean();
                var unknownBool4 = reader.ReadBoolean();
            }
        }

        private sealed class AttackAimWeaponState : State
        {
            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                var unknownBool1 = reader.ReadBoolean();
                var unknownBool2 = reader.ReadBoolean();
            }
        }

        private sealed class AttackFireWeaponState : State
        {
            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);
            }
        }
    }
}

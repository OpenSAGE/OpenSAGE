using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackMoveState : MoveTowardsState
    {
        private readonly AttackMoveStateMachine _stateMachine;

        public AttackMoveState()
        {
            _stateMachine = new AttackMoveStateMachine();
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            base.Load(reader);

            var unknownInt1 = reader.ReadInt32();
            var unknownInt2 = reader.ReadInt32();

            _stateMachine.Load(reader);
        }
    }

    internal sealed class AttackMoveStateMachine : StateMachineBase
    {
        public AttackMoveStateMachine()
        {
            AddState(0, new IdleState());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }
}

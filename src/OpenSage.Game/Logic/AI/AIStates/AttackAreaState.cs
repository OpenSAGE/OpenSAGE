using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackAreaState : State
    {
        private readonly AttackAreaAIStateMachine _stateMachine;

        public AttackAreaState()
        {
            _stateMachine = new AttackAreaAIStateMachine();
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownBool1 = reader.ReadBoolean();
            if (!unknownBool1)
            {
                throw new InvalidDataException();
            }

            _stateMachine.Load(reader);

            var unknownInt = reader.ReadUInt32();
        }
    }

    internal sealed class AttackAreaAIStateMachine : StateMachineBase
    {
        public AttackAreaAIStateMachine()
        {
            AddState(0, new IdleState());
            AddState(10, new AttackState());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }
}

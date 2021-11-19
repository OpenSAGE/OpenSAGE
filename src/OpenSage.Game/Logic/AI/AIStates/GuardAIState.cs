using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class GuardAIState : State
    {
        private readonly GuardAIStateMachine _stateMachine;

        public GuardAIState()
        {
            _stateMachine = new GuardAIStateMachine();
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownBool1 = reader.ReadBoolean();

            _stateMachine.Load(reader);
        }
    }

    internal sealed class GuardAIStateMachine : StateMachineBase
    {
        public GuardAIStateMachine()
        {
            AddState(5001, new GuardIdleAIState());
            AddState(5003, new GuardMoveAIState());
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            base.Load(reader);

            var unknownInt1 = reader.ReadUInt32();
            if (unknownInt1 != 0)
            {
                throw new InvalidDataException();
            }

            var unknownInt2 = reader.ReadUInt32();
            var unknownPos = reader.ReadVector3();

            var polygonTriggerName = reader.ReadAsciiString();
        }

        private sealed class GuardIdleAIState : State
        {
            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                var unknownInt1 = reader.ReadUInt32();
            }
        }

        private sealed class GuardMoveAIState : State
        {
            internal override void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                var unknownInt1 = reader.ReadUInt32();
            }
        }
    }
}

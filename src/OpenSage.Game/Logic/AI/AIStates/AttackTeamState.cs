using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackTeamState : State
    {
        private readonly AttackAreaStateMachine _stateMachine;

        public AttackTeamState()
        {
            _stateMachine = new AttackAreaStateMachine();
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
        }
    }
}

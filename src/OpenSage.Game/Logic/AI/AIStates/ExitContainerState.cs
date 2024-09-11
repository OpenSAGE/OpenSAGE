#nullable enable

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class ExitContainerState : State
    {
        private uint _containerObjectId;

        internal ExitContainerState(AIUpdateStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistObjectID(ref _containerObjectId);
        }
    }
}

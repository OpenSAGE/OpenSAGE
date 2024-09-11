#nullable enable

namespace OpenSage.Logic.AI.AIStates
{
    internal class EnterContainerState : MoveTowardsState
    {
        private uint _containerObjectId;

        internal EnterContainerState(StateMachineBase stateMachine) : base(stateMachine)
        {
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();

            reader.PersistObjectID(ref _containerObjectId);
        }
    }
}

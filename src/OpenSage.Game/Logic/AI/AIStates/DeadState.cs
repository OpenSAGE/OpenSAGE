using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class DeadState : State
    {
        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);
        }
    }
}

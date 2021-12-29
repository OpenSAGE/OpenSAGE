using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class DeadState : State
    {
        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);
        }
    }
}

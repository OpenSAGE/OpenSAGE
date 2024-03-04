using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class HackInternetState : State
    {
        public LogicFrameSpan FramesUntilNextHack;

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistLogicFrameSpan(ref FramesUntilNextHack);
        }
    }
}

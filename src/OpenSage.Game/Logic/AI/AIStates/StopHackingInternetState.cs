using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class StopHackingInternetState : State
    {
        public LogicFrameSpan FramesUntilFinishedPacking;

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistLogicFrameSpan(ref FramesUntilFinishedPacking);
        }
    }
}

using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class StartHackingInternetState : State
    {
        public LogicFrameSpan FramesUntilHackingBegins;

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistLogicFrameSpan(ref FramesUntilHackingBegins);
        }
    }
}

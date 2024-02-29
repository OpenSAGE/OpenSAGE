namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class HackInternetState : State
    {
        public uint FramesUntilNextHack;

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistUInt32(ref FramesUntilNextHack);
        }
    }
}

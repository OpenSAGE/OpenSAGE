namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class StartHackingInternetState : State
    {
        public uint FramesUntilHackingBegins;

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistUInt32(ref FramesUntilHackingBegins);
        }
    }
}

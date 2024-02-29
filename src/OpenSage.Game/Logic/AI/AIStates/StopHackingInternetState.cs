namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class StopHackingInternetState : State
    {
        public uint FramesUntilFinishedPacking;

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistUInt32(ref FramesUntilFinishedPacking);
        }
    }
}

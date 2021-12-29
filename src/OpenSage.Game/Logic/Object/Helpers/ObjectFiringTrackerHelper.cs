namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class ObjectFiringTrackerHelper : UpdateModule
    {
        private uint _numShotsFiredAtLastTarget;
        private uint _lastTargetObjectId;

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.ReadUInt32(ref _numShotsFiredAtLastTarget);
            reader.ReadObjectID(ref _lastTargetObjectId);

            reader.SkipUnknownBytes(4);
        }
    }
}

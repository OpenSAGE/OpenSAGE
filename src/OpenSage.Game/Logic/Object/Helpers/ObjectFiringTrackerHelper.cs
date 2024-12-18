namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class ObjectFiringTrackerHelper : UpdateModule
    {
        private uint _numShotsFiredAtLastTarget;
        private uint _lastTargetObjectId;

        protected override UpdateOrder UpdateOrder => UpdateOrder.Order3;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistBase(base.Load);

            reader.PersistUInt32(ref _numShotsFiredAtLastTarget);
            reader.PersistObjectID(ref _lastTargetObjectId);

            reader.SkipUnknownBytes(4);
        }
    }
}

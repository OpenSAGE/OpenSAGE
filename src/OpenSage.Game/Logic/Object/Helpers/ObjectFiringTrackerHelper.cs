namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class ObjectFiringTrackerHelper : UpdateModule
    {
        private uint _numShotsFiredAtLastTarget;
        private uint _lastTargetObjectId;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistUInt32("NumShotsFiredAtLastTarget", ref _numShotsFiredAtLastTarget);
            reader.PersistObjectID("LastTargetObjectId", ref _lastTargetObjectId);

            reader.SkipUnknownBytes(4);
        }
    }
}

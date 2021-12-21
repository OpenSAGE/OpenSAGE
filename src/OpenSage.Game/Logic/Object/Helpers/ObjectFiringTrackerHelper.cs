namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class ObjectFiringTrackerHelper : UpdateModule
    {
        private uint _numShotsFiredAtLastTarget;
        private uint _lastTargetObjectId;

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            _numShotsFiredAtLastTarget = reader.ReadUInt32();

            _lastTargetObjectId = reader.ReadObjectID();

            reader.SkipUnknownBytes(4);
        }
    }
}

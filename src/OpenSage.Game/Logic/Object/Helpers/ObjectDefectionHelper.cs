namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class ObjectDefectionHelper : ObjectHelperModule
    {
        private uint _frameStart;
        private uint _frameEnd;
        private bool _unknown;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);

            reader.PersistUInt32(ref _frameStart);
            reader.PersistUInt32(ref _frameEnd);

            reader.SkipUnknownBytes(4);

            reader.PersistBoolean(ref _unknown);
        }
    }
}

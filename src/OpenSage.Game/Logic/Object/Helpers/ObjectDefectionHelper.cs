namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class ObjectDefectionHelper : ObjectHelperModule
    {
        private uint _frameStart;
        private uint _frameEnd;
        private bool _unknown;

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.ReadUInt32(ref _frameStart);
            reader.ReadUInt32(ref _frameEnd);

            reader.SkipUnknownBytes(4);

            reader.ReadBoolean(ref _unknown);
        }
    }
}

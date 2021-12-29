namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class ObjectDefectionHelper : ObjectHelperModule
    {
        private uint _frameStart;
        private uint _frameEnd;
        private bool _unknown;

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            _frameStart = reader.ReadUInt32();
            _frameEnd = reader.ReadUInt32();

            reader.SkipUnknownBytes(4);

            reader.ReadBoolean(ref _unknown);
        }
    }
}

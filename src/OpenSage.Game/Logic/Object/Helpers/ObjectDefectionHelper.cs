namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class ObjectDefectionHelper : ObjectHelperModule
    {
        // TODO

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var frameStart = reader.ReadUInt32();
            var frameEnd = reader.ReadUInt32();

            reader.SkipUnknownBytes(4);

            var unknown = reader.ReadBoolean();
        }
    }
}

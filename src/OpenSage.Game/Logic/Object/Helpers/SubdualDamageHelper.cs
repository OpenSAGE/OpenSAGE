namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class SubdualDamageHelper : ObjectHelperModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);

            reader.SkipUnknownBytes(4);
        }
    }
}

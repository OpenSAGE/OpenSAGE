namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class StatusDamageHelper : ObjectHelperModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistBase(base.Load);

            reader.SkipUnknownBytes(8);
        }
    }
}

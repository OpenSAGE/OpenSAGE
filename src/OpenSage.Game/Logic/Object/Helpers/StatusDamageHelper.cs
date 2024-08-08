namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class StatusDamageHelper : ObjectHelperModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.SkipUnknownBytes(8);
        }
    }
}

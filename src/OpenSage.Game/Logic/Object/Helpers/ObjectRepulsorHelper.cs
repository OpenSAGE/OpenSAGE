namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class ObjectRepulsorHelper : ObjectHelperModule
    {
        // TODO

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }
}

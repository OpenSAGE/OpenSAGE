namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class ObjectRepulsorHelper : ObjectHelperModule
    {
        // TODO

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }
}

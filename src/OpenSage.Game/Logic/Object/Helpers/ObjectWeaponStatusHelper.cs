namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class ObjectWeaponStatusHelper : ObjectHelperModule
    {
        // TODO

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);
        }
    }
}

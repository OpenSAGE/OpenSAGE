namespace OpenSage.Logic.Object.Helpers
{
    internal abstract class ObjectHelperModule : UpdateModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);
        }
    }
}

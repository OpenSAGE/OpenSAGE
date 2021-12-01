namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class ObjectFiringTrackerHelper : UpdateModule
    {
        // TODO

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.__Skip(12);
        }
    }
}

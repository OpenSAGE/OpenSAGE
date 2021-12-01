namespace OpenSage.Logic.Object.Helpers
{
    internal abstract class ObjectHelperModule : UpdateModule
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }
}

namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class TempWeaponBonusHelper : ObjectHelperModule
    {
        private int _unknownInt;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistInt32(ref _unknownInt);
            if (_unknownInt != -1 && _unknownInt != 0)
            {
                // Normally -1, 0 for a civilian building when an infantry is garrisoned inside it
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(4);
        }
    }
}

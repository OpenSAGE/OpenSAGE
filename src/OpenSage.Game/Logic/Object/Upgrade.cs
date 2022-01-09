namespace OpenSage.Logic.Object
{
    public sealed class Upgrade : IPersistableObject
    {
        private UpgradeStatus _status;

        public Upgrade(UpgradeTemplate template)
        {
            Template = template;
        }

        public readonly UpgradeTemplate Template;

        public UpgradeStatus Status
        {
            get => _status;
            internal set => _status = value;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistEnum(ref _status);
        }
    }
}

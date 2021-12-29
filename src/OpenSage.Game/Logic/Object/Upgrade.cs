namespace OpenSage.Logic.Object
{
    public sealed class Upgrade
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

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            reader.ReadEnum(ref _status);
        }
    }
}

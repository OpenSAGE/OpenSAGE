using OpenSage.Data.Sav;

namespace OpenSage.Logic.Object
{
    public sealed class Upgrade
    {
        public Upgrade(UpgradeTemplate template)
        {
            Template = template;
        }

        public readonly UpgradeTemplate Template;

        public UpgradeStatus Status { get; private set; }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            Status = reader.ReadEnum<UpgradeStatus>();
        }
    }
}

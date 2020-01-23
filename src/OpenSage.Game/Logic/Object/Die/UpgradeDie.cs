using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Frees the object-based upgrade for the producer object.
    /// </summary>
    public sealed class UpgradeDieModuleData : DieModuleData
    {
        internal static UpgradeDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<UpgradeDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<UpgradeDieModuleData>
            {
                { "UpgradeToRemove", (parser, x) => x.UpgradeToRemove = UpgradeToRemove.Parse(parser) }
            });

        public UpgradeToRemove UpgradeToRemove { get; private set; }
    }

    public struct UpgradeToRemove
    {
        internal static UpgradeToRemove Parse(IniParser parser)
        {
            return new UpgradeToRemove
            {
                UpgradeName = parser.ParseAssetReference(),
                ModuleTag = parser.ParseIdentifier()
            };
        }

        public string UpgradeName { get; private set; }
        public string ModuleTag { get; private set; }
    }
}

using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Frees the object-based upgrade for the producer object.
    /// </summary>
    public sealed class UpgradeDie : ObjectBehavior
    {
        internal static UpgradeDie Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<UpgradeDie> FieldParseTable = new IniParseTable<UpgradeDie>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "UpgradeToRemove", (parser, x) => x.UpgradeToRemove = UpgradeToRemove.Parse(parser) }
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
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

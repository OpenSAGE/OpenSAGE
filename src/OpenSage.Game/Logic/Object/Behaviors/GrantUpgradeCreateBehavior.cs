using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class GrantUpgradeCreateBehavior : ObjectBehavior
    {
        internal static GrantUpgradeCreateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GrantUpgradeCreateBehavior> FieldParseTable = new IniParseTable<GrantUpgradeCreateBehavior>
        {
            { "UpgradeToGrant", (parser, x) => x.UpgradeToGrant = parser.ParseAssetReference() }
        };

        public string UpgradeToGrant { get; private set; }
    }
}

using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class GrantUpgradeCreate : ObjectBehavior
    {
        internal static GrantUpgradeCreate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GrantUpgradeCreate> FieldParseTable = new IniParseTable<GrantUpgradeCreate>
        {
            { "UpgradeToGrant", (parser, x) => x.UpgradeToGrant = parser.ParseAssetReference() },
            { "ExemptStatus", (parser, x) => x.ExemptStatus = parser.ParseEnum<ObjectStatus>() }
        };

        public string UpgradeToGrant { get; private set; }
        public ObjectStatus ExemptStatus { get; private set; }
    }
}

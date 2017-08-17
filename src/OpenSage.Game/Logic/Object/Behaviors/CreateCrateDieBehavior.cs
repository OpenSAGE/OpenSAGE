using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class CreateCrateDieBehavior : ObjectBehavior
    {
        internal static CreateCrateDieBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CreateCrateDieBehavior> FieldParseTable = new IniParseTable<CreateCrateDieBehavior>
        {
            { "CrateData", (parser, x) => x.CrateData = parser.ParseAssetReference() }
        };

        public string CrateData { get; private set; }
    }
}

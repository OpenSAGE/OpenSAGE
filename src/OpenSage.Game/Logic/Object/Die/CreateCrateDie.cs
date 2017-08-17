using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class CreateCrateDie : ObjectBehavior
    {
        internal static CreateCrateDie Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CreateCrateDie> FieldParseTable = new IniParseTable<CreateCrateDie>
        {
            { "CrateData", (parser, x) => x.CrateData = parser.ParseAssetReference() }
        };

        public string CrateData { get; private set; }
    }
}

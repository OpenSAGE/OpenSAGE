using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class OCLUpdate : ObjectBehavior
    {
        internal static OCLUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<OCLUpdate> FieldParseTable = new IniParseTable<OCLUpdate>
        {
            { "OCL", (parser, x) => x.OCL = parser.ParseAssetReference() },
            { "MinDelay", (parser, x) => x.MinDelay = parser.ParseInteger() },
            { "MaxDelay", (parser, x) => x.MaxDelay = parser.ParseInteger() },
            { "CreateAtEdge", (parser, x) => x.CreateAtEdge = parser.ParseBoolean() }
        };

        public string OCL { get; private set; }
        public int MinDelay { get; private set; }
        public int MaxDelay { get; private set; }
        public bool CreateAtEdge { get; private set; }
    }
}

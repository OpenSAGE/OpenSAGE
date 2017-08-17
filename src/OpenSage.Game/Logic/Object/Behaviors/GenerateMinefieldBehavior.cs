using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class GenerateMinefieldBehavior : ObjectBehavior
    {
        internal static GenerateMinefieldBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GenerateMinefieldBehavior> FieldParseTable = new IniParseTable<GenerateMinefieldBehavior>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() },
            { "MineName", (parser, x) => x.MineName = parser.ParseAssetReference() },
            { "SmartBorder", (parser, x) => x.SmartBorder = parser.ParseBoolean() },
            { "AlwaysCircular", (parser, x) => x.AlwaysCircular = parser.ParseBoolean() }
        };

        public string TriggeredBy { get; private set; }
        public string MineName { get; private set; }
        public bool SmartBorder { get; private set; }
        public bool AlwaysCircular { get; private set; }
    }
}

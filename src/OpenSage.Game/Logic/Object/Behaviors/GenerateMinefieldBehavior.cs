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
            { "DistanceAroundObject", (parser, x) => x.DistanceAroundObject = parser.ParseInteger() },
            { "GenerateOnlyOnDeath", (parser, x) => x.GenerateOnlyOnDeath = parser.ParseBoolean() },
            { "SmartBorder", (parser, x) => x.SmartBorder = parser.ParseBoolean() },
            { "SmartBorderSkipInterior", (parser, x) => x.SmartBorderSkipInterior = parser.ParseBoolean() },
            { "AlwaysCircular", (parser, x) => x.AlwaysCircular = parser.ParseBoolean() },
            { "GenerationFX", (parser, x) => x.GenerationFX = parser.ParseAssetReference() }
        };

        public string TriggeredBy { get; private set; }
        public string MineName { get; private set; }
        public int DistanceAroundObject { get; private set; }
        public bool GenerateOnlyOnDeath { get; private set; }
        public bool SmartBorder { get; private set; }
        public bool SmartBorderSkipInterior { get; private set; }
        public bool AlwaysCircular { get; private set; }
        public string GenerationFX { get; private set; }
    }
}

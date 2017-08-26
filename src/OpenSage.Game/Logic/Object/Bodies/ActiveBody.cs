using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class ActiveBody : ObjectBody
    {
        internal static ActiveBody Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ActiveBody> FieldParseTable = new IniParseTable<ActiveBody>
        {
            { "MaxHealth", (parser, x) => x.MaxHealth = parser.ParseFloat() },
            { "InitialHealth", (parser, x) => x.InitialHealth = parser.ParseFloat() }
        }.Concat<ActiveBody, ObjectBody>(BodyFieldParseTable);

        public float MaxHealth { get; private set; }
        public float InitialHealth { get; private set; }
    }
}

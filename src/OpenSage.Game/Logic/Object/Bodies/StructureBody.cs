using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Used by objects with STRUCTURE and IMMOBILE KindOfs defined.
    /// </summary>
    public sealed class StructureBody : ObjectBody
    {
        internal static StructureBody Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StructureBody> FieldParseTable = new IniParseTable<StructureBody>
        {
            { "MaxHealth", (parser, x) => x.MaxHealth = parser.ParseFloat() },
            { "InitialHealth", (parser, x) => x.InitialHealth = parser.ParseFloat() }
        }.Concat<StructureBody, ObjectBody>(BodyFieldParseTable);

        public float MaxHealth { get; private set; }
        public float InitialHealth { get; private set; }
    }
}

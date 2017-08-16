using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Prevents the object from dying or taking damage.
    /// </summary>
    public sealed class ImmortalBody : ObjectBody
    {
        internal static ImmortalBody Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ImmortalBody> FieldParseTable = new IniParseTable<ImmortalBody>
        {
            { "MaxHealth", (parser, x) => x.MaxHealth = parser.ParseFloat() },
            { "InitialHealth", (parser, x) => x.InitialHealth = parser.ParseFloat() }
        };

        public float MaxHealth { get; private set; }
        public float InitialHealth { get; private set; }
    }
}

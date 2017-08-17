using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class VeterancyCrateCollide : ObjectBehavior
    {
        internal static VeterancyCrateCollide Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<VeterancyCrateCollide> FieldParseTable = new IniParseTable<VeterancyCrateCollide>
        {
            { "ForbiddenKindOf", (parser, x) => x.ForbiddenKindOf = parser.ParseEnum<ObjectKinds>() },
            { "EffectRange", (parser, x) => x.EffectRange = parser.ParseInteger() }
        };

        public ObjectKinds ForbiddenKindOf { get; private set; }
        public int EffectRange { get; private set; }
    }
}

using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class VeterancyCrateCollideBehavior : ObjectBehavior
    {
        internal static VeterancyCrateCollideBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<VeterancyCrateCollideBehavior> FieldParseTable = new IniParseTable<VeterancyCrateCollideBehavior>
        {
            { "ForbiddenKindOf", (parser, x) => x.ForbiddenKindOf = parser.ParseEnum<ObjectKinds>() },
            { "EffectRange", (parser, x) => x.EffectRange = parser.ParseInteger() }
        };

        public ObjectKinds ForbiddenKindOf { get; private set; }
        public int EffectRange { get; private set; }
    }
}

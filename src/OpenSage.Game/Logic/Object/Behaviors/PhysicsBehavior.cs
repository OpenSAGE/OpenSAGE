using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class PhysicsBehavior : ObjectBehavior
    {
        internal static PhysicsBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<PhysicsBehavior> FieldParseTable = new IniParseTable<PhysicsBehavior>
        {
            { "Mass", (parser, x) => x.Mass = parser.ParseFloat() },
            { "AllowBouncing", (parser, x) => x.AllowBouncing = parser.ParseBoolean() },
            { "KillWhenRestingOnGround", (parser, x) => x.KillWhenRestingOnGround = parser.ParseBoolean() },
        };

        public float Mass { get; private set; }
        public bool AllowBouncing { get; private set; }
        public bool KillWhenRestingOnGround { get; private set; }
    }
}

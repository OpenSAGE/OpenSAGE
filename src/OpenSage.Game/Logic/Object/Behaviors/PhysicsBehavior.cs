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
            { "AerodynamicFriction", (parser, x) => x.AerodynamicFriction = parser.ParseInteger() },
            { "ForwardFriction", (parser, x) => x.ForwardFriction = parser.ParseInteger() },
            { "CenterOfMassOffset", (parser, x) => x.CenterOfMassOffset = parser.ParseInteger() },
            { "AllowBouncing", (parser, x) => x.AllowBouncing = parser.ParseBoolean() },
            { "KillWhenRestingOnGround", (parser, x) => x.KillWhenRestingOnGround = parser.ParseBoolean() },
            { "AllowCollideForce", (parser, x) => x.AllowCollideForce = parser.ParseBoolean() },
        };

        public float Mass { get; private set; }
        public int AerodynamicFriction { get; private set; }
        public int ForwardFriction { get; private set; }
        public int CenterOfMassOffset { get; private set; }
        public bool AllowBouncing { get; private set; }
        public bool KillWhenRestingOnGround { get; private set; }
        public bool AllowCollideForce { get; private set; } = true;
    }
}

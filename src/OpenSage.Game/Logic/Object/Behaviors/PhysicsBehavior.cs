using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class PhysicsBehaviorModuleData : UpdateModuleData
    {
        internal static PhysicsBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PhysicsBehaviorModuleData> FieldParseTable = new IniParseTable<PhysicsBehaviorModuleData>
        {
            { "Mass", (parser, x) => x.Mass = parser.ParseFloat() },
            { "AerodynamicFriction", (parser, x) => x.AerodynamicFriction = parser.ParseFloat() },
            { "ForwardFriction", (parser, x) => x.ForwardFriction = parser.ParseFloat() },
            { "CenterOfMassOffset", (parser, x) => x.CenterOfMassOffset = parser.ParseFloat() },
            { "AllowBouncing", (parser, x) => x.AllowBouncing = parser.ParseBoolean() },
            { "KillWhenRestingOnGround", (parser, x) => x.KillWhenRestingOnGround = parser.ParseBoolean() },
            { "AllowCollideForce", (parser, x) => x.AllowCollideForce = parser.ParseBoolean() },
            { "GravityMult", (parser, x) => x.GravityMult = parser.ParseFloat() },
        };

        public float Mass { get; private set; }
        public float AerodynamicFriction { get; private set; }
        public float ForwardFriction { get; private set; }
        public float CenterOfMassOffset { get; private set; }
        public bool AllowBouncing { get; private set; }
        public bool KillWhenRestingOnGround { get; private set; }
        public bool AllowCollideForce { get; private set; } = true;

        [AddedIn(SageGame.Bfme)]
        public float GravityMult { get; private set; }
    }
}

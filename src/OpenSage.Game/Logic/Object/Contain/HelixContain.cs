using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Like Transport, but when full, passes transport queries along to first passenger 
    /// (redirects like tunnel). Basically works like the Overlord Contain.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class HelixContain : ObjectBehavior
    {
        internal static HelixContain Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<HelixContain> FieldParseTable = new IniParseTable<HelixContain>
        {
            { "Slots", (parser, x) => x.Slots = parser.ParseInteger() },
            { "DamagePercentToUnits", (parser, x) => x.DamagePercentToUnits = parser.ParsePercentage() },
            { "AllowInsideKindOf", (parser, x) => x.AllowInsideKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "ForbidInsideKindOf", (parser, x) => x.ForbidInsideKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "PassengersAllowedToFire", (parser, x) => x.PassengersAllowedToFire = parser.ParseBoolean() },
            //{ "PassengersInTurret", (parser, x) => x.PassengersInTurret = parser.ParseBoolean() }
            { "ShouldDrawPips", (parser, x) => x.ShouldDrawPips = parser.ParseBoolean() },
            { "ExitDelay", (parser, x) => x.ExitDelay = parser.ParseInteger() },
            { "NumberOfExitPaths", (parser, x) => x.NumberOfExitPaths = parser.ParseInteger() },
        };

        public int Slots { get; private set; }
        public float DamagePercentToUnits { get; private set; }
        public BitArray<ObjectKinds> AllowInsideKindOf { get; private set; }
        public BitArray<ObjectKinds> ForbidInsideKindOf { get; private set; }
        public bool PassengersAllowedToFire { get; private set; }
        //public bool PassengersInTurret { get; private set; }
        public bool ShouldDrawPips { get; private set; }
        public int ExitDelay { get; private set; }
        public int NumberOfExitPaths { get; private set; }
    }
}

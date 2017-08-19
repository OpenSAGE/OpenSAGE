using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Automatically heals and restores the health of units that enter or exit the object.
    /// </summary>
    public sealed class HealContain : ObjectBehavior
    {
        internal static HealContain Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<HealContain> FieldParseTable = new IniParseTable<HealContain>
        {
            { "ContainMax", (parser, x) => x.ContainMax = parser.ParseInteger() },
            { "TimeForFullHeal", (parser, x) => x.TimeForFullHeal = parser.ParseInteger() },
            { "AllowInsideKindOf", (parser, x) => x.AllowInsideKindOf = parser.ParseEnum<ObjectKinds>() },
            { "AllowAlliesInside", (parser, x) => x.AllowAlliesInside = parser.ParseBoolean() },
            { "AllowNeutralInside", (parser, x) => x.AllowNeutralInside = parser.ParseBoolean() },
            { "AllowEnemiesInside", (parser, x) => x.AllowEnemiesInside = parser.ParseBoolean() }
        };

        public int ContainMax { get; private set; }
        public int TimeForFullHeal { get; private set; }
        public ObjectKinds AllowInsideKindOf { get; private set; }
        public bool AllowAlliesInside { get; private set; }
        public bool AllowNeutralInside { get; private set; }
        public bool AllowEnemiesInside { get; private set; }
    }
}

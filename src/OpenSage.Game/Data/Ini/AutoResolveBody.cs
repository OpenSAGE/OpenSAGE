using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AutoResolveBody
    {
        internal static AutoResolveBody Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AutoResolveBody> FieldParseTable = new IniParseTable<AutoResolveBody>
        {
            { "HitpointsAtLevel", (parser, x) => x.HitpointsAtLevels.Add(HitpointsAtLevel.Parse(parser)) },
            { "LeaveInArmySummary", (parser, x) => x.LeaveInArmySummary = parser.ParseBoolean() },
            { "CanBeAttacked", (parser, x) => x.CanBeAttacked = parser.ParseBoolean() }
        };

        public string Name { get; private set; }

        public List<HitpointsAtLevel> HitpointsAtLevels { get; } = new List<HitpointsAtLevel>();
        public bool LeaveInArmySummary { get; private set; }
        public bool CanBeAttacked { get; private set; }
    }

    public sealed class HitpointsAtLevel
    {
        internal static HitpointsAtLevel Parse(IniParser parser)
        {
            return new HitpointsAtLevel
            {
                Hitpoints = parser.ParseAttributeInteger("Hitpoints"),
                Level = parser.ParseAttributeInteger("Level")
            };
        }

        public int Hitpoints { get; private set; }
        public int Level { get; private set; }
    }
}

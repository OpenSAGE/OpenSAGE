using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class Armor
    {
        internal static Armor Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Armor> FieldParseTable = new IniParseTable<Armor>
        {
            { "DamageScalar", (parser, x) => x.DamageScalar = parser.ParsePercentage() },
            { "Armor", (parser, x) => x.Values.Add(ArmorValue.Parse(parser)) }
        };

        public string Name { get; private set; }

        /// <summary>
        /// Scales all damage done to this unit.
        /// </summary>
        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float DamageScalar { get; private set; }

        public List<ArmorValue> Values { get; } = new List<ArmorValue>();
    }

    public sealed class ArmorValue
    {
        internal static ArmorValue Parse(IniParser parser)
        {
            return new ArmorValue
            {
                DamageType = parser.ParseEnum<DamageType>(),
                Percent = parser.ParsePercentage()
            };
        }

        public DamageType DamageType { get; internal set; }
        public float Percent { get; internal set; }
    }
}

using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object.Damage;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class Armor
    {
        internal static Armor Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Armor> FieldParseTable = new IniParseTable<Armor>
        {
            { "DamageScalar", (parser, x) => x.DamageScalar = parser.ParsePercentage() },
            { "Armor", (parser, x) => x.Values.Add(ArmorValue.Parse(parser)) },
            { "FlankedPenalty", (parser, x) => x.FlankedPenalty = parser.ParsePercentage() }
        };

        public string Name { get; private set; }

        /// <summary>
        /// Scales all damage done to this unit.
        /// </summary>
        [AddedIn(SageGame.Bfme)]
        public Percentage DamageScalar { get; private set; }

        public List<ArmorValue> Values { get; } = new List<ArmorValue>();

        [AddedIn(SageGame.Bfme2)]
        public Percentage FlankedPenalty { get; private set; }
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

        public DamageType DamageType { get; private set; }
        public Percentage Percent { get; private set; }
    }
}

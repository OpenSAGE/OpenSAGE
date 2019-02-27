using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class Armor
    {
        internal static void Parse(IniParser parser, IniDataContext context) => parser.ParseBlockContent(
            (x, name) => x.Name = name,
            context.Armors,
            FieldParseTable);

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
        public float DamageScalar { get; private set; }

        public List<ArmorValue> Values { get; } = new List<ArmorValue>();

        [AddedIn(SageGame.Bfme2)]
        public float FlankedPenalty { get; private set; }
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
        public float Percent { get; private set; }
    }
}

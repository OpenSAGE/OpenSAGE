using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class ArmorTemplate : BaseAsset
    {
        internal static ArmorTemplate Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("Armor", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<ArmorTemplate> FieldParseTable = new IniParseTable<ArmorTemplate>
        {
            { "DamageScalar", (parser, x) => x.DamageScalar = parser.ParsePercentage() },
            { "Armor", (parser, x) => { var armorValue = ArmorValue.Parse(parser); x.Values.Add(armorValue.DamageType, armorValue); } },
            { "FlankedPenalty", (parser, x) => x.FlankedPenalty = parser.ParsePercentage() }
        };

        /// <summary>
        /// Scales all damage done to this unit.
        /// </summary>
        [AddedIn(SageGame.Bfme)]
        public Percentage DamageScalar { get; private set; }

        public Dictionary<DamageType, ArmorValue> Values { get; } = new Dictionary<DamageType, ArmorValue>();

        [AddedIn(SageGame.Bfme2)]
        public Percentage FlankedPenalty { get; private set; }

        internal Percentage GetDamagePercent(DamageType damageType)
        {
            if (Values.TryGetValue(damageType, out var result))
            {
                return result.Percent;
            }

            if (Values.TryGetValue(DamageType.Default, out result))
            {
                return result.Percent;
            }

            return new Percentage(1);
        }
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

        /// <summary>
        /// If this is 0%, the object will not be harmed at all by the specified <see cref="DamageType"/>.
        /// If this is 100%, the object will receive the full damage amount.
        /// </summary>
        public Percentage Percent { get; private set; }
    }
}

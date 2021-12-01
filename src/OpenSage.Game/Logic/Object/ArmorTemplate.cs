using System;
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
            {
                "Armor",
                (parser, x) =>
                {
                    var damageTypeString = parser.ParseString();
                    var percent = parser.ParsePercentage();

                    if (string.Equals(damageTypeString, "DEFAULT", StringComparison.InvariantCultureIgnoreCase))
                    {
                        for (var i = 0; i < x.Values.Length; i++)
                        {
                            x.Values[i] = percent;
                        }
                    }
                    else
                    {
                        var damageType = IniParser.ParseEnum<DamageType>(damageTypeString);
                        x.Values[(int)damageType] = percent;
                    }
                    
                }
            },
            { "FlankedPenalty", (parser, x) => x.FlankedPenalty = parser.ParsePercentage() }
        };

        /// <summary>
        /// Scales all damage done to this unit.
        /// </summary>
        [AddedIn(SageGame.Bfme)]
        public Percentage DamageScalar { get; private set; }

        public Percentage[] Values { get; } = new Percentage[Enum.GetValues(typeof(DamageType)).Length];

        [AddedIn(SageGame.Bfme2)]
        public Percentage FlankedPenalty { get; private set; }

        /// <summary>
        /// If this is 0%, the object will not be harmed at all by the specified <see cref="DamageType"/>.
        /// If this is 100%, the object will receive the full damage amount.
        /// </summary>
        internal Percentage GetDamagePercent(DamageType damageType)
        {
            return Values[(int) damageType];
        }
    }
}

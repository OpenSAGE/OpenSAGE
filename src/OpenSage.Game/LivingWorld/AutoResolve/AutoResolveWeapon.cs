using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.LivingWorld.AutoResolve
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AutoResolveWeapon
    {
        internal static AutoResolveWeapon Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AutoResolveWeapon> FieldParseTable = new IniParseTable<AutoResolveWeapon>
        {
            { "MissPercentChance", (parser, x) => x.MissPercentChance = parser.ParseInteger() },
            { "ReduceAttackWhenHurt", (parser, x) => x.ReduceAttackWhenHurt = parser.ParseBoolean() },
            { "DamagePerRound", (parser, x) => x.DamagesPerRound.Add(DamagePerRound.Parse(parser)) },
            { "LevelBonus", (parser, x) => x.LevelBonuses.Add(LevelBonus.Parse(parser)) }
        };

        public string Name { get; private set; }

        public int MissPercentChance { get; private set; }
        public bool ReduceAttackWhenHurt { get; private set; }
        public List<DamagePerRound> DamagesPerRound { get; } = new List<DamagePerRound>();
        public List<LevelBonus> LevelBonuses { get; } = new List<LevelBonus>();
    }

    public class DamagePerRound
    {
        internal static DamagePerRound Parse(IniParser parser)
        {
            return new DamagePerRound
            {
                Damage = parser.ParseAttributeInteger("Damage"),
                Against = parser.ParseAttributeIdentifier("Against")
            };
        }

        public int Damage { get; private set; }
        public string Against { get; private set; }
    }

    public class LevelBonus
    {
        internal static LevelBonus Parse(IniParser parser)
        {
            return new LevelBonus
            {
                Level = parser.ParseAttributeInteger("Level"),
                Bonus = parser.ParseAttributeFloat("Bonus")
            };
        }

        public int Level { get; private set; }
        public float Bonus { get; private set; }
    }
}

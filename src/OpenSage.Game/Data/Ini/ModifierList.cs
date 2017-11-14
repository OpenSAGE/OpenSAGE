using OpenSage.Data.Ini.Parser;
using System.Collections.Generic;

namespace OpenSage.Data.Ini
{
    /// <summary>
    /// A set of bonuses that can given together as a package.
    /// </summary>
    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class ModifierList
    {
        internal static ModifierList Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<ModifierList> FieldParseTable = new IniParseTable<ModifierList>
        {
            { "Category", (parser, x) => x.Category = parser.ParseEnum<ModifierCategory>() },
            { "Modifier", (parser, x) => x.Modifiers.Add(Modifier.Parse(parser)) },
            { "Duration", (parser, x) => x.Duration = parser.ParseInteger() },
        };

        public string Name { get; private set; }

        public ModifierCategory Category { get; private set; }
        public List<Modifier> Modifiers { get; } = new List<Modifier>();
        public int Duration { get; private set; }
    }

    public struct Modifier
    {
        internal static Modifier Parse(IniParser parser)
        {
            return new Modifier
            {
                ModifierType = parser.ParseEnum<ModifierType>(),
                Amount = parser.ParsePercentage()
            };
        }

        public ModifierType ModifierType;
        public float Amount;
    }

    [AddedIn(SageGame.BattleForMiddleEarth)]
    public enum ModifierCategory
    {
        [IniEnum("LEADERSHIP")]
        Leadership,

        [IniEnum("SPELL")]
        Spell,

        [IniEnum("FORMATION")]
        Formation,

        [IniEnum("WEAPON")]
        Weapon,

        [IniEnum("STRUCTURE")]
        Structure,

        [IniEnum("LEVEL")]
        Level
    }

    [AddedIn(SageGame.BattleForMiddleEarth)]
    public enum ModifierType
    {
        [IniEnum("ARMOR")]
        Armor,

        [IniEnum("DAMAGE_ADD")]
        DamageAdd,

        [IniEnum("DAMAGE_MULT")]
        DamageMult,

        [IniEnum("SPELL_DAMAGE")]
        SpellDamage,

        [IniEnum("RESIST_FEAR")]
        ResistFear,

        [IniEnum("EXPERIENCE")]
        Experience,

        [IniEnum("RANGE")]
        Range,

        [IniEnum("SPEED")]
        Speed,

        [IniEnum("CRUSH_DECELERATE")]
        CrushDecelerate,

        [IniEnum("RESIST_KNOCKBACK")]
        ResistKnockback,

        [IniEnum("RECHARGE_TIME")]
        RechargeTime,

        [IniEnum("PRODUCTION")]
        Production,

        [IniEnum("HEALTH")]
        Health,

        [IniEnum("VISION")]
        Vision,

        [IniEnum("AUTO_HEAL")]
        AutoHeal
    }
}

using OpenSage.Data.Ini.Parser;
using OpenSage.Logic.Object;
using System.Collections.Generic;

namespace OpenSage.Data.Ini
{
    /// <summary>
    /// A set of bonuses that can given together as a package.
    /// </summary>
    [AddedIn(SageGame.Bfme)]
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
            { "ClearModelCondition", (parser, x) => x.ClearModelCondition = parser.ParseEnum<ModelConditionFlag>() },
            { "ModelCondition", (parser, x) => x.ModelCondition = parser.ParseEnum<ModelConditionFlag>() },
            { "FX", (parser, x) => x.FX = parser.ParseAssetReference() },
            { "FX2", (parser, x) => x.FX2 = parser.ParseAssetReference() },
            { "FX3", (parser, x) => x.FX3 = parser.ParseAssetReference() },
            { "EndFX", (parser, x) => x.EndFX = parser.ParseAssetReference() },
            { "EndFX2", (parser, x) => x.EndFX2 = parser.ParseAssetReference() },
            { "EndFX3", (parser, x) => x.EndFX3 = parser.ParseAssetReference() },
            { "MultiLevelFX", (parser, x) => x.MultiLevelFX = parser.ParseBoolean() },
            { "Upgrade", (parser, x) => x.Upgrade = ModifierUpgrade.Parse(parser) },
        };

        public string Name { get; private set; }

        public ModifierCategory Category { get; private set; }
        public List<Modifier> Modifiers { get; } = new List<Modifier>();
        public int Duration { get; private set; }
        public ModelConditionFlag ClearModelCondition { get; private set; }
        public ModelConditionFlag ModelCondition { get; private set; }
        public string FX { get; private set; }
        public string FX2 { get; private set; }
        public string FX3 { get; private set; }
        public string EndFX { get; private set; }
        public string EndFX2 { get; private set; }
        public string EndFX3 { get; private set; }
        public bool MultiLevelFX { get; private set; }
        public ModifierUpgrade? Upgrade { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
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

    [AddedIn(SageGame.Bfme)]
    public struct ModifierUpgrade
    {
        internal static ModifierUpgrade Parse(IniParser parser)
        {
            return new ModifierUpgrade
            {
                Upgrade = parser.ParseAssetReference(),
                Delay = parser.ParseAttributeInteger("Delay")
            };
        }

        public string Upgrade;
        public int Delay;
    }

    [AddedIn(SageGame.Bfme)]
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

    [AddedIn(SageGame.Bfme)]
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
        AutoHeal,

        [IniEnum("BOUNTY_PERCENTAGE")]
        BountyPercentage,

        [IniEnum("MINIMUM_CRUSH_VELOCITY")]
        MinimumCrushVelocity
    }
}

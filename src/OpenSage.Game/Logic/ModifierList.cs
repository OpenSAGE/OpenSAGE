using OpenSage.Data.Ini;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using System.Collections.Generic;

namespace OpenSage.Logic
{
    /// <summary>
    /// A set of bonuses that can given together as a package.
    /// </summary>
    [AddedIn(SageGame.Bfme)]
    public sealed class ModifierList
    {
        internal static ModifierList Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<ModifierList> FieldParseTable = new IniParseTable<ModifierList>
        {
            { "Category", (parser, x) => x.Category = parser.ParseEnum<ModifierCategory>() },
            { "Modifier", (parser, x) => x.Modifiers.Add(Modifier.Parse(parser)) },
            { "Duration", (parser, x) => x.Duration = parser.ParseLong() },
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
            { "ReplaceInCategoryIfLongest", (parser, x) => x.ReplaceInCategoryIfLongest = parser.ParseBoolean() },
            { "IgnoreIfAnticategoryActive", (parser, x) => x.IgnoreIfAnticategoryActive = parser.ParseBoolean() }
        };

        public string Name { get; private set; }

        public ModifierCategory Category { get; private set; }
        public List<Modifier> Modifiers { get; } = new List<Modifier>();
        public long Duration { get; private set; }
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

        [AddedIn(SageGame.Bfme2)]
        public bool ReplaceInCategoryIfLongest { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool IgnoreIfAnticategoryActive { get; private set; }
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
        public Percentage Amount;
    }

    [AddedIn(SageGame.Bfme)]
    public struct ModifierUpgrade
    {
        internal static ModifierUpgrade Parse(IniParser parser)
        {
            var upgrades = new List<string>();
            var token = parser.PeekNextTokenOptional();
            while (token.HasValue && !token.Value.Text.Contains(":"))
            {
                upgrades.Add(parser.ParseAssetReference());
                token = parser.PeekNextTokenOptional();
            }

            return new ModifierUpgrade
            {
                Upgrades = upgrades.ToArray(),
                Delay = parser.ParseAttributeInteger("Delay")
            };
        }

        public string[] Upgrades;
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
        Level,

        [IniEnum("BUFF")]
        Buff,

        [IniEnum("DEBUFF"), AddedIn(SageGame.Bfme2)]
        Debuff,

        [IniEnum("INNATE_VISION"), AddedIn(SageGame.Bfme2)]
        InnateVision,

        [IniEnum("INNATE_ARMOR"), AddedIn(SageGame.Bfme2)]
        InnateArmor,

        [IniEnum("INNATE_AUTOHEAL"), AddedIn(SageGame.Bfme2)]
        InnateAutoheal,

        [IniEnum("INNATE_HEALTH"), AddedIn(SageGame.Bfme2)]
        InnateHealth,

        [IniEnum("INNATE_DAMAGEMULT"), AddedIn(SageGame.Bfme2Rotwk)]
        InnateDamagemult,

        [IniEnum("STUN"), AddedIn(SageGame.Bfme2Rotwk)]
        Stun,
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
        MinimumCrushVelocity,

        [IniEnum("INVULNERABLE"), AddedIn(SageGame.Bfme2)]
        Invulnerable,

        [IniEnum("CRUSHER_LEVEL"), AddedIn(SageGame.Bfme2)]
        CrusherLevel,

        [IniEnum("SHROUD_CLEARING"), AddedIn(SageGame.Bfme2)]
        ShroudClearing,

        [IniEnum("RATE_OF_FIRE"), AddedIn(SageGame.Bfme2)]
        RateOfFire,

        [IniEnum("CRUSHED_DECELERATE"), AddedIn(SageGame.Bfme2)]
        CrushedDecelerate,

        [IniEnum("COMMAND_POINT_BONUS"), AddedIn(SageGame.Bfme2)]
        CommandPointBonus,

        [IniEnum("HEALTH_MULT"), AddedIn(SageGame.Bfme2)]
        HealthMult,

        [IniEnum("RESIST_TERROR"), AddedIn(SageGame.Bfme2Rotwk)]
        ResistTerror,

        [IniEnum("CRUSHABLE_LEVEL"), AddedIn(SageGame.Bfme2Rotwk)]
        CrushableLevel,

        [IniEnum("DAMAGE_STRUCTURE_BOUNTY_ADD"), AddedIn(SageGame.Bfme2Rotwk)]
        DamageStructureBountyAdd,
    }
}

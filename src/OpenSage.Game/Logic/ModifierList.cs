using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using FixedMath.NET;
using System;
using System.Collections.Generic;

namespace OpenSage.Logic
{
    [AddedIn(SageGame.Bfme)]
    public class AttributeModifier
    {
        private readonly ModifierList _modifierList;
        private TimeSpan _activeUntil;
        private readonly bool _selfExpiring;

        private List<(UpgradeTemplate, TimeSpan)> _delayedUpgrades;

        public bool Applied { get; private set; }
        public bool Invalid { get; set; }

        public AttributeModifier(ModifierList modifierList)
        {
            _modifierList = modifierList;
            _selfExpiring = _modifierList.Duration > 0;
            Invalid = false;
            Applied = false;
            _delayedUpgrades = new List<(UpgradeTemplate, TimeSpan)>();
        }

        public void Apply(GameObject gameObject, in TimeInterval time)
        {
            if (_selfExpiring)
            {
                _activeUntil = time.TotalTime + TimeSpan.FromMilliseconds(_modifierList.Duration);
            }

            foreach (var modifier in _modifierList.Modifiers)
            {
                switch (modifier.ModifierType)
                {
                    case ModifierType.Production:
                        gameObject.ProductionModifier *= modifier.Amount;
                        break;
                    case ModifierType.Health:
                        var amount = (Fix64) (int) (modifier.Amount * 100);
                        gameObject.HealthModifier += amount;
                        gameObject.Health += amount;
                        break;
                }
            }

            if (_modifierList.Upgrade != null)
            {
                foreach (var upgrade in _modifierList.Upgrade.Upgrades)
                {
                    if (_modifierList.Upgrade.Delay == 0)
                    {
                        gameObject.Upgrade(upgrade.Value);
                    }
                    else
                    {
                        var activatesAt = time.TotalTime + TimeSpan.FromMilliseconds(_modifierList.Upgrade.Delay);
                        _delayedUpgrades.Add((upgrade.Value, activatesAt));
                    }
                }
            }

            TriggerFX(_modifierList.FX, gameObject);
            TriggerFX(_modifierList.FX2, gameObject);
            TriggerFX(_modifierList.FX3, gameObject);

            Applied = true;
        }

        public bool Expired(in TimeInterval time)
        {
            if (!_selfExpiring)
            {
                return false;
            }

            return time.TotalTime > _activeUntil;
        }

        public void Update(GameObject gameObject, in TimeInterval time)
        {
            for (var i = 0; i < _delayedUpgrades.Count; i++)
            {
                var (upgrade, activatesAt) = _delayedUpgrades[i];
                if (time.TotalTime > activatesAt)
                {
                    gameObject.Upgrade(upgrade);
                    _delayedUpgrades.RemoveAt(i);
                }
            }
        }

        public void Remove(GameObject gameObject)
        {
            foreach (var modifier in _modifierList.Modifiers)
            {
                switch (modifier.ModifierType)
                {
                    case ModifierType.Production:
                        gameObject.ProductionModifier /= modifier.Amount;
                        break;
                    case ModifierType.Health:
                        gameObject.HealthModifier -= (Fix64) (int) (modifier.Amount * 100);
                        // TODO: also reduce health again by this amount?
                        break;
                }
            }

            if (_modifierList.Upgrade != null)
            {
                foreach (var upgrade in _modifierList.Upgrade.Upgrades)
                {
                    gameObject.RemoveUpgrade(upgrade.Value);
                }
            }

            TriggerFX(_modifierList.EndFX, gameObject);
            TriggerFX(_modifierList.EndFX2, gameObject);
            TriggerFX(_modifierList.EndFX3, gameObject);
        }

        private void TriggerFX(LazyAssetReference<FXList> fx, GameObject gameObject)
        {
            fx?.Value?.Execute(new FXListExecutionContext(
                    gameObject.Rotation,
                    gameObject.Translation,
                    gameObject.GameContext));
        }
    }

    /// <summary>
    /// A set of bonuses that can given together as a package.
    /// </summary>
    [AddedIn(SageGame.Bfme)]
    public sealed class ModifierList : BaseAsset
    {
        // A ModifierList is a set of bonuses that can be given as a package.  You can't ever be given the same list
        // twice at the same time, but you can have two different lists that have the same effect.
        internal static ModifierList Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("ModifierList", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<ModifierList> FieldParseTable = new IniParseTable<ModifierList>
        {
            { "Category", (parser, x) => x.Category = parser.ParseEnum<ModifierCategory>() },
            { "Modifier", (parser, x) => x.Modifiers.Add(Modifier.Parse(parser)) },
            { "Duration", (parser, x) => x.Duration = parser.ParseLong() },
            { "ClearModelCondition", (parser, x) => x.ClearModelCondition = parser.ParseEnum<ModelConditionFlag>() },
            { "ModelCondition", (parser, x) => x.ModelCondition = parser.ParseEnum<ModelConditionFlag>() },
            { "FX", (parser, x) => x.FX = parser.ParseFXListReference() },
            { "FX2", (parser, x) => x.FX2 = parser.ParseFXListReference() },
            { "FX3", (parser, x) => x.FX3 = parser.ParseFXListReference() },
            { "EndFX", (parser, x) => x.EndFX = parser.ParseFXListReference() },
            { "EndFX2", (parser, x) => x.EndFX2 = parser.ParseFXListReference() },
            { "EndFX3", (parser, x) => x.EndFX3 = parser.ParseFXListReference() },
            { "MultiLevelFX", (parser, x) => x.MultiLevelFX = parser.ParseBoolean() },
            { "Upgrade", (parser, x) => x.Upgrade = ModifierUpgrade.Parse(parser) },
            { "ReplaceInCategoryIfLongest", (parser, x) => x.ReplaceInCategoryIfLongest = parser.ParseBoolean() },
            { "IgnoreIfAnticategoryActive", (parser, x) => x.IgnoreIfAnticategoryActive = parser.ParseBoolean() }
        };

        // Category = LEADERSHIP, SPELL, FORMATION, WEAPON, STRUCTURE, LEVEL
        // The reason you have this bonus.So things can affect all Leadership bonuses or dispel all spell effects
        public ModifierCategory Category { get; private set; }
        public List<Modifier> Modifiers { get; } = new List<Modifier>();
        public long Duration { get; private set; }
        public ModelConditionFlag ClearModelCondition { get; private set; }
        public ModelConditionFlag ModelCondition { get; private set; }
        public LazyAssetReference<FXList> FX { get; private set; }
        public LazyAssetReference<FXList> FX2 { get; private set; }
        public LazyAssetReference<FXList> FX3 { get; private set; }
        public LazyAssetReference<FXList> EndFX { get; private set; }
        public LazyAssetReference<FXList> EndFX2 { get; private set; }
        public LazyAssetReference<FXList> EndFX3 { get; private set; }
        public bool MultiLevelFX { get; private set; }
        public ModifierUpgrade Upgrade { get; private set; }

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
    public class ModifierUpgrade
    {
        internal static ModifierUpgrade Parse(IniParser parser)
        {
            var upgrades = new List<LazyAssetReference<UpgradeTemplate>>();
            var token = parser.PeekNextTokenOptional();
            while (token.HasValue && !token.Value.Text.Contains(":"))
            {
                upgrades.Add(parser.ParseUpgradeReference());
                token = parser.PeekNextTokenOptional();
            }

            return new ModifierUpgrade
            {
                Upgrades = upgrades.ToArray(),
                Delay = parser.ParseAttributeInteger("Delay")
            };
        }

        public LazyAssetReference<UpgradeTemplate>[] Upgrades;
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
        // Additive.  The armor coefficients in Armor.ini go first to multiply the damage.  Then all of these are added together, capped at
        // GameData's AttributeModifierArmorMaxBonus protection, and then the damage is multiplied by it again.
        Armor,

        [IniEnum("DAMAGE_ADD")]
        // Additive.  'Base' damage gets increased by this before hitting the DamageMult.
        DamageAdd,

        [IniEnum("DAMAGE_MULT")]
        // Multiplicitive.  Then after DamageAdd, the damage is multiplied by all of these.
        DamageMult,

        [IniEnum("SPELL_DAMAGE")]
        // Multiplicitive.  Just like DamageMult bonus, but only applies if damage type is Magic.  REPLACES DamageMult bonus.
        SpellDamage,

        [IniEnum("RESIST_FEAR")]
        // Additive.  Sum of these is a saving throw against fear
        ResistFear,

        [IniEnum("EXPERIENCE")]
        // Multiplicitive.  Experience gained multiplied by this, will compound in multiple bonuses
        Experience,

        [IniEnum("RANGE")]
        // Additive.  Sum of these added to max range.  20% and 10% makes range 130% normal.  (You probably want a vision range boost for targeting too.)
        Range,

        [IniEnum("SPEED")]
        // Multiplicitive.  Multiply your speed by each of these numbers in turn.
        Speed,

        [IniEnum("CRUSH_DECELERATE")]
        // Multiplicitive.  The percentage you slow down when crushing gets multiplied by each of these.
        CrushDecelerate,

        [IniEnum("RESIST_KNOCKBACK")]
        // Additive.  Sum of these is saving through against knockback.
        ResistKnockback,

        [IniEnum("RECHARGE_TIME")]
        // Multiplicitive.  Recharge time for all special powers multiplied by these.
        // Time is figured at the moment power is used, so this has no effect if gained or lost while power is recharging.
        RechargeTime,

        [IniEnum("PRODUCTION")]
        // Multiplicitive.  Production speed for units and money amount produced by supply centers or money generators multiplied by these.
        // Again, time is computed at moment production starts.
        Production,

        [IniEnum("HEALTH")]
        // Additive.  The moment you get this upgrade, this many hitpoints are added to both your max and current hitpoint scores.
        Health,

        [IniEnum("VISION")]
        // Additive.  Sum of these is added to vision range, which is used for targeting.
        Vision,

        [IniEnum("AUTO_HEAL")]
        // Additive. Sum of these is added to the AutoHeal value.
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

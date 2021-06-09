using OpenSage.Data.Ini;
using OpenSage.Mathematics;
using FixedMath.NET;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Just does damage.
    /// </summary>
    [AddedIn(SageGame.Bfme)]
    public class DamageNugget : WeaponEffectNugget
    {
        internal static DamageNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private protected static new readonly IniParseTable<DamageNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<DamageNugget>
            {
                { "Damage", (parser, x) => x.Damage = parser.ParseFloat() },
                { "Radius", (parser, x) => x.Radius = parser.ParseFloat() },
                { "DelayTime", (parser, x) => x.DelayTime = parser.ParseInteger() },
                { "DamageType", (parser, x) => x.DamageType = parser.ParseEnum<DamageType>() },
                { "DamageFXType", (parser, x) => x.DamageFXType = parser.ParseEnum<DamageFXType>() },
                { "DeathType", (parser, x) => x.DeathType = parser.ParseEnum<DeathType>() },
                { "DamageSpeed", (parser, x) => x.DamageSpeed = parser.ParseFloat() },
                { "DamageArc", (parser, x) => x.DamageArc = parser.ParseInteger() },
                { "DamageScalar", (parser, x) => x.DamageScalar = DamageScalar.Parse(parser) },
                { "DamageMaxHeight", (parser, x) => x.DamageMaxHeight = parser.ParseInteger() },
                { "AcceptDamageAdd", (parser, x) => x.AcceptDamageAdd = parser.ParseBoolean() },
                { "FlankingBonus", (parser, x) => x.FlankingBonus = parser.ParsePercentage() },
                { "DamageTaperOff", (parser, x) => x.DamageTaperOff = parser.ParseInteger() },
                { "DamageSubType", (parser, x) => x.DamageSubType = parser.ParseEnum<DamageType>() },
                { "DrainLife", (parser, x) => x.DrainLife = parser.ParseBoolean() },
                { "DrainLifeMultiplier", (parser, x) => x.DrainLifeMultiplier = parser.ParseFloat() },
                { "CylinderAOE", (parser, x) => x.CylinderAOE = parser.ParseBoolean() },
                { "DamageArcInverted", (parser, x) => x.DamageArcInverted = parser.ParseBoolean() },
                { "ForceKillObjectFilter", (parser, x) => x.ForceKillObjectFilter = ObjectFilter.Parse(parser) },
                { "DamageMaxHeightAboveTerrain", (parser, x) => x.DamageMaxHeightAboveTerrain = parser.ParseInteger() },
                { "MinRadius", (parser, x) => x.MinRadius = parser.ParseInteger() },
                { "LostLeadershipUselessAgainst", (parser, x) => x.LostLeadershipUselessAgainst = parser.ParseEnum<ObjectKinds>() }
            });

        /// <summary>
        /// Damage for a normal hit.
        /// </summary>
        public float Damage { get; internal set; }

        /// <summary>
        /// Radius of damage.
        /// </summary>
        public float Radius { get; internal set; }

        /// <summary>
        /// Delay after hit till the damage is applied.
        /// </summary>
        public int DelayTime { get; private set; }
        public DamageType DamageType { get; internal set; }

        /// <summary>
        /// Used when <see cref="DamageType" is <see cref="DamageType.Status"/> />.
        /// </summary>
        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public DamageStatusType DamageStatusType { get; internal set; }

        public DamageFXType DamageFXType { get; private set; }
        public DeathType DeathType { get; internal set; }

        [AddedIn(SageGame.Bfme)]
        public float DamageSpeed { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int DamageArc { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public DamageScalar DamageScalar { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int DamageMaxHeight { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AcceptDamageAdd { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectKinds LostLeadershipUselessAgainst { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage FlankingBonus { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int DamageTaperOff { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public DamageType DamageSubType { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool DrainLife { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float DrainLifeMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool CylinderAOE { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool DamageArcInverted { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ObjectFilter ForceKillObjectFilter { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int DamageMaxHeightAboveTerrain { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int MinRadius { get; private set; }

        internal override void Execute(WeaponEffectExecutionContext context)
        {
            var weaponBonus = context.Weapon.ParentGameObject.FindBehavior<WeaponBonusUpgrade>();
            if (weaponBonus?.Triggered ?? false)
            {
                //TODO: increase damage with context.Weapon.Template.WeaponBonuses
            }

            context.Weapon.CurrentTarget.DoDamage(DamageType, (Fix64) Damage, DeathType, context.Time);
        }
    }
}

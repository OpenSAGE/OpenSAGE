using System;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;
using OpenSage.Mathematics.FixedMath;

namespace OpenSage.Logic.Object
{
    public class DamageNugget : WeaponEffectNugget
    {
        private readonly Weapon _weapon;
        private readonly DamageNuggetData _data;

        internal DamageNugget(Weapon weapon, DamageNuggetData data)
        {
            _weapon = weapon;
            _data = data;
        }

        internal override void Activate(TimeSpan currentTime)
        {
            _weapon.CurrentTarget.DoDamage(
                _data.DamageType,
                (Fix64) _data.Damage);
        }

        internal override void Update(TimeSpan currentTime)
        {
            // TODO: DelayTime
        }
    }

    [AddedIn(SageGame.Bfme2)]
    public class DamageNuggetData : WeaponEffectNuggetData
    {
        internal static DamageNuggetData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private protected static new readonly IniParseTable<DamageNuggetData> FieldParseTable = WeaponEffectNuggetData.FieldParseTable
            .Concat(new IniParseTable<DamageNuggetData>
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
            });

        public float Damage { get; internal set; }
        public float Radius { get; internal set; }
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

        internal override WeaponEffectNugget CreateNugget(Weapon weapon)
        {
            return new DamageNugget(weapon, this);
        }
    }
}

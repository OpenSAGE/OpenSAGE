using System.Collections.Generic;
using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class Armor
    {
        internal static Armor Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Armor> FieldParseTable = new IniParseTable<Armor>
        {
            { "Armor", (parser, x) => x.Values.Add(ArmorValue.Parse(parser)) }
        };

        public string Name { get; private set; }

        public List<ArmorValue> Values { get; } = new List<ArmorValue>();
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

        public DamageType DamageType { get; internal set; }
        public float Percent { get; internal set; }
    }

    public enum DamageType
    {
        [IniEnum("DEFAULT")]
        Default = 0,

        [IniEnum("EXPLOSION")]
        Explosion,

        [IniEnum("CRUSH")]
        Crush,

        [IniEnum("ARMOR_PIERCING")]
        ArmorPiercing,

        [IniEnum("SMALL_ARMS")]
        SmallArms,

        [IniEnum("GATTLING")]
        Gattling, // [sic]

        [IniEnum("RADIATION")]
        Radiation,

        [IniEnum("FLAME")]
        Flame,

        [IniEnum("LASER")]
        Laser,

        [IniEnum("SNIPER")]
        Sniper,

        [IniEnum("POISON")]
        Poison,

        [IniEnum("HEALING")]
        Healing,

        [IniEnum("UNRESISTABLE")]
        Unresistable,

        [IniEnum("WATER")]
        Water,

        [IniEnum("DEPLOY")]
        Deploy,

        [IniEnum("SURRENDER")]
        Surrender,

        [IniEnum("HACK")]
        Hack,

        [IniEnum("KILL_PILOT")]
        KillPilot,

        [IniEnum("PENALTY")]
        Penalty,

        [IniEnum("FALLING")]
        Falling,

        [IniEnum("MELEE")]
        Melee,

        [IniEnum("DISARM")]
        Disarm,

        [IniEnum("HAZARD_CLEANUP")]
        HazardCleanup,

        [IniEnum("INFANTRY_MISSILE")]
        InfantryMissile,

        [IniEnum("AURORA_BOMB")]
        AuroraBomb,

        [IniEnum("LAND_MINE")]
        LandMine,

        [IniEnum("JET_MISSILES")]
        JetMissiles,

        [IniEnum("STEALTHJET_MISSILES")]
        StealthjetMissiles,

        [IniEnum("MOLOTOV_COCKTAIL")]
        MolotovCocktail,

        [IniEnum("COMANCHE_VULCAN")]
        ComancheVulcan,

        [IniEnum("FLESHY_SNIPER")]
        FleshySniper,

        [IniEnum("PARTICLE_BEAM")]
        ParticleBeam
    }
}

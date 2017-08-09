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
            { "Armor", (parser, x) => x.Values.Add(parser.ParseArmorValue()) }
        };

        public string Name { get; private set; }

        public List<ArmorValue> Values { get; } = new List<ArmorValue>();
    }

    public sealed class ArmorValue
    {
        public DamageType DamageType { get; internal set; }
        public float Percent { get; internal set; }
    }

    public enum DamageType
    {
        Default = 0,

        Explosion,
        Crush,
        ArmorPiercing,
        SmallArms,
        Gattling, // [sic]
        Radiation,
        Flame,
        Laser,
        Sniper,
        Poison,
        Healing,
        Unresistable,
        Water,
        Deploy,
        Surrender,
        Hack,
        KillPilot,
        Penalty,
        Falling,
        Melee,
        Disarm,
        HazardCleanup,
        InfantryMissile,
        AuroraBomb,
        LandMine,
        JetMissiles,
        StealthjetMissiles,
        MolotovCocktail,
        ComancheVulcan,
        FleshySniper,
        ParticleBeam
    }
}

using System;
using System.Linq;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

/// <summary>
/// An armor encapsulates a set of modifiers for different types of damage taken,
/// in order to simulate different materials, and to help make game balance
/// easier to adjust.
/// </summary>
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

    public Percentage[] Values { get; } = Enumerable.Repeat(new Percentage(1), Enum.GetValues(typeof(DamageType)).Length).ToArray(); // default to 100%

    [AddedIn(SageGame.Bfme2)]
    public Percentage FlankedPenalty { get; private set; }

    /// <summary>
    /// Given a damage type and amount, adjusts the damage and returns the
    /// amount that should be dealt.
    /// </summary>
    internal float AdjustDamage(DamageType damageType, float damage)
    {
        if (damageType == DamageType.Unresistable)
        {
            return damage;
        }

        damage *= Values[(int)damageType];

        if (damage < 0.0f)
        {
            damage = 0.0f;
        }

        return damage;
    }
}

internal readonly struct Armor(ArmorTemplate template)
{
    public static readonly Armor NoArmor = new Armor(null);

    public float AdjustDamage(DamageType type, float damage)
    {
        return template?.AdjustDamage(type, damage) ?? damage;
    }
}

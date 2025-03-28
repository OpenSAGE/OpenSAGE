using System;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;
using OpenSage.FX;

namespace OpenSage.Logic.Object;

public sealed class DamageFX : BaseAsset
{
    internal static DamageFX Parse(IniParser parser)
    {
        return parser.ParseNamedBlock(
            (x, name) => x.SetNameAndInstanceId("DamageFX", name),
            FieldParseTable);
    }

    private static readonly IniParseTable<DamageFX> FieldParseTable = new IniParseTable<DamageFX>
    {
        { "ThrottleTime", (parser, x) => x.ParseGroupProperty(parser, parser.ParseTimeMillisecondsToLogicFrames, (g, v) => g.ThrottleTime = v) },
        { "AmountForMajorFX", (parser, x) => x.ParseGroupProperty(parser, parser.ParseFloat, (g, v) => g.AmountForMajorFX = v) },
        { "MajorFX", (parser, x) => x.ParseGroupProperty(parser, parser.ParseFXListReference, (g, v) => g.MajorFX = v) },
        { "MinorFX", (parser, x) => x.ParseGroupProperty(parser, parser.ParseFXListReference, (g, v) => g.MinorFX = v) },
        { "VeterancyMajorFX", (parser, x) => x.ParseVeterancyGroupProperty(parser, parser.ParseFXListReference, (g, v) => g.MajorFX = v) },
        { "VeterancyMinorFX", (parser, x) => x.ParseVeterancyGroupProperty(parser, parser.ParseFXListReference, (g, v) => g.MinorFX = v) },
    };

    private static readonly int NumDamageTypes = EnumUtility.GetEnumValueLength<DamageType>();
    private static readonly int NumVeterancyLevels = EnumUtility.GetEnumValueLength<VeterancyLevel>();

    public DamageFXGroup[,] Groups { get; }

    public DamageFX()
    {
        Groups = new DamageFXGroup[NumDamageTypes, NumVeterancyLevels];

        for (var i = 0; i < NumDamageTypes; i++)
        {
            for (var j = 0; j < NumVeterancyLevels; j++)
            {
                Groups[i, j] = new DamageFXGroup();
            }
        }
    }

    private DamageFXGroup GetGroup(DamageType damageType, GameObject source)
    {
        return GetGroup(damageType, source?.Rank ?? VeterancyLevel.Regular);
    }

    private DamageFXGroup GetGroup(DamageType damageType, VeterancyLevel level)
    {
        return Groups[(int)damageType, (int)level];
    }

    private void ParseVeterancyGroupProperty<T>(IniParser parser, Func<T> parseCallback, Action<DamageFXGroup, T> callback)
    {
        ParseGroupProperty(
            parser,
            [parser.ParseEnum<VeterancyLevel>()],
            parseCallback,
            callback);
    }

    private void ParseGroupProperty<T>(IniParser parser, Func<T> parseCallback, Action<DamageFXGroup, T> callback)
    {
        ParseGroupProperty(
            parser,
            Enum.GetValues<VeterancyLevel>(),
            parseCallback,
            callback);
    }

    private void ParseGroupProperty<T>(IniParser parser, VeterancyLevel[] veterancyLevels, Func<T> parseCallback, Action<DamageFXGroup, T> callback)
    {
        var damageTypeString = parser.ParseString();

        var value = parseCallback();

        var damageTypes = string.Equals(damageTypeString, "DEFAULT", StringComparison.InvariantCultureIgnoreCase)
            ? Enum.GetValues<DamageType>()
            : [IniParser.ParseEnum<DamageType>(damageTypeString)];

        foreach (var damageType in damageTypes)
        {
            foreach (var veterancyLevel in veterancyLevels)
            {
                var group = GetGroup(damageType, veterancyLevel);
                callback(group, value);
            }
        }
    }

    public LogicFrameSpan GetDamageFXThrottleTime(DamageType damageType, GameObject source)
    {
        return GetGroup(damageType, source).ThrottleTime;
    }

    public void DoDamageFX(DamageType damageType, float damageAmount, GameObject source, GameObject victim, GameEngine gameEngine)
    {
        var fxList = GetDamageFXList(damageType, damageAmount, source);

        // Since the victim is receiving the damage, it's the "primary" object.
        // The source is the "secondary" object - unused by most fx, but could
        // be useful in some cases.

        // TODO(Port): Pass source to FXList.

        fxList?.Execute(
            new FXListExecutionContext(
                victim.Rotation,
                victim.Translation,
                gameEngine));
    }

    private FXList GetDamageFXList(DamageType damageType, float damageAmount, GameObject source)
    {
        // If damage is zero, never do damage fx. This is by design, since
        // "zero" damage can happen with some special weapons, like the
        // battleship, which is a "faux" weapon that never does damage.
        // If you really need to change this for some reason, consider
        // carefully...
        if (damageAmount == 0.0f)
        {
            return null;
        }

        var group = GetGroup(damageType, source);

        return damageAmount >= group.AmountForMajorFX
            ? group.MajorFX?.Value
            : group.MinorFX?.Value;
    }
}

public sealed class DamageFXGroup
{
    public LogicFrameSpan ThrottleTime { get; internal set; }

    public float AmountForMajorFX { get; internal set; }

    public LazyAssetReference<FXList> MajorFX { get; internal set; }
    public LazyAssetReference<FXList> MinorFX { get; internal set; }
}

using System;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;

namespace OpenSage.Logic.Object
{
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
            { "ThrottleTime", (parser, x) => x.ParseGroupProperty(parser, parser.ParseInteger, (g, v) => g.ThrottleTime = v) },
            { "AmountForMajorFX", (parser, x) => x.ParseGroupProperty(parser, parser.ParseFloat, (g, v) => g.AmountForMajorFX = v) },
            { "MajorFX", (parser, x) => x.ParseGroupProperty(parser, parser.ParseFXListReference, (g, v) => g.MajorFX = v) },
            { "MinorFX", (parser, x) => x.ParseGroupProperty(parser, parser.ParseFXListReference, (g, v) => g.MinorFX = v) },
            { "VeterancyMajorFX", (parser, x) => x.ParseVeterancyGroupProperty(parser, parser.ParseFXListReference, (g, v) => g.VeterancyMajorFX = v) },
            { "VeterancyMinorFX", (parser, x) => x.ParseVeterancyGroupProperty(parser, parser.ParseFXListReference, (g, v) => g.VeterancyMinorFX = v) },
        };

        public DamageFXGroup[] Groups { get; }

        public DamageFX()
        {
            Groups = new DamageFXGroup[Enum.GetValues(typeof(DamageType)).Length];

            for (var i = 0; i < Groups.Length; i++)
            {
                Groups[i] = new DamageFXGroup();
            }
        }

        public DamageFXGroup GetGroup(DamageType damageType)
        {
            return Groups[(int)damageType];
        }

        private void ParseVeterancyGroupProperty<T>(IniParser parser, Func<T> parseCallback, Action<DamageFXGroup, T> callback)
        {
            var token = parser.GetNextToken();
            if (token.Text != "HEROIC")
            {
                throw new IniParseException($"Unexpected identifier: {token.Text}", token.Position);
            }

            ParseGroupProperty(parser, parseCallback, callback);
        }

        private void ParseGroupProperty<T>(IniParser parser, Func<T> parseCallback, Action<DamageFXGroup, T> callback)
        {
            var damageTypeString = parser.ParseString();

            var value = parseCallback();

            if (string.Equals(damageTypeString, "DEFAULT", StringComparison.InvariantCultureIgnoreCase))
            {
                for (var i = 0; i < Groups.Length; i++)
                {
                    callback(Groups[i], value);
                }
            }
            else
            {
                var damageType = IniParser.ParseEnum<DamageType>(damageTypeString);
                callback(Groups[(int) damageType], value);
            }
        }
    }

    public sealed class DamageFXGroup
    {
        public int ThrottleTime { get; internal set; }

        public float AmountForMajorFX { get; internal set; }

        public LazyAssetReference<FXList> MajorFX { get; internal set; }
        public LazyAssetReference<FXList> MinorFX { get; internal set; }

        public LazyAssetReference<FXList> VeterancyMajorFX { get; internal set; }
        public LazyAssetReference<FXList> VeterancyMinorFX { get; internal set; }
    }
}

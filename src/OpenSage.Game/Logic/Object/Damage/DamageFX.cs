using System;
using System.Collections.Generic;
using System.Linq;
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
            { "ThrottleTime", (parser, x) => x.ParseGroupProperty(parser, g => g.ThrottleTime = parser.ParseInteger()) },
            { "AmountForMajorFX", (parser, x) => x.ParseGroupProperty(parser, g => g.AmountForMajorFX = parser.ParseFloat()) },
            { "MajorFX", (parser, x) => x.ParseGroupProperty(parser, g => g.MajorFX = parser.ParseFXListReference()) },
            { "MinorFX", (parser, x) => x.ParseGroupProperty(parser, g => g.MinorFX = parser.ParseFXListReference()) },
            { "VeterancyMajorFX", (parser, x) => x.ParseVeterancyGroupProperty(parser, g => g.VeterancyMajorFX = parser.ParseFXListReference()) },
            { "VeterancyMinorFX", (parser, x) => x.ParseVeterancyGroupProperty(parser, g => g.VeterancyMinorFX = parser.ParseFXListReference()) },
        };

        public Dictionary<DamageType, DamageFXGroup> Groups { get; } = new Dictionary<DamageType, DamageFXGroup>();

        public DamageFXGroup GetGroup(DamageType damageType)
        {
            if (!Groups.TryGetValue(damageType, out var result))
            {
                result = Groups[DamageType.Default];
            }

            return result;
        }

        private void ParseVeterancyGroupProperty(IniParser parser, Action<DamageFXGroup> callback)
        {
            var token = parser.GetNextToken();
            if (token.Text != "HEROIC")
            {
                throw new IniParseException($"Unexpected identifier: {token.Text}", token.Position);
            }

            ParseGroupProperty(parser, callback);
        }

        private void ParseGroupProperty(IniParser parser, Action<DamageFXGroup> callback)
        {
            var damageType = parser.ParseEnum<DamageType>();

            if (!Groups.TryGetValue(damageType, out var group))
            {
                Groups.Add(damageType, group = new DamageFXGroup(damageType));
            }

            callback(group);
        }
    }

    public sealed class DamageFXGroup
    {
        public DamageType DamageType { get; }

        public int ThrottleTime { get; internal set; }

        public float AmountForMajorFX { get; internal set; }

        public LazyAssetReference<FXList> MajorFX { get; internal set; }
        public LazyAssetReference<FXList> MinorFX { get; internal set; }

        public LazyAssetReference<FXList> VeterancyMajorFX { get; internal set; }
        public LazyAssetReference<FXList> VeterancyMinorFX { get; internal set; }

        internal DamageFXGroup(DamageType damageType)
        {
            DamageType = damageType;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class DamageFX
    {
        internal static DamageFX Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<DamageFX> FieldParseTable = new IniParseTable<DamageFX>
        {
            { "ThrottleTime", (parser, x) => x.ParseGroupProperty(parser, g => g.ThrottleTime = parser.ParseInteger()) },
            { "AmountForMajorFX", (parser, x) => x.ParseGroupProperty(parser, g => g.AmountForMajorFX = parser.ParseFloat()) },
            { "MajorFX", (parser, x) => x.ParseGroupProperty(parser, g => g.MajorFX = parser.ParseAssetReference()) },
            { "MinorFX", (parser, x) => x.ParseGroupProperty(parser, g => g.MinorFX = parser.ParseAssetReference()) },
            { "VeterancyMajorFX", (parser, x) => x.ParseVeterancyGroupProperty(parser, g => g.VeterancyMajorFX = parser.ParseAssetReference()) },
            { "VeterancyMinorFX", (parser, x) => x.ParseVeterancyGroupProperty(parser, g => g.VeterancyMinorFX = parser.ParseAssetReference()) },
        };

        public string Name { get; private set; }

        public List<DamageFXGroup> Groups { get; } = new List<DamageFXGroup>();

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

            var group = Groups.FirstOrDefault(x => x.DamageType == damageType);
            if (group == null)
            {
                Groups.Add(group = new DamageFXGroup(damageType));
            }

            callback(group);
        }
    }

    public sealed class DamageFXGroup
    {
        public DamageType DamageType { get; }

        public int ThrottleTime { get; internal set; }

        public float AmountForMajorFX { get; internal set; }

        public string MajorFX { get; internal set; }
        public string MinorFX { get; internal set; }

        public string VeterancyMajorFX { get; internal set; }
        public string VeterancyMinorFX { get; internal set; }

        internal DamageFXGroup(DamageType damageType)
        {
            DamageType = damageType;
        }
    }
}

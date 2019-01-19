using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using OpenSage.Logic.Object;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class CrowdResponse
    {
        internal static CrowdResponse Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<CrowdResponse> FieldParseTable = new IniParseTable<CrowdResponse>
        {
            { "Threshold", (parser, x) => x.Thresholds.Add(Threshold.Parse(parser)) },
            { "Weight", (parser, x) => x.Weight = parser.ParseInteger() },
        };

        public string Name { get; private set; }
        public List<Threshold> Thresholds { get; } = new List<Threshold>();
        public int Weight { get; private set; }
    }

    public sealed class Threshold
    {
        internal static Threshold Parse(IniParser parser)
        {
            return parser.ParseTopLevelIndexedBlock(
                (x, index) => x.Index = index,
                FieldParseTable);
        }

        private static readonly IniParseTable<Threshold> FieldParseTable = new IniParseTable<Threshold>
        {
            { "VoiceAttack", (parser, x) => x.VoiceAttack = parser.ParseAssetReference() },
            { "VoiceAttackCharge", (parser, x) => x.VoiceAttackCharge = parser.ParseAssetReference() },
            { "VoiceMove", (parser, x) => x.VoiceMove = parser.ParseAssetReference() },
            { "VoiceSelect", (parser, x) => x.VoiceSelect = parser.ParseAssetReference() },
            { "UnitSpecificSounds", (parser, x) => x.UnitSpecificSoundSet.Add(UnitSpecificSounds.Parse(parser)) }
        };

        public int Index { get; private set; }

        public string VoiceAttack { get; private set; }
        public string VoiceAttackCharge { get; private set; }
        public string VoiceMove { get; private set; }
        public string VoiceSelect { get; private set; }

        public List<UnitSpecificSounds> UnitSpecificSoundSet { get; } = new List<UnitSpecificSounds>();
    }
}

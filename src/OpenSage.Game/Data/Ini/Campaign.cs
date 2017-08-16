using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class Campaign
    {
        internal static Campaign Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Campaign> FieldParseTable = new IniParseTable<Campaign>
        {
            { "CampaignNameLabel", (parser, x) => x.CampaignNameLabel = parser.ParseLocalizedStringKey() },
            { "FirstMission", (parser, x) => x.FirstMission = parser.ParseAssetReference() },
            { "FinalVictoryMovie", (parser, x) => x.FinalVictoryMovie = parser.ParseAssetReference() },
            { "Mission", (parser, x) => x.Missions.Add(CampaignMission.Parse(parser)) }
        };

        public string Name { get; private set; }

        public string CampaignNameLabel { get; private set; }
        public string FirstMission { get; private set; }
        public string FinalVictoryMovie { get; private set; }

        public List<CampaignMission> Missions { get; } = new List<CampaignMission>();
    }

    public sealed class CampaignMission
    {
        internal static CampaignMission Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<CampaignMission> FieldParseTable = new IniParseTable<CampaignMission>
        {
            { "Map", (parser, x) => x.Map = parser.ParseFileName() },
            { "NextMission", (parser, x) => x.NextMission = parser.ParseAssetReference() },
            { "IntroMovie", (parser, x) => x.IntroMovie = parser.ParseAssetReference() },
            { "ObjectiveLine0", (parser, x) => x.ObjectiveLine0 = parser.ParseLocalizedStringKey() },
            { "ObjectiveLine1", (parser, x) => x.ObjectiveLine1 = parser.ParseLocalizedStringKey() },
            { "ObjectiveLine2", (parser, x) => x.ObjectiveLine2 = parser.ParseLocalizedStringKey() },
            { "ObjectiveLine3", (parser, x) => x.ObjectiveLine3 = parser.ParseLocalizedStringKey() },
            { "ObjectiveLine4", (parser, x) => x.ObjectiveLine4 = parser.ParseLocalizedStringKey() },
            { "BriefingVoice", (parser, x) => x.BriefingVoice = parser.ParseAssetReference() },
            { "UnitNames0", (parser, x) => x.UnitNames0 = parser.ParseLocalizedStringKey() },
            { "UnitNames1", (parser, x) => x.UnitNames1 = parser.ParseLocalizedStringKey() },
            { "UnitNames2", (parser, x) => x.UnitNames2 = parser.ParseLocalizedStringKey() },
            { "LocationNameLabel", (parser, x) => x.LocationNameLabel = parser.ParseLocalizedStringKey() },
            { "VoiceLength", (parser, x) => x.VoiceLength = parser.ParseInteger() }
        };

        public string Name { get; private set; }

        public string Map { get; private set; }
        public string NextMission { get; private set; }
        public string IntroMovie { get; private set; }
        public string ObjectiveLine0 { get; private set; }
        public string ObjectiveLine1 { get; private set; }
        public string ObjectiveLine2 { get; private set; }
        public string ObjectiveLine3 { get; private set; }
        public string ObjectiveLine4 { get; private set; }
        public string BriefingVoice { get; private set; }
        public string UnitNames0 { get; private set; }
        public string UnitNames1 { get; private set; }
        public string UnitNames2 { get; private set; }
        public string LocationNameLabel { get; private set; }
        public int VoiceLength { get; private set; }
    }
}

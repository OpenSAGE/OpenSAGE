using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Ini;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;

namespace OpenSage.Logic
{
    public sealed class CampaignTemplate
    {
        internal static CampaignTemplate Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<CampaignTemplate> FieldParseTable = new IniParseTable<CampaignTemplate>
        {
            { "CampaignNameLabel", (parser, x) => x.DisplayName = parser.ParseLocalizedStringKey() },
            { "FirstMission", (parser, x) => x.FirstMission = parser.ParseAssetReference() },
            { "FinalVictoryMovie", (parser, x) => x.FinalMovie = parser.ParseAssetReference() },
            { "IsChallengeCampaign", (parser, x) => x.IsChallengeCampaign = parser.ParseBoolean() },
            { "PlayerFaction", (parser, x) => x.PlayerFaction = parser.ParseAssetReference() },
            { "Mission", (parser, x) => x.Missions.Add(MissionTemplate.Parse(parser)) }
        };

        internal static CampaignTemplate ParseAsset(BinaryReader reader, Asset asset, AssetImportCollection imports)
        {
            throw new System.NotImplementedException();

            return new CampaignTemplate
            {
                Name = asset.Name,
                DisplayName = reader.ReadUInt32PrefixedAsciiStringAtOffset(),
                FinalMovie = reader.ReadUInt32PrefixedAsciiStringAtOffset(),
                AlternateFinalMovie = reader.ReadUInt32PrefixedAsciiStringAtOffset(),
                ConsoleAutosaveFilename = reader.ReadUInt32PrefixedAsciiStringAtOffset(),
                //TheatersOfWar = reader.ReadArrayAtOffset(() => imports.GetImportedData<TheaterOfWarTemplate>(reader)),
            };
        }

        public string Name { get; private set; }

        public string DisplayName { get; private set; }
        public string FirstMission { get; private set; }
        public string FinalMovie { get; private set; }

        [AddedIn(SageGame.Cnc3)]
        public string AlternateFinalMovie { get; private set; }

        [AddedIn(SageGame.Cnc3)]
        public string ConsoleAutosaveFilename { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool IsChallengeCampaign { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string PlayerFaction { get; private set; }

        public List<MissionTemplate> Missions { get; } = new List<MissionTemplate>();
    }

    public sealed class MissionTemplate
    {
        internal static MissionTemplate Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<MissionTemplate> FieldParseTable = new IniParseTable<MissionTemplate>
        {
            { "Map", (parser, x) => x.Map = parser.ParseFileName() },
            { "GeneralName", (parser, x) => x.GeneralName = parser.ParseLocalizedStringKey() },
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

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string GeneralName { get; private set; }

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

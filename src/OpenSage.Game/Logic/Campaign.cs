﻿using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic
{
    public sealed class CampaignTemplate : BaseAsset
    {
        internal static CampaignTemplate Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("CampaignTemplate", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<CampaignTemplate> FieldParseTable = new IniParseTable<CampaignTemplate>
        {
            { "CampaignNameLabel", (parser, x) => x.DisplayName = parser.ParseLocalizedStringKey() },
            { "FirstMission", (parser, x) => x.FirstMission = parser.ParseAssetReference() },
            { "FinalVictoryMovie", (parser, x) => x.FinalMovie = parser.ParseAssetReference() },
            { "IsChallengeCampaign", (parser, x) => x.IsChallengeCampaign = parser.ParseBoolean() },
            { "PlayerFaction", (parser, x) => x.PlayerFaction = parser.ParseAssetReference() },
            { "Mission", (parser, x) => x.AddMission(MissionTemplate.Parse(parser)) }
        };

        private void AddMission(MissionTemplate mission)
        {
            Missions.Add(mission.Name, mission);
        }

        public string DisplayName { get; private set; }
        public string FirstMission { get; private set; }
        public string FinalMovie { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool IsChallengeCampaign { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string PlayerFaction { get; private set; }

        public Dictionary<string, MissionTemplate> Missions { get; } = new Dictionary<string, MissionTemplate>(StringComparer.InvariantCultureIgnoreCase);
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

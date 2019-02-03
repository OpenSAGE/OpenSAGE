using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class LinearCampaign
    {
        internal static LinearCampaign Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<LinearCampaign> FieldParseTable = new IniParseTable<LinearCampaign>
        {
            { "CampaignDisplayNameLabel", (parser, x) => x.CampaignDisplayNameLabel = parser.ParseLocalizedStringKey() },
            { "CarryoverUnit", (parser, x) => x.CarryoverUnits.Add(parser.ParseAssetReference()) },
            { "OverallCampaignIntroMovie", (parser, x) => x.OverallCampaignIntroMovie = parser.ParseAssetReference() },
            { "Mission", (parser, x) => x.Missions.Add(Mission.Parse(parser)) }
        };

        public string CampaignDisplayNameLabel { get; private set; }
        public List<string> CarryoverUnits { get; } = new List<string>();
        public string OverallCampaignIntroMovie { get; private set; }
        public List<Mission> Missions { get; } = new List<Mission>();
    }

    public class Mission
    {
        internal static Mission Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Mission> FieldParseTable = new IniParseTable<Mission>
        {
            { "Map", (parser, x) => x.Map = parser.ParseQuotedString() },
            { "IntroMovie", (parser, x) => x.IntroMovie = parser.ParseAssetReference() },
            { "LoadScreenImage", (parser, x) => x.LoadScreenImage = parser.ParseAssetReference() },
            { "LoadScreenMusicTrack", (parser, x) => x.LoadScreenMusicTrack = parser.ParseAssetReference() },
            { "MillisecondsAfterStartToStartFadeUp", (parser, x) => x.MillisecondsAfterStartToStartFadeUp = parser.ParseInteger() },
            { "DelayCarryoverSpawningOf", (parser, x) => x.DelayCarryoverSpawningOfs.Add(parser.ParseAssetReference()) }
        };

        public string Name { get; private set; }
        public string Map { get; private set; }
        public string IntroMovie { get; private set; }
        public string LoadScreenImage { get; private set; }
        public string LoadScreenMusicTrack { get; private set; }
        public int MillisecondsAfterStartToStartFadeUp { get; private set; }

        //; List CarryoverUnits which we don't want to appear at the beginning of the map automatically
        //some dont spawn at all, but are saved here for later missions
        public List<string> DelayCarryoverSpawningOfs { get; } = new List<string>();
    }
}

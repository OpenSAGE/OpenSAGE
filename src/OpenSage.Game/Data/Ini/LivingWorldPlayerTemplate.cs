using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class LivingWorldPlayerTemplate
    {
        internal static LivingWorldPlayerTemplate Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldPlayerTemplate> FieldParseTable = new IniParseTable<LivingWorldPlayerTemplate>
        {
            { "Name", (parser, x) => x.Name = parser.ParseAssetReference() },
            { "Faction", (parser, x) => x.Faction = parser.ParseString() },
            { "StartingWorldCP", (parser, x) => x.StartingWorldCP = parser.ParseInteger() },
            { "MaxWorldCP", (parser, x) => x.MaxWorldCP = parser.ParseInteger() },
            { "StartingHeroCP", (parser, x) => x.StartingHeroCP = parser.ParseInteger() },
            { "MaxHeroCP", (parser, x) => x.MaxWorldCP = parser.ParseInteger() },
            { "FactionIcon", (parser, x) => x.FactionIcon = parser.ParseQuotedString() },
            { "DefaultArmyIconName", (parser, x) => x.DefaultArmyIconName = parser.ParseQuotedString() },
            { "BuildPlotIconName", (parser, x) => x.BuildPlotIconName = parser.ParseQuotedString() },
            { "BuildPlotSelectionPortraitName", (parser, x) => x.BuildPlotSelectionPortraitName = parser.ParseQuotedString() },
            { "GarrisonSelectionPortraitName", (parser, x) => x.GarrisonSelectionPortraitName = parser.ParseQuotedString() },
            { "GarrisonDisplayNameTag", (parser, x) => x.GarrisonDisplayNameTag = parser.ParseLocalizedStringKey() },
            { "Music", (parser, x) => x.Music = parser.ParseAssetReference() },
            { "AutoResolveLoop", (parser, x) => x.AutoResolveLoop = parser.ParseAssetReference() },
            { "ScenarioStartResources", (parser, x) => x.ScenarioStartResources = parser.ParseInteger() },
            { "ScenarioMaxResources", (parser, x) => x.ScenarioMaxResources = parser.ParseInteger() },
            { "FactionDozerTemplateName", (parser, x) => x.FactionDozerTemplateName = parser.ParseQuotedString() },
            { "FactionInnUnitTemplateName", (parser, x) => x.FactionInnUnitTemplateName = parser.ParseQuotedString() }
        };

        public string Name { get; private set; }

        public string Faction { get; private set; }
        public int StartingWorldCP { get; private set; }
        public int MaxWorldCP { get; private set; }
        public int StartingHeroCP { get; private set; }
        public int MaxHeroCP { get; private set; }
        public string FactionIcon { get; private set; }
        public string DefaultArmyIconName { get; private set; }
        public string BuildPlotIconName { get; private set; }
        public string BuildPlotSelectionPortraitName { get; private set; }
        public string GarrisonSelectionPortraitName { get; private set; }
        public string GarrisonDisplayNameTag { get; private set; }
        public string Music { get; private set; }
        public string AutoResolveLoop { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int ScenarioStartResources { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int ScenarioMaxResources { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string FactionDozerTemplateName { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string FactionInnUnitTemplateName { get; private set; }
    }
}

using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class WallHubBehaviorModuleData : BehaviorModuleData
    {
        internal static WallHubBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<WallHubBehaviorModuleData> FieldParseTable = new IniParseTable<WallHubBehaviorModuleData>
        {
            { "Options", (parser, x) => x.Options = parser.ParseEnum<CommandButtonOption>() },
            { "MaxBuildoutDistance", (parser, x) => x.MaxBuildoutDistance = parser.ParseInteger() },
            { "StaggeredBuildFactor", (parser, x) => x.StaggeredBuildFactor = parser.ParseFloat() },
            { "SegmentTemplateName", (parser, x) => x.SegmentTemplateNames.Add(parser.ParseAssetReference()) },
            { "HubCapTemplateName", (parser, x) => x.HubCapTemplateName = parser.ParseAssetReference() },
            { "DefaultSegmentTemplateName", (parser, x) => x.DefaultSegmentTemplateName = parser.ParseAssetReference() },
            { "CliffCapTemplateName", (parser, x) => x.CliffCapTemplateName = parser.ParseAssetReference() },
            { "ShoreCapTemplateName", (parser, x) => x.ShoreCapTemplateName = parser.ParseAssetReference() },
            { "BorderCapTemplateName", (parser, x) => x.BorderCapTemplateName = parser.ParseAssetReference() },
            { "ElevatedSegmentTemplateName", (parser, x) => x.ElevatedSegmentTemplateName = parser.ParseAssetReference() },
            { "BuilderRadius", (parser, x) => x.BuilderRadius = parser.ParseFloat() },
        };

        public CommandButtonOption Options { get; private set; }
        public int MaxBuildoutDistance { get; private set; }
        public float StaggeredBuildFactor { get; private set; }
        public List<string> SegmentTemplateNames { get; } = new List<string>();
        public string HubCapTemplateName { get; private set; }
        public string DefaultSegmentTemplateName { get; private set; }
        public string CliffCapTemplateName { get; private set; }
        public string ShoreCapTemplateName { get; private set; }
        public string BorderCapTemplateName { get; private set; }
        public string ElevatedSegmentTemplateName { get; private set; }
        public float BuilderRadius { get; private set; }
    }
}

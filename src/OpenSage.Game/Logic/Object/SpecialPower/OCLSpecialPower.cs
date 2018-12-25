using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class OCLSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new OCLSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<OCLSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<OCLSpecialPowerModuleData>
            {
                { "OCL", (parser, x) => x.OCL = parser.ParseAssetReference() },
                { "UpgradeOCL", (parser, x) => x.UpgradeOCLs.Add(OCLUpgradePair.Parse(parser)) },
                { "CreateLocation", (parser, x) => x.CreateLocation = parser.ParseEnum<OCLCreateLocation>() },
                { "ScriptedSpecialPowerOnly", (parser, x) => x.ScriptedSpecialPowerOnly = parser.ParseBoolean() },
                { "OCLAdjustPositionToPassable", (parser, x) => x.OCLAdjustPositionToPassable = parser.ParseBoolean() },
                { "ReferenceObject", (parser, x) => x.ReferenceObject = parser.ParseAssetReference() },
                { "UpgradeName", (parser, x) => x.UpgradeName = parser.ParseIdentifier() },
                { "NearestSecondaryObjectFilter", (parser, x) => x.NearestSecondaryObjectFilter = ObjectFilter.Parse(parser) },
                { "ReEnableAntiCategory", (parser, x) => x.ReEnableAntiCategory = parser.ParseBoolean() },
                { "WeatherDuration", (parser, x) => x.WeatherDuration = parser.ParseInteger() },
                { "ChangeWeather", (parser, x) => x.ChangeWeather = parser.ParseEnum<WeatherType>() }
            });

        public string OCL { get; private set; }
        public List<OCLUpgradePair> UpgradeOCLs { get; } = new List<OCLUpgradePair>();
        public OCLCreateLocation CreateLocation { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool ScriptedSpecialPowerOnly { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool OCLAdjustPositionToPassable { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string ReferenceObject { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string UpgradeName { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ObjectFilter NearestSecondaryObjectFilter { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ReEnableAntiCategory { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int WeatherDuration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public WeatherType ChangeWeather { get; private set; }
    }

    public sealed class OCLUpgradePair
    {
        internal static OCLUpgradePair Parse(IniParser parser)
        {
            return new OCLUpgradePair
            {
                Science = parser.ParseAssetReference(),
                OCL = parser.ParseAssetReference()
            };
        }

        public string Science { get; private set; }
        public string OCL { get; private set; }
    }

    public enum OCLCreateLocation
    {
        [IniEnum("USE_OWNER_OBJECT")]
        UseOwnerObject,

        [IniEnum("CREATE_AT_EDGE_NEAR_SOURCE")]
        CreateAtEdgeNearSource,

        [IniEnum("CREATE_AT_EDGE_FARTHEST_FROM_TARGET")]
        CreateAtEdgeFarthestFromTarget,

        [IniEnum("CREATE_ABOVE_LOCATION")]
        CreateAboveLocation,

        [IniEnum("CREATE_AT_LOCATION")]
        CreateAtLocation,

        [IniEnum("CREATE_AT_EDGE_NEAR_TARGET_AND_MOVE_TO_LOCATION"), AddedIn(SageGame.Bfme)]
        CreateAtEdgeNearTargetAndMoveToLocation,

        [IniEnum("USE_SECONDARY_OBJECT_LOCATION"), AddedIn(SageGame.Bfme2)]
        UseSecondaryObjectLocation,
    }
}

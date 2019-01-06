using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using OpenSage.Data.Map;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class W3dFloorDrawModuleData : DrawModuleData
    {
        internal static W3dFloorDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<W3dFloorDrawModuleData> FieldParseTable = new IniParseTable<W3dFloorDrawModuleData>
        {
            { "ModelName", (parser, x) => x.ModelName = parser.ParseAssetReference() },
            { "StaticModelLODMode", (parser, x) => x.StaticModelLODMode = parser.ParseBoolean() },
            { "StartHidden", (parser, x) => x.StartHidden = parser.ParseBoolean() },
            { "ForceToBack", (parser, x) => x.ForceToBack = parser.ParseBoolean() },
            { "FloorFadeRateOnObjectDeath", (parser, x) => x.FloorFadeRateOnObjectDeath = parser.ParseFloat() },
            { "HideIfModelConditions", (parser, x) => x.HideIfModelConditions.Add(parser.ParseEnum<ModelConditionFlag>())},
            { "WeatherTexture", (parser, x) => x.WeatherTexture = WeatherTexture.Parse(parser) }
        };

        public string ModelName { get; private set; }
        public bool StaticModelLODMode { get; private set; }
        public bool StartHidden { get; private set; }
        public bool ForceToBack { get; private set; }
        public float FloorFadeRateOnObjectDeath { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<ModelConditionFlag> HideIfModelConditions { get; } = new List<ModelConditionFlag>();

        [AddedIn(SageGame.Bfme2)]
        public WeatherTexture WeatherTexture { get; private set; }
    }

    public struct WeatherTexture
    {
        internal static WeatherTexture Parse(IniParser parser)
        {
            return new WeatherTexture()
            {
                WatherType = parser.ParseEnum<MapWeatherType>(),
                Texture = parser.ParseAssetReference()
            };
        }

        public string Texture { get; private set; }
        public MapWeatherType WatherType { get; private set; }
    }
}

﻿using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Terrain
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class Fire : BaseSingletonAsset
    {
        internal static void Parse(IniParser parser, Fire value) => parser.ParseBlockContent(value, FieldParseTable);

        private static readonly IniParseTable<Fire> FieldParseTable = new IniParseTable<Fire>
        {
            { "TerrainFireSystem", (parser, x) => x.TerrainFireSystem = parser.ParseAssetReference() },
            { "TerrainSmokeSystem", (parser, x) => x.TerrainSmokeSystem = parser.ParseAssetReference() },
            { "BurntTerrainColor", (parser, x) => x.BurntTerrainColor = parser.ParseColorRgb() },
            { "FuelIndicatorColor", (parser, x) => x.FuelIndicatorColor = parser.ParseColorRgb() },
            { "EnableScorches", (parser, x) => x.EnableScorches = parser.ParseBoolean() },
            { "ScorchFrequency", (parser, x) => x.ScorchFrequency = parser.ParseRandomVariable() },
            { "ScorchSize", (parser, x) => x.ScorchSize = parser.ParseRandomVariable() },
            { "ScorchIntensity", (parser, x) => x.ScorchIntensity = parser.ParseFloat() }
        };

        public string TerrainFireSystem { get; private set; }
        public string TerrainSmokeSystem { get; private set; }
        public ColorRgb BurntTerrainColor { get; private set; }
        public ColorRgb FuelIndicatorColor { get; private set; }
        public bool EnableScorches { get; private set; }
        public RandomVariable ScorchFrequency { get; private set; }
        public RandomVariable ScorchSize { get; private set; }
        public float ScorchIntensity { get; private set; }
    }
}

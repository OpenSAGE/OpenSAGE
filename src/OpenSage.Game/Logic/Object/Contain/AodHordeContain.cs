using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class AodHordeContainModuleData : HordeContainModuleData
    {
        internal static new AodHordeContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly new IniParseTable<AodHordeContainModuleData> FieldParseTable = HordeContainModuleData.FieldParseTable
            .Concat(new IniParseTable<AodHordeContainModuleData>
            {
                { "FrequencyScale", (parser, x) => x.FrequencyScale = parser.ParseFloat() },
                { "FrequencyRandomness", (parser, x) => x.FrequencyRandomness = parser.ParseFloat() },
                { "AmplitudeScale", (parser, x) => x.AmplitudeScale = parser.ParseInteger() },
                { "AmplitudeRandomness", (parser, x) => x.AmplitudeRandomness = parser.ParseFloat() },
                { "StillAmplitude", (parser, x) => x.StillAmplitude = parser.ParseFloat() },

                { "FrequencyScaleZ", (parser, x) => x.FrequencyScaleZ = parser.ParseFloat() },
                { "FrequencyRandomnessZ", (parser, x) => x.FrequencyRandomnessZ = parser.ParseFloat() },
                { "AmplitudeScaleZ", (parser, x) => x.AmplitudeScaleZ = parser.ParseInteger() },
                { "AmplitudeRandomnessZ", (parser, x) => x.AmplitudeRandomnessZ = parser.ParseFloat() },
                { "StillAmplitudeZ", (parser, x) => x.StillAmplitudeZ = parser.ParseFloat() },

                { "LargeUnitHeightFactor", (parser, x) => x.LargeUnitHeightFactor = parser.ParseFloat() },
                { "LargeUnitMinHeight", (parser, x) => x.LargeUnitMinHeight = parser.ParseFloat() },
                { "LargeUnitMaxHeight", (parser, x) => x.LargeUnitMaxHeight = parser.ParseFloat() },
                { "LargeUnitTimeout", (parser, x) => x.LargeUnitTimeout = parser.ParseInteger() },
                { "LargeUnitTailOff", (parser, x) => x.LargeUnitTailOff = parser.ParseFloat() },

                { "ScatterSpeedFactor", (parser, x) => x.ScatterSpeedFactor = parser.ParseFloat() },
                { "ScatterRandomness", (parser, x) => x.ScatterRandomness = parser.ParseFloat() },
            });

        public float FrequencyScale { get; private set; }
        public float FrequencyRandomness { get; private set; }
        public int AmplitudeScale { get; private set; }
        public float AmplitudeRandomness { get; private set; }
        public float StillAmplitude { get; private set; }

        public float FrequencyScaleZ { get; private set; }
        public float FrequencyRandomnessZ { get; private set; }
        public int AmplitudeScaleZ { get; private set; }
        public float AmplitudeRandomnessZ { get; private set; }
        public float StillAmplitudeZ { get; private set; }

        public float LargeUnitHeightFactor { get; private set; }
        public float LargeUnitMinHeight { get; private set; }
        public float LargeUnitMaxHeight { get; private set; }
        public int LargeUnitTimeout { get; private set; }
        public float LargeUnitTailOff { get; private set; }

        public float ScatterSpeedFactor { get; private set; }
        public float ScatterRandomness { get; private set; }
    }
}

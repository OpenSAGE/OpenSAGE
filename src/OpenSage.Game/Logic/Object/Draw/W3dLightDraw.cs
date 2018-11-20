using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class W3dLightDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dLightDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<W3dLightDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dLightDrawModuleData>
            {
                { "Ambient", (parser, x) => x.Ambient = parser.ParseColorRgba() },
                { "Diffuse", (parser, x) => x.Diffuse = parser.ParseColorRgba() },
                { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
                { "Intensity", (parser, x) => x.Intensity = parser.ParseFloat() },
                { "FlickerAmplitude", (parser, x) => x.FlickerAmplitude = parser.ParseFloat() },
                { "FlickerFrequency", (parser, x) => x.FlickerFrequency = parser.ParseFloat() }
            });

        public ColorRgba Ambient { get; private set; }
		public ColorRgba Diffuse { get; private set; }
        public int Radius { get; private set; }
        public float Intensity { get; private set; }
        public float FlickerAmplitude { get; private set; }
        public float FlickerFrequency { get; private set; }
    }
}

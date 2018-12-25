using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public class W3dSailModelDrawModuleData : W3dModelDrawModuleData
    {
        internal static  W3dSailModelDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable< W3dSailModelDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable< W3dSailModelDrawModuleData>
            {
                { "MaxRotationDegrees", (parser, x) => x.MaxRotationDegrees = parser.ParseInteger() },
                { "BlowingThresholdDegrees", (parser, x) => x.BlowingThresholdDegrees = parser.ParseInteger() },
                { "AboutDamping", (parser, x) => x.AboutDamping = parser.ParseFloat() },
            });

        public int MaxRotationDegrees { get; private set; }
        public int BlowingThresholdDegrees { get; private set; }
        public float AboutDamping { get; private set; }
    }
}

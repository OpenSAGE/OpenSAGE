using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows for the use of the FRONTCRUSHED and BACKCRUSHED ConditionStates with this object.
    /// Also allows the object to cast a ambient light around itself.
    /// </summary>
    public sealed class W3dPoliceCarDraw : W3dModelDraw
    {
        internal static W3dPoliceCarDraw Parse(IniParser parser) => parser.ParseBlock(PoliceCarFieldParseTable);

        private static readonly IniParseTable<W3dPoliceCarDraw> PoliceCarFieldParseTable = new IniParseTable<W3dPoliceCarDraw>
        {
            { "PowerslideRotationAddition", (parser, x) => x.PowerslideRotationAddition = parser.ParseFloat() },
            { "TireRotationMultiplier", (parser, x) => x.TireRotationMultiplier = parser.ParseFloat() },
            { "RightFrontTireBone", (parser, x) => x.RightFrontTireBone = parser.ParseBoneName() },
            { "LeftFrontTireBone", (parser, x) => x.LeftFrontTireBone = parser.ParseBoneName() },
            { "RightRearTireBone", (parser, x) => x.RightRearTireBone = parser.ParseBoneName() },
            { "LeftRearTireBone", (parser, x) => x.LeftRearTireBone = parser.ParseBoneName() },
        }.Concat<W3dPoliceCarDraw, W3dModelDraw>(ModelFieldParseTable);

        public float PowerslideRotationAddition { get; private set; }
        public float TireRotationMultiplier { get; private set; }
        public string RightFrontTireBone { get; private set; }
        public string LeftFrontTireBone { get; private set; }
        public string RightRearTireBone { get; private set; }
        public string LeftRearTireBone { get; private set; }
    }
}

using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows for the use of the FRONTCRUSHED and BACKCRUSHED ConditionStates with this object.
    /// Also allows the object to cast a ambient light around itself.
    /// </summary>
    public sealed class W3dPoliceCarDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dPoliceCarDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dPoliceCarDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dPoliceCarDrawModuleData>
            {
                { "PowerslideRotationAddition", (parser, x) => x.PowerslideRotationAddition = parser.ParseFloat() },
                { "TireRotationMultiplier", (parser, x) => x.TireRotationMultiplier = parser.ParseFloat() },
                { "RightFrontTireBone", (parser, x) => x.RightFrontTireBone = parser.ParseBoneName() },
                { "LeftFrontTireBone", (parser, x) => x.LeftFrontTireBone = parser.ParseBoneName() },
                { "RightRearTireBone", (parser, x) => x.RightRearTireBone = parser.ParseBoneName() },
                { "LeftRearTireBone", (parser, x) => x.LeftRearTireBone = parser.ParseBoneName() },
            });

        public float PowerslideRotationAddition { get; private set; }
        public float TireRotationMultiplier { get; private set; }
        public string RightFrontTireBone { get; private set; }
        public string LeftFrontTireBone { get; private set; }
        public string RightRearTireBone { get; private set; }
        public string LeftRearTireBone { get; private set; }
    }
}

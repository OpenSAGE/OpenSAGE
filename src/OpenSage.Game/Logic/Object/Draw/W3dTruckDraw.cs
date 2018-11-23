using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to call for the TreadDebrisRight and TreadDebrisLeft (unless overriden) particle 
    /// system definitions and allows use of TruckPowerslideSound and TruckLandingSound within the 
    /// UnitSpecificSounds section of the object.
    /// 
    /// This module also includes automatic logic for showing and hiding of HEADLIGHT bones in and 
    /// out of the NIGHT ModelConditionState.
    /// </summary>
    public class W3dTruckDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dTruckDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<W3dTruckDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dTruckDrawModuleData>
            {
                { "CabRotationMultiplier", (parser, x) => x.CabRotationMultiplier = parser.ParseFloat() },
                { "TrailerRotationMultiplier", (parser, x) => x.TrailerRotationMultiplier = parser.ParseFloat() },
                { "CabBone", (parser, x) => x.CabBone = parser.ParseBoneName() },
                { "TrailerBone", (parser, x) => x.TrailerBone = parser.ParseBoneName() },
                { "RotationDamping", (parser, x) => x.RotationDamping = parser.ParseFloat() },

                { "PowerslideRotationAddition", (parser, x) => x.PowerslideRotationAddition = parser.ParseFloat() },
                { "TireRotationMultiplier", (parser, x) => x.TireRotationMultiplier = parser.ParseFloat() },
                { "RightFrontTireBone", (parser, x) => x.RightFrontTireBone = parser.ParseBoneName() },
                { "LeftFrontTireBone", (parser, x) => x.LeftFrontTireBone = parser.ParseBoneName() },
                { "MidRightFrontTireBone", (parser, x) => x.MidRightFrontTireBone = parser.ParseBoneName() },
                { "MidLeftFrontTireBone", (parser, x) => x.MidLeftFrontTireBone = parser.ParseBoneName() },
                { "MidRightMidTireBone", (parser, x) => x.MidRightMidTireBone = parser.ParseBoneName() },
                { "MidLeftMidTireBone", (parser, x) => x.MidLeftMidTireBone = parser.ParseBoneName() },
                { "MidRightRearTireBone", (parser, x) => x.MidRightRearTireBone = parser.ParseBoneName() },
                { "MidLeftRearTireBone", (parser, x) => x.MidLeftRearTireBone = parser.ParseBoneName() },
                { "RightRearTireBone", (parser, x) => x.RightRearTireBone = parser.ParseBoneName() },
                { "LeftRearTireBone", (parser, x) => x.LeftRearTireBone = parser.ParseBoneName() },

                { "Dust", (parser, x) => x.Dust = parser.ParseAssetReference() },
                { "DirtSpray", (parser, x) => x.DirtSpray = parser.ParseAssetReference() },
                { "PowerslideSpray", (parser, x) => x.PowerslideSpray = parser.ParseAssetReference() },
            });

        // Settings for the attached "cab" model on the vehicle
        public float CabRotationMultiplier { get; private set; }
        public float TrailerRotationMultiplier { get; private set; }
        public string CabBone { get; private set; }
        public string TrailerBone { get; private set; }
        public float RotationDamping { get; private set; }

        // Wheel configuration
        public float PowerslideRotationAddition { get; private set; }
        public float TireRotationMultiplier { get; private set; }
        public string RightFrontTireBone { get; private set; }
        public string LeftFrontTireBone { get; private set; }
        public string MidRightFrontTireBone { get; private set; }
        public string MidLeftFrontTireBone { get; private set; }
        public string MidRightMidTireBone { get; private set; }
        public string MidLeftMidTireBone { get; private set; }
        public string MidRightRearTireBone { get; private set; }
        public string MidLeftRearTireBone { get; private set; }
        public string RightRearTireBone { get; private set; }
        public string LeftRearTireBone { get; private set; }

        // Dust spray configuration
        public string Dust { get; private set; }
        public string DirtSpray { get; private set; }
        public string PowerslideSpray { get; private set; }
    }
}

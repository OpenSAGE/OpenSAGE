﻿using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Special-case logic allows for ParentObject to be specified as a bone name to allow other 
    /// objects to appear on the bridge.
    /// </summary>
    public sealed class BridgeBehaviorModuleData : BehaviorModuleData
    {
        internal static BridgeBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BridgeBehaviorModuleData> FieldParseTable = new IniParseTable<BridgeBehaviorModuleData>
        {
            { "LateralScaffoldSpeed", (parser, x) => x.LateralScaffoldSpeed = parser.ParseFloat() },
            { "VerticalScaffoldSpeed", (parser, x) => x.VerticalScaffoldSpeed = parser.ParseFloat() },

            { "BridgeDieOCL", (parser, x) => x.BridgeDieOCLs.Add(BridgeDieObjectCreationList.Parse(parser)) },
            { "BridgeDieFX", (parser, x) => x.BridgeDieFXs.Add(BridgeDieFX.Parse(parser)) }
        };

        public float LateralScaffoldSpeed { get; private set; }
        public float VerticalScaffoldSpeed { get; private set; }

        public List<BridgeDieObjectCreationList> BridgeDieOCLs { get; } = new List<BridgeDieObjectCreationList>();
        public List<BridgeDieFX> BridgeDieFXs { get; } = new List<BridgeDieFX>();
    }

    public sealed class BridgeDieObjectCreationList
    {
        internal static BridgeDieObjectCreationList Parse(IniParser parser)
        {
            return new BridgeDieObjectCreationList
            {
                OCL = parser.ParseAttribute("OCL", parser.ScanAssetReference),
                Delay = parser.ParseAttributeInteger("Delay"),
                Bone = parser.ParseAttribute("Bone", parser.ScanBoneName)
            };
        }

        public string OCL { get; private set; }
        public int Delay { get; private set; }
        public string Bone { get; private set; }
    }

    public sealed class BridgeDieFX
    {
        internal static BridgeDieFX Parse(IniParser parser)
        {
            return new BridgeDieFX
            {
                FX = parser.ParseAttribute("FX", parser.ScanAssetReference),
                Delay = parser.ParseAttributeInteger("Delay"),
                Bone = parser.ParseAttribute("Bone", parser.ScanBoneName)
            };
        }

        public string FX { get; private set; }
        public int Delay { get; private set; }
        public string Bone { get; private set; }
    }
}

using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Logic requires bones for either end of the rope to be defined as RopeEnd and RopeStart.
    /// Infantry (or tanks) can be made to rappel down a rope by adding CAN_RAPPEL to the object's 
    /// KindOf field. Having done that, the "RAPPELLING" ModelConditonState becomes available for 
    /// rappelling out of the object that has the rappel code of this module.
    /// </summary>
    public sealed class ChinookAIUpdate : AIUpdateInterface
    {
        internal static new ChinookAIUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ChinookAIUpdate> FieldParseTable = new IniParseTable<ChinookAIUpdate>
        {
            { "MaxBoxes", (parser, x) => x.MaxBoxes = parser.ParseInteger() },
            { "SupplyCenterActionDelay", (parser, x) => x.SupplyCenterActionDelay = parser.ParseInteger() },
            { "SupplyWarehouseActionDelay", (parser, x) => x.SupplyWarehouseActionDelay = parser.ParseInteger() },
            { "SupplyWarehouseScanDistance", (parser, x) => x.SupplyWarehouseScanDistance = parser.ParseInteger() },
            { "SuppliesDepletedVoice", (parser, x) => x.SuppliesDepletedVoice = parser.ParseAssetReference() },

            { "NumRopes", (parser, x) => x.NumRopes = parser.ParseInteger() },
            { "PerRopeDelayMin", (parser, x) => x.PerRopeDelayMin = parser.ParseInteger() },
            { "PerRopeDelayMax", (parser, x) => x.PerRopeDelayMax = parser.ParseInteger() },
            { "RopeWidth", (parser, x) => x.RopeWidth = parser.ParseFloat() },
            { "RopeColor", (parser, x) => x.RopeColor = IniColorRgb.Parse(parser) },
            { "RopeWobbleLen", (parser, x) => x.RopeWobbleLen = parser.ParseInteger() },
            { "RopeWobbleAmplitude", (parser, x) => x.RopeWobbleAmplitude = parser.ParseFloat() },
            { "RopeWobbleRate", (parser, x) => x.RopeWobbleRate = parser.ParseInteger() },
            { "RopeFinalHeight", (parser, x) => x.RopeFinalHeight = parser.ParseInteger() },
            { "RappelSpeed", (parser, x) => x.RappelSpeed = parser.ParseInteger() },
            { "MinDropHeight", (parser, x) => x.MinDropHeight = parser.ParseInteger() },
        }.Concat<ChinookAIUpdate, AIUpdateInterface>(BaseFieldParseTable);

        public int MaxBoxes { get; private set; }
        public int SupplyCenterActionDelay { get; private set; }
        public int SupplyWarehouseActionDelay { get; private set; }
        public int SupplyWarehouseScanDistance { get; private set; }
        public string SuppliesDepletedVoice { get; private set; }

        public int NumRopes { get; private set; }
        public int PerRopeDelayMin { get; private set; }
        public int PerRopeDelayMax { get; private set; }
        public float RopeWidth { get; private set; }
        public IniColorRgb RopeColor { get; private set; }
        public int RopeWobbleLen { get; private set; }
        public float RopeWobbleAmplitude { get; private set; }
        public int RopeWobbleRate { get; private set; }
        public int RopeFinalHeight { get; private set; }
        public int RappelSpeed { get; private set; }
        public int MinDropHeight { get; private set; }
    }
}

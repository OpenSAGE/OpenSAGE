using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows use of UnitPack, UnitUnpack, and UnitCashPing within the UnitSpecificSounds section 
    /// of the object.
    /// Also allows use of PACKING and UNPACKING condition states.
    /// </summary>
    public sealed class HackInternetAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static HackInternetAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<HackInternetAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<HackInternetAIUpdateModuleData>
            {
                { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
                { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
                { "CashUpdateDelay", (parser, x) => x.CashUpdateDelay = parser.ParseInteger() },
                { "CashUpdateDelayFast", (parser, x) => x.CashUpdateDelayFast = parser.ParseInteger() },
                { "RegularCashAmount", (parser, x) => x.RegularCashAmount = parser.ParseInteger() },
                { "VeteranCashAmount", (parser, x) => x.VeteranCashAmount = parser.ParseInteger() },
                { "EliteCashAmount", (parser, x) => x.EliteCashAmount = parser.ParseInteger() },
                { "HeroicCashAmount", (parser, x) => x.HeroicCashAmount = parser.ParseInteger() },
                { "XpPerCashUpdate", (parser, x) => x.XpPerCashUpdate = parser.ParseInteger() },
                { "PackUnpackVariationFactor", (parser, x) => x.PackUnpackVariationFactor = parser.ParseFloat() },
            });

        public int UnpackTime { get; private set; }
        public int PackTime { get; private set; }
        public int CashUpdateDelay { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int CashUpdateDelayFast { get; private set; }

        public int RegularCashAmount { get; private set; }
        public int VeteranCashAmount { get; private set; }
        public int EliteCashAmount { get; private set; }
        public int HeroicCashAmount { get; private set; }
        public int XpPerCashUpdate { get; private set; }
        public float PackUnpackVariationFactor { get; private set; }
    }
}

using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class OCLUpdateModuleData : UpdateModuleData
    {
        internal static OCLUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<OCLUpdateModuleData> FieldParseTable = new IniParseTable<OCLUpdateModuleData>
        {
            { "OCL", (parser, x) => x.OCL = parser.ParseAssetReference() },
            { "MinDelay", (parser, x) => x.MinDelay = parser.ParseInteger() },
            { "MaxDelay", (parser, x) => x.MaxDelay = parser.ParseInteger() },
            { "CreateAtEdge", (parser, x) => x.CreateAtEdge = parser.ParseBoolean() },
            { "FactionTriggered", (parser, x) => x.FactionTriggered = parser.ParseBoolean() },
            { "FactionOCL", (parser, x) => x.FactionOCLs.Add(FactionOCL.Parse(parser)) }
        };

        public string OCL { get; private set; }
        public int MinDelay { get; private set; }
        public int MaxDelay { get; private set; }
        public bool CreateAtEdge { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool FactionTriggered { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public List<FactionOCL> FactionOCLs { get; } = new List<FactionOCL>();
    }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public struct FactionOCL
    {
        internal static FactionOCL Parse(IniParser parser)
        {
            return new FactionOCL
            {
                Faction = parser.ParseAttribute("Faction", parser.ScanAssetReference),
                OCL = parser.ParseAttribute("OCL", parser.ScanAssetReference),
            };
        }

        public string Faction;
        public string OCL;
    }
}

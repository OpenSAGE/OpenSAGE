﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class FireSpreadUpdateModuleData : UpdateModuleData
    {
        internal static FireSpreadUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FireSpreadUpdateModuleData> FieldParseTable = new IniParseTable<FireSpreadUpdateModuleData>
        {
            { "OCLEmbers", (parser, x) => x.OCLEmbers = parser.ParseAssetReference() },
            { "MinSpreadDelay", (parser, x) => x.MinSpreadDelay = parser.ParseInteger() },
            { "MaxSpreadDelay", (parser, x) => x.MaxSpreadDelay = parser.ParseInteger() },
            { "SpreadTryRange", (parser, x) => x.SpreadTryRange = parser.ParseInteger() }
        };

        public string OCLEmbers { get; private set; }
        public int MinSpreadDelay { get; private set; }
        public int MaxSpreadDelay { get; private set; }
        public int SpreadTryRange { get; private set; }
    }
}

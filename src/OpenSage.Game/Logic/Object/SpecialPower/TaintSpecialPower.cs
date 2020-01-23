using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class TaintSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new TaintSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<TaintSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<TaintSpecialPowerModuleData>
            {
                { "TaintObject", (parser, x) => x.TaintObject = parser.ParseIdentifier() },
                { "TaintRadius", (parser, x) => x.TaintRadius = parser.ParseInteger() },
                { "TaintFX", (parser, x) => x.TaintFX = parser.ParseAssetReference() },
                { "TaintOCL", (parser, x) => x.TaintOcl = parser.ParseAssetReference() }
            });

        public string TaintObject { get; private set; }
        public int TaintRadius { get; private set; }
        public string TaintFX { get; private set; }
        public string TaintOcl { get; private set; }
    }
}

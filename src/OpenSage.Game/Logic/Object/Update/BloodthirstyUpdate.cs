using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class BloodthirstyUpdateModuleData : UpdateModuleData
    {
        internal static BloodthirstyUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BloodthirstyUpdateModuleData> FieldParseTable = new IniParseTable<BloodthirstyUpdateModuleData>
        {
            { "SacrificeFilter", (parser, x) => x.SacrificeFilter = ObjectFilter.Parse(parser) },
            { "NumToSacrifice", (parser, x) => x.NumToSacrifice = parser.ParseInteger() },
            { "InitiateVoice", (parser, x) => x.InitiateVoice = parser.ParseAssetReference() },
            { "InitiateVoice2", (parser, x) => x.InitiateVoice2 = parser.ParseAssetReference() },
        };
        public ObjectFilter SacrificeFilter { get; private set; }
        public int NumToSacrifice { get; private set; }
        public string InitiateVoice { get; private set; }
        public string InitiateVoice2 { get; private set; }
    }
}

using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class CloudBreakSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new CloudBreakSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CloudBreakSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<CloudBreakSpecialPowerModuleData>
            {
                { "SunbeamObject", (parser, x) => x.SunbeamObject = parser.ParseAssetReference() },
                { "ObjectSpacing", (parser, x) => x.ObjectSpacing = parser.ParseInteger() },
                { "AntiFX", (parser, x) => x.AntiFX = parser.ParseAssetReference() },
                { "ReEnableAntiCategory", (parser, x) => x.ReEnableAntiCategory = parser.ParseBoolean() },
            });

        public string SunbeamObject { get; private set; }
        public int ObjectSpacing { get; private set; }
        public string AntiFX { get; private set; }
        public bool ReEnableAntiCategory { get; private set; }
    }
}

using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Like Transport, but when full, passes transport queries along to first passenger 
    /// (redirects like tunnel).
    /// </summary>
    public sealed class OverlordContainModuleData : TransportContainModuleData
    {
        internal static new OverlordContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<OverlordContainModuleData> FieldParseTable = TransportContainModuleData.FieldParseTable
            .Concat(new IniParseTable<OverlordContainModuleData>
            {
                { "PayloadTemplateName", (parser, x) => x.PayloadTemplateName = parser.ParseAssetReference() },
                { "ExperienceSinkForRider", (parser, x) => x.ExperienceSinkForRider = parser.ParseBoolean() }
            });

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string PayloadTemplateName { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool ExperienceSinkForRider { get; private set; }
    }
}

using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class RefundDieModuleData : DieModuleData
    {
        internal static RefundDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<RefundDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<RefundDieModuleData>
            {
                { "UpgradeRequired", (parser, x) => x.UpgradeRequired = parser.ParseAssetReference() },
                { "BuildingRequired", (parser, x) => x.BuildingRequired = ObjectFilter.Parse(parser) },
                { "RefundPercent", (parser, x) => x.RefundPercent = parser.ParsePercentage() }
            });

        public string UpgradeRequired { get; internal set; }
        public ObjectFilter BuildingRequired { get; internal set; }
        public float RefundPercent { get; internal set; }
    }
}

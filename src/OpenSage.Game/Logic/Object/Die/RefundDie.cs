using OpenSage.Data.Ini;
using OpenSage.Mathematics;

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

        public string UpgradeRequired { get; private set; }
        public ObjectFilter BuildingRequired { get; private set; }
        public Percentage RefundPercent { get; private set; }
    }
}

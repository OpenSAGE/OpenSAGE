using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    public sealed class FXListAtBonePosFXNuggetData : FXNuggetData
    {
        internal static FXListAtBonePosFXNuggetData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FXListAtBonePosFXNuggetData> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<FXListAtBonePosFXNuggetData>
        {
            { "FX", (parser, x) => x.FX = parser.ParseAssetReference() },
            { "BoneName", (parser, x) => x.BoneName = parser.ParseAssetReference() },
            { "OrientToBone", (parser, x) => x.OrientToBone = parser.ParseBoolean() },
        });

        public string FX { get; private set; }
        public string BoneName { get; private set; }
        public bool OrientToBone { get; private set; }
    }
}

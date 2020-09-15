using OpenSage.Data.Ini;

namespace OpenSage.Logic
{
    public sealed class Science : BaseAsset
    {
        internal static Science Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("Science", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<Science> FieldParseTable = new IniParseTable<Science>
        {
            { "PrerequisiteSciences", (parser, x) => x.PrerequisiteSciences = parser.ParseAssetReferenceArray() },
            { "SciencePurchasePointCost", (parser, x) => x.SciencePurchasePointCost = parser.ParseInteger() },
            { "IsGrantable", (parser, x) => x.IsGrantable = parser.ParseBoolean() },
            { "DisplayName", (parser, x) => x.DisplayName = parser.ParseLocalizedStringKey() },
            { "Description", (parser, x) => x.Description = parser.ParseLocalizedStringKey() },
            { "SciencePurchasePointCostMP", (parser, x) => x.SciencePurchasePointCostMP = parser.ParseInteger() }
        };

        public string[] PrerequisiteSciences { get; private set; }
        public int SciencePurchasePointCost { get; private set; }
        public bool IsGrantable { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int  SciencePurchasePointCostMP { get; private set; }
    }

    public enum ScienceAvailability
    {
        Available = 0,
        Disabled  = 1,
        Hidden    = 2,
    }
}

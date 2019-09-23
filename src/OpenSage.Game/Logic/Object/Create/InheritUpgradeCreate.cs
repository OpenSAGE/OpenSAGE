using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class InheritUpgradeCreateModuleData : CreateModuleData
    {
        internal static InheritUpgradeCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<InheritUpgradeCreateModuleData> FieldParseTable = new IniParseTable<InheritUpgradeCreateModuleData>
        {
            { "Radius", (parser, x) => x.Radius = parser.ParseFloat() },
            { "Upgrade", (parser, x) => x.Upgrade = parser.ParseAssetReference() },
            { "ObjectFilter", (parser, x) => x.ObjectFilter = ObjectFilter.Parse(parser) }
        };

        public float Radius { get; private set; }
        public string Upgrade { get; private set; }
        public ObjectFilter ObjectFilter { get; private set; }
    }
}

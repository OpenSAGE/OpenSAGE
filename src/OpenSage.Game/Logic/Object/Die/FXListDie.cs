using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class FXListDieModuleData : DieModuleData
    {
        internal static FXListDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<FXListDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<FXListDieModuleData>
            {
                { "DeathFX", (parser, x) => x.DeathFX = parser.ParseAssetReference() },
                { "OrientToObject", (parser, x) => x.OrientToObject = parser.ParseBoolean() },
                { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
                { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseAssetReferenceArray() },
                { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() }
            });

        public string DeathFX { get; private set; }
        public bool OrientToObject { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool StartsActive { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string[] ConflictsWith { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string[] TriggeredBy { get; private set; }
    }
}

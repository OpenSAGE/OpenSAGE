using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public abstract class UpgradeModuleData : BehaviorModuleData
    {
        internal static readonly IniParseTable<UpgradeModuleData> FieldParseTable = new IniParseTable<UpgradeModuleData>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() },
            { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseAssetReferenceArray() },
            { "RequiresAllTriggers", (parser, x) => x.RequiresAllTriggers = parser.ParseBoolean() },
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
        };

        public string[] TriggeredBy { get; private set; }
        public string[] ConflictsWith { get; private set; }
        public bool RequiresAllTriggers { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool StartsActive { get; private set; }
    }
}

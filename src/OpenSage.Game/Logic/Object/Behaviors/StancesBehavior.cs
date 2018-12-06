using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class StancesBehaviorModuleData : UpgradeModuleData
    {
        internal static StancesBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<StancesBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<StancesBehaviorModuleData>
            {
                { "StanceTemplate", (parser, x) => x.StanceTemplate = parser.ParseIdentifier() },
               
            });

        public string StanceTemplate { get; private set; }
      
    }
}

using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2Rotwk)]
    public sealed class FreeLifeBodyModuleData : ActiveBodyModuleData
    {
        internal static new FreeLifeBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<FreeLifeBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
            .Concat(new IniParseTable<FreeLifeBodyModuleData>
            {
                { "FreeLifeHealthPercent", (parser, x) => x.FreeLifeHealthPercent = parser.ParseInteger() },
                { "FreeLifeTime", (parser, x) => x.FreeLifeTime = parser.ParseInteger() },
                { "FreeLifeInvincible", (parser, x) => x.FreeLifeInvincible = parser.ParseBoolean() },
                { "FreeLifePrerequisiteUpgrade", (parser, x) => x.FreeLifePrerequisiteUpgrade = parser.ParseAssetReference() },
                { "FreeLifeAnimAndDuration", (parser, x) => x.FreeLifeAnimAndDuration = AnimAndDuration.Parse(parser) }
            });

        public int FreeLifeHealthPercent { get; private set; }
        public int FreeLifeTime { get; private set; }
        public bool FreeLifeInvincible { get; private set; }
        public string FreeLifePrerequisiteUpgrade { get; private set; }
        public AnimAndDuration FreeLifeAnimAndDuration { get; private set; }
     }
}

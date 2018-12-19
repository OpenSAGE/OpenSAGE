using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class FoundationAIUpdateModuleData : AIUpdateModuleData
    {
        internal static new FoundationAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<FoundationAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<FoundationAIUpdateModuleData>
            {
                { "BuildVariation", (parser, x) => x.BuildVariation = parser.ParseInteger() },
            });

        [AddedIn(SageGame.Bfme2)]
        public int BuildVariation { get; private set; }
    }
}

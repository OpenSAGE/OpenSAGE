using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class FoundationAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static FoundationAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<FoundationAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<FoundationAIUpdateModuleData>
            {
                { "BuildVariation", (parser, x) => x.BuildVariation = parser.ParseInteger() },
            });

        [AddedIn(SageGame.Bfme2)]
        public int BuildVariation { get; private set; }
    }
}

using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    [AddedIn(SageGame.Bfme)]
    public sealed class EvaEventFXNugget : FXNugget
    {
        internal static EvaEventFXNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<EvaEventFXNugget> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<EvaEventFXNugget>
        {
            { "EvaEventOwner", (parser, x) => x.EvaEventOwner = parser.ParseAssetReference() },
            { "EvaEventAlly", (parser, x) => x.EvaEventAlly = parser.ParseAssetReference() }
        });

        public string EvaEventOwner { get; private set; }
        public string EvaEventAlly { get; private set; }
    }
}

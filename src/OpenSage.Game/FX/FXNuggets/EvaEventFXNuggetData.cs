using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    [AddedIn(SageGame.Bfme)]
    public sealed class EvaEventFXNuggetData : FXNuggetData
    {
        internal static EvaEventFXNuggetData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<EvaEventFXNuggetData> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<EvaEventFXNuggetData>
        {
            { "EvaEventOwner", (parser, x) => x.EvaEventOwner = parser.ParseAssetReference() },
            { "EvaEventAlly", (parser, x) => x.EvaEventAlly = parser.ParseAssetReference() }
        });

        public string EvaEventOwner { get; private set; }
        public string EvaEventAlly { get; private set; }
    }
}

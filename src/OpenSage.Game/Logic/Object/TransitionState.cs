using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class TransitionState : ObjectDrawState
    {
        internal static TransitionState Parse(IniParser parser)
        {
            var from = parser.ParseAssetReference();
            var to = parser.ParseAssetReference();

            // ODDITY: FactionBuilding.ini:849 has a mistake:
            // UP_SNOW NIGHT DOWN_DEFAULT
            // It should be:
            // UP_SNOWNIGHT DOWN_DEFAULT
            if (parser.CurrentTokenType == IniTokenType.Identifier)
            {
                var extra = parser.ParseAssetReference();

                from = from + to;
                to = extra;
            }

            var result = parser.ParseBlock(FieldParseTable);

            result.FromTransitionKey = from;
            result.ToTransitionKey = to;

            return result;
        }

        private static readonly IniParseTable<TransitionState> FieldParseTable = new IniParseTable<TransitionState>
        {
            { "FromTransitionKey", (parser, x) => x.FromTransitionKey = parser.ParseAssetReference() },
            { "ToTransitionKey", (parser, x) => x.ToTransitionKey = parser.ParseAssetReference() },
        }.Concat<TransitionState, ObjectDrawState>(BaseFieldParseTable);

        public string FromTransitionKey { get; private set; }
        public string ToTransitionKey { get; private set; }
    }
}

using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class TransitionState : ModelConditionState
    {
        internal static new TransitionState Parse(IniParser parser)
        {
            var from = parser.ParseAssetReference();
            var to = parser.ParseAssetReference();

            var result = parser.ParseBlock(FieldParseTable);

            result.FromTransitionKey = from;
            result.ToTransitionKey = to;

            return result;
        }

        private static new readonly IniParseTable<TransitionState> FieldParseTable = ModelConditionState.FieldParseTable
            .Concat(new IniParseTable<TransitionState>());

        public string FromTransitionKey { get; private set; }
        public string ToTransitionKey { get; private set; }
    }
}

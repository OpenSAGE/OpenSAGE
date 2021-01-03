using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public class GeneralsTransitionState : ModelConditionState
    {
        internal static GeneralsTransitionState Parse(IniParser parser)
        {
            var from = parser.ParseAssetReference();
            var to = "";
            var token = parser.GetNextTokenOptional();
            if (token.HasValue)
            {
                to = parser.ScanAssetReference(token.Value);
            }

            var result = parser.ParseBlock(FieldParseTable);
            result.FromTransitionKey = from;
            result.ToTransitionKey = to;
            return result;
        }

        private static readonly new IniParseTable<GeneralsTransitionState> FieldParseTable = ModelConditionState.FieldParseTable
            .Concat(new IniParseTable<GeneralsTransitionState>
            {
            });

        public string FromTransitionKey { get; private set; }
        public string ToTransitionKey { get; private set; }
    }


    public class BfmeTransitionState : AnimationState
    {
        internal static new BfmeTransitionState Parse(IniParser parser)
        {
            var name = parser.ParseString();
            var result = parser.ParseBlock(FieldParseTable);
            result.Name = name;
            return result;
        }

        private static readonly new IniParseTable<BfmeTransitionState> FieldParseTable = AnimationState.FieldParseTable
            .Concat(new IniParseTable<BfmeTransitionState>
            {
            });

        public string Name { get; private set; }
    }
}

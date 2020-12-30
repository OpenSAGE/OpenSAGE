using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class TransitionState : AnimationState
    {
        internal static new TransitionState Parse(IniParser parser)
        {
            var from = parser.ParseAssetReference();
            var to = "";
            var token = parser.GetNextTokenOptional();
            if (token.HasValue)
            {
                to = parser.ScanAssetReference(token.Value);
            }
            
            var result = parser.ParseBlock(FieldParseTable);
            result.Name = from;
            result.FromTransitionKey = from;
            result.ToTransitionKey = to;

            return result;
        }

        private static new readonly IniParseTable<TransitionState> FieldParseTable = AnimationState.FieldParseTable
            .Concat(new IniParseTable<TransitionState>()
            {
            });

        public string Name { get; private set; }

        public string FromTransitionKey { get; private set; }
        public string ToTransitionKey { get; private set; }
    }
}

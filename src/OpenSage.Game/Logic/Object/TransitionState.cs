using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class TransitionState : ModelConditionState
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

            result.FromTransitionKey = from;
            result.ToTransitionKey = to;

            return result;
        }

        private static new readonly IniParseTable<TransitionState> FieldParseTable = ModelConditionState.FieldParseTable
            .Concat(new IniParseTable<TransitionState>()
            {
                { "FXEvent", (parser, x) => x.FXEvents.Add(FXEvent.Parse(parser)) },
                { "EnteringStateFX", (parser, x) => x.EnteringStateFX = parser.ParseAssetReference() },
            });

        public string FromTransitionKey { get; private set; }
        public string ToTransitionKey { get; private set; }
        public List<FXEvent> FXEvents { get; private set; } = new List<FXEvent>();

        [AddedIn(SageGame.Bfme)]
        public string EnteringStateFX { get; private set; }
    }
}

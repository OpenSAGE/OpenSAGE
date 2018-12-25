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
                { "EnteringStateFX", (parser, x) => x.EnteringStateFX = parser.ParseAssetReference() },
                { "BeginScript", (parser, x) => x.Script = IniScript.Parse(parser) },
                { "LuaEvent", (parser, x) => x.LuaEvents.Add(LuaEvent.Parse(parser)) }
            });

        public string FromTransitionKey { get; private set; }
        public string ToTransitionKey { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string EnteringStateFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public IniScript Script { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<LuaEvent> LuaEvents { get; } = new List<LuaEvent>();
    }
}

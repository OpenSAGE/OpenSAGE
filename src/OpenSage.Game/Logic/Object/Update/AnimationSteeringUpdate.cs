using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class AnimationSteeringUpdate : ObjectBehavior
    {
        internal static AnimationSteeringUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AnimationSteeringUpdate> FieldParseTable = new IniParseTable<AnimationSteeringUpdate>
        {
            { "MinTransitionTime", (parser, x) => x.MinTransitionTime = parser.ParseInteger() }
        };

        public int MinTransitionTime { get; private set; }
    }
}

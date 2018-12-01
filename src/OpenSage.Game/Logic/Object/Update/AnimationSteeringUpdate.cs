using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class AnimationSteeringUpdateModuleData : UpdateModuleData
    {
        internal static AnimationSteeringUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AnimationSteeringUpdateModuleData> FieldParseTable = new IniParseTable<AnimationSteeringUpdateModuleData>
        {
            { "MinTransitionTime", (parser, x) => x.MinTransitionTime = parser.ParseInteger() }
        };

        public int MinTransitionTime { get; private set; }
    }
}

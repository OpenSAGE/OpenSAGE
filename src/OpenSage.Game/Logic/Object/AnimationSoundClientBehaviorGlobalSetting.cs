using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AnimationSoundClientBehaviorGlobalSetting
    {
        internal static AnimationSoundClientBehaviorGlobalSetting Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<AnimationSoundClientBehaviorGlobalSetting> FieldParseTable = new IniParseTable<AnimationSoundClientBehaviorGlobalSetting>
        {
            { "MinMicrophoneDistanceToDirty", (parser, x) => x.MinMicrophoneDistanceToDirty = parser.ParseFloat() },
        };

        public float MinMicrophoneDistanceToDirty { get; private set; }
    }
}

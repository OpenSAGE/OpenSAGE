using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class ModelConditionSoundSelectorClientBehaviorData : ClientBehaviorModuleData
    {
        internal static ModelConditionSoundSelectorClientBehaviorData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<ModelConditionSoundSelectorClientBehaviorData> FieldParseTable = new IniParseTable<ModelConditionSoundSelectorClientBehaviorData>
        {
            { "SoundState", (parser, x) => x.SoundState = SoundState.Parse(parser) }
        };

        public SoundState SoundState { get; internal set; }
    }

    public sealed class SoundState
    {
        internal static SoundState Parse(IniParser parser)
        {
            var condition = parser.ParseEnum<ModelConditionFlag>();
            var result = parser.ParseBlock(FieldParseTable);
            result.Condition = condition;
            return result;
        }

        internal static readonly IniParseTable<SoundState> FieldParseTable = new IniParseTable<SoundState>
        {
             { "VoiceSelect", (parser, x) => x.VoiceSelect = parser.ParseAssetReference() },
             { "VoiceSelect2", (parser, x) => x.VoiceSelect2 = parser.ParseAssetReference() },
             { "VoiceSelectBattle", (parser, x) => x.VoiceSelectBattle = parser.ParseAssetReference() },
             { "VoiceSelectBattle2", (parser, x) => x.VoiceSelectBattle2 = parser.ParseAssetReference() },
             { "VoiceMove", (parser, x) => x.VoiceMove = parser.ParseAssetReference() },
             { "VoiceMove2", (parser, x) => x.VoiceMove2 = parser.ParseAssetReference() },
             { "VoiceMoveToCamp", (parser, x) => x.VoiceMoveToCamp = parser.ParseAssetReference() },
             { "VoiceMoveWhileAttacking", (parser, x) => x.VoiceMoveWhileAttacking = parser.ParseAssetReference() },
             { "VoiceEnterStateMove", (parser, x) => x.VoiceEnterStateMove = parser.ParseAssetReference() },
             { "VoiceEnterStateMoveToCamp", (parser, x) => x.VoiceEnterStateMoveToCamp = parser.ParseAssetReference() },
             { "VoiceEnterStateMoveWhileAttacking", (parser, x) => x.VoiceEnterStateMoveWhileAttacking = parser.ParseAssetReference() },
        };

        public ModelConditionFlag Condition { get; internal set; }

        public string VoiceSelect { get; internal set; }
        public string VoiceSelect2 { get; internal set; }
        public string VoiceSelectBattle { get; internal set; }
        public string VoiceSelectBattle2 { get; internal set; }
        public string VoiceMove { get; internal set; }
        public string VoiceMove2 { get; internal set; }
        public string VoiceMoveToCamp { get; internal set; }
        public string VoiceMoveWhileAttacking { get; internal set; }
        public string VoiceEnterStateMove { get; internal set; }
        public string VoiceEnterStateMoveToCamp { get; internal set; }
        public string VoiceEnterStateMoveWhileAttacking { get; internal set; }
    }
}

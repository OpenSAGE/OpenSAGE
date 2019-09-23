using OpenSage.Data.Ini;

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

        public SoundState SoundState { get; private set; }
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
             { "VoiceAttack", (parser, x) => x.VoiceAttack = parser.ParseAssetReference() },
             { "VoiceAttackCharge", (parser, x) => x.VoiceAttackCharge = parser.ParseAssetReference() },
             { "VoiceAttackMachine", (parser, x) => x.VoiceAttackMachine = parser.ParseAssetReference() },
             { "VoiceAttackStructure", (parser, x) => x.VoiceAttackStructure = parser.ParseAssetReference() },
             { "VoiceFear", (parser, x) => x.VoiceFear = parser.ParseAssetReference() },
             { "VoiceRetreatToCastle", (parser, x) => x.VoiceRetreatToCastle = parser.ParseAssetReference() },
             { "VoiceGuard", (parser, x) => x.VoiceGuard = parser.ParseAssetReference() },
             { "SoundMoveStart", (parser, x) => x.SoundMoveStart = parser.ParseAssetReference() },
             { "SoundImpact", (parser, x) => x.SoundImpact = parser.ParseAssetReference() },
             { "VoicePriority", (parser, x) => x.VoicePriority = parser.ParseInteger() },
             { "UnitSpecificSounds", (parser, x) => x.UnitSpecificSounds = UnitSpecificSounds.Parse(parser) },
             { "SoundMoveLoop", (parser, x) => x.SoundMoveLoop = parser.ParseAssetReference() },
             { "VoiceEnterStateAttackCharge", (parser, x) => x.VoiceEnterStateAttackCharge = parser.ParseAssetReference() },
             { "VoiceCreated", (parser, x) => x.VoiceCreated = parser.ParseAssetReference() },
             { "VoiceFullyCreated", (parser, x) => x.VoiceFullyCreated = parser.ParseAssetReference() }
        };

        public ModelConditionFlag Condition { get; private set; }

        public string VoiceSelect { get; private set; }
        public string VoiceSelect2 { get; private set; }
        public string VoiceSelectBattle { get; private set; }
        public string VoiceSelectBattle2 { get; private set; }
        public string VoiceMove { get; private set; }
        public string VoiceMove2 { get; private set; }
        public string VoiceMoveToCamp { get; private set; }
        public string VoiceMoveWhileAttacking { get; private set; }
        public string VoiceEnterStateMove { get; private set; }
        public string VoiceEnterStateMoveToCamp { get; private set; }
        public string VoiceEnterStateMoveWhileAttacking { get; private set; }
        public string VoiceAttack { get; private set; }
        public string VoiceAttackCharge { get; private set; }
        public string VoiceAttackMachine { get; private set; }
        public string VoiceAttackStructure { get; private set; }
        public string VoiceFear { get; private set; }
        public string VoiceRetreatToCastle { get; private set; }
        public string VoiceGuard { get; private set; }
        public string SoundMoveStart { get; private set; }
        public string SoundImpact { get; private set; }
        public int VoicePriority { get; private set; }
        public UnitSpecificSounds UnitSpecificSounds { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string SoundMoveLoop { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string VoiceEnterStateAttackCharge { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string VoiceCreated { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string VoiceFullyCreated { get; private set; }
    }

    public class UnitSpecificSounds
    {
        internal static UnitSpecificSounds Parse (IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<UnitSpecificSounds> FieldParseTable = new IniParseTable<UnitSpecificSounds>
        {
            { "VoiceGarrison", (parser, x) => x.VoiceGarrison = parser.ParseAssetReference() },

            { "VoiceEnterUnitElvenTransportShip", (parser, x) => x.VoiceEnterUnitElvenTransportShip = parser.ParseAssetReference() },
            { "VoiceEnterUnitMordorMumakil", (parser, x) => x.VoiceEnterUnitMordorMumakil = parser.ParseAssetReference() },
            { "VoiceEnterUnitSlaughterHouse", (parser, x) => x.VoiceEnterUnitSlaughterHouse = parser.ParseAssetReference() },
            { "VoiceEnterUnitTransportShip", (parser, x) => x.VoiceEnterUnitTransportShip = parser.ParseAssetReference() },
            { "VoiceInitiateCaptureBuilding", (parser, x) => x.VoiceInitiateCaptureBuilding = parser.ParseAssetReference() },
            { "VoiceAttackFireball", (parser, x) => x.VoiceAttackFireball = parser.ParseAssetReference() },
            { "VoiceMoveToTrees", (parser, x) => x.VoiceMoveToTrees = parser.ParseAssetReference() },
        };

        public string VoiceGarrison { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceEnterUnitElvenTransportShip { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceEnterUnitMordorMumakil { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceEnterUnitSlaughterHouse { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceEnterUnitTransportShip { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceInitiateCaptureBuilding { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceAttackFireball { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceMoveToTrees { get; private set; }
    }
}

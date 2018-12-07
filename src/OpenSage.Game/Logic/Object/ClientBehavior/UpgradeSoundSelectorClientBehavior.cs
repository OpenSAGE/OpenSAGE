using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class UpgradeSoundSelectorClientBehaviorData : ClientBehaviorModuleData
    {
        internal static UpgradeSoundSelectorClientBehaviorData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<UpgradeSoundSelectorClientBehaviorData> FieldParseTable = new IniParseTable<UpgradeSoundSelectorClientBehaviorData>
        {
            { "SoundUpgrade", (parser, x) => x.SoundUpgrades.Add(SoundUpgrade.Parse(parser)) }
        };

        public List<SoundUpgrade> SoundUpgrades { get; private set; } = new List<SoundUpgrade>();
    }


    public sealed class SoundUpgrade
    {
        internal static SoundUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<SoundUpgrade> FieldParseTable = new IniParseTable<SoundUpgrade>
        {
            { "RequiredModelConditions", (parser, x) => x.RequiredModelConditions = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "VoiceSelect", (parser, x) => x.VoiceSelect = parser.ParseAssetReference() },
            { "ExcludedUpgrades", (parser, x) => x.ExcludedUpgrades = parser.ParseAssetReferenceArray() },
            { "VoiceAttack", (parser, x) => x.VoiceAttack = parser.ParseAssetReference() },
            { "VoiceAttackAir", (parser, x) => x.VoiceAttackAir = parser.ParseAssetReference() },
            { "VoiceAttackCharge", (parser, x) => x.VoiceAttackCharge = parser.ParseAssetReference() },
            { "VoiceAttackMachine", (parser, x) => x.VoiceAttackMachine = parser.ParseAssetReference() },
            { "VoiceAttackStructure", (parser, x) => x.VoiceAttackStructure = parser.ParseAssetReference() },
            { "VoiceCreated", (parser, x) => x.VoiceCreated = parser.ParseAssetReference() },
            { "VoiceFear", (parser, x) => x.VoiceFear = parser.ParseAssetReference() },
            { "VoiceFullyCreated", (parser, x) => x.VoiceFullyCreated = parser.ParseAssetReference() },
            { "VoiceGuard", (parser, x) => x.VoiceGuard = parser.ParseAssetReference() },
            { "VoiceMove", (parser, x) => x.VoiceMove = parser.ParseAssetReference() },
            { "VoiceMoveToCamp", (parser, x) => x.VoiceMoveToCamp = parser.ParseAssetReference() },
            { "VoiceMoveWhileAttacking", (parser, x) => x.VoiceMoveWhileAttacking = parser.ParseAssetReference() },
            { "VoicePriority", (parser, x) => x.VoicePriority = parser.ParseInteger() },
            { "VoiceRetreatToCastle", (parser, x) => x.VoiceRetreatToCastle = parser.ParseAssetReference() },
            { "VoiceSelectBattle", (parser, x) => x.VoiceSelectBattle = parser.ParseAssetReference() },
            { "SoundImpact", (parser, x) => x.SoundImpact = parser.ParseAssetReference() },
            { "UnitSpecificSounds", (parser, x) => x.UnitSpecificSounds = UnitSpecificSounds.Parse(parser) },
        };

        [AddedIn(SageGame.Bfme2)]
        public BitArray<ModelConditionFlag> RequiredModelConditions { get; private set; }

        public string VoiceSelect { get; private set; }
        public string[] ExcludedUpgrades { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceAttack { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceAttackAir { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceAttackCharge { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceAttackMachine { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceAttackStructure { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceCreated { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceFear { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceFullyCreated { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceGuard { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceMove { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceMoveToCamp { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceMoveWhileAttacking { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int VoicePriority { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceRetreatToCastle { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string VoiceSelectBattle { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string SoundImpact { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public UnitSpecificSounds UnitSpecificSounds { get; private set; }
    }
}

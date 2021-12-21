using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class StealthUpdate : UpdateModule
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            // TODO

            var frameSomething = reader.ReadUInt32();

            var frameSomething2 = reader.ReadUInt32();

            var unknownBool1 = reader.ReadBoolean();
            if (!unknownBool1)
            {
                throw new InvalidStateException();
            }

            var unknownFloat1 = reader.ReadSingle();
            var unknownFloat2 = reader.ReadSingle();

            var unknownInt2 = reader.ReadInt32();
            if (unknownInt2 != -1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(8);
        }
    }

    /// <summary>
    /// Allows the use of the <see cref="ObjectDefinition.SoundStealthOn"/> and 
    /// <see cref="ObjectDefinition.SoundStealthOff"/> parameters on the object and is hardcoded to 
    /// display MESSAGE:StealthNeutralized when the object has been discovered.
    /// </summary>
    public sealed class StealthUpdateModuleData : BehaviorModuleData
    {
        internal static StealthUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StealthUpdateModuleData> FieldParseTable = new IniParseTable<StealthUpdateModuleData>
        {
            { "StealthDelay", (parser, x) => x.StealthDelay = parser.ParseInteger() },
            { "StealthForbiddenConditions", (parser, x) => x.StealthForbiddenConditions = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "HintDetectableConditions", (parser, x) => x.HintDetectableConditions = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "FriendlyOpacityMin", (parser, x) => x.FriendlyOpacityMin = parser.ParsePercentage() },
            { "FriendlyOpacityMax", (parser, x) => x.FriendlyOpacityMax = parser.ParsePercentage() },
            { "PulseFrequency", (parser, x) => x.PulseFrequency = parser.ParseInteger() },
            { "MoveThresholdSpeed", (parser, x) => x.MoveThresholdSpeed = parser.ParseInteger() },
            { "InnateStealth", (parser, x) => x.InnateStealth = parser.ParseBoolean() },
            { "OrderIdleEnemiesToAttackMeUponReveal", (parser, x) => x.OrderIdleEnemiesToAttackMeUponReveal = parser.ParseBoolean() },
            { "DisguisesAsTeam", (parser, x) => x.DisguisesAsTeam = parser.ParseBoolean() },
            { "RevealDistanceFromTarget", (parser, x) => x.RevealDistanceFromTarget = parser.ParseFloat() },
            { "DisguiseFX", (parser, x) => x.DisguiseFX = parser.ParseAssetReference() },
            { "DisguiseRevealFX", (parser, x) => x.DisguiseRevealFX = parser.ParseAssetReference() },
            { "DisguiseTransitionTime", (parser, x) => x.DisguiseTransitionTime = parser.ParseInteger() },
            { "DisguiseRevealTransitionTime", (parser, x) => x.DisguiseRevealTransitionTime = parser.ParseInteger() },
            { "GrantedBySpecialPower", (parser, x) => x.GrantedBySpecialPower = parser.ParseBoolean() },
            { "EnemyDetectionEvaEvent", (parser, x) => x.EnemyDetectionEvaEvent = parser.ParseAssetReference() },
            { "OwnDetectionEvaEvent", (parser, x) => x.OwnDetectionEvaEvent = parser.ParseAssetReference() },
            { "UseRiderStealth", (parser, x) => x.UseRiderStealth = parser.ParseBoolean() },
            { "DetectedByAnyoneRange", (parser, x) => x.DetectedByAnyoneRange = parser.ParseFloat() },
            { "RemoveTerrainRestrictionOnUpgrade", (parser, x) => x.RemoveTerrainRestrictionOnUpgrade = parser.ParseString() },
            { "RevealWeaponSets", (parser, x) => x.RevealWeaponSets = parser.ParseEnumFlags<WeaponSetConditions>() },
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
            { "DetectedByFriendliesOnly", (parser, x) => x.DetectedByFriendliesOnly = parser.ParseBoolean() },
            { "VoiceMoveToStealthyArea", (parser, x) => x.VoiceMoveToStealthyArea = parser.ParseAssetReference() },
            { "VoiceEnterStateMoveToStealthyArea", (parser, x) => x.VoiceEnterStateMoveToStealthyArea = parser.ParseAssetReference() },
            { "OneRingDelayOn", (parser, x) => x.OneRingDelayOn = parser.ParseInteger() },
            { "OneRingDelayOff", (parser, x) => x.OneRingDelayOff = parser.ParseInteger() },
            { "RingAnimTimeOn", (parser, x) => x.RingAnimTimeOn = parser.ParseInteger() },
            { "RingAnimTimeOff", (parser, x) => x.RingAnimTimeOff = parser.ParseInteger() },
            { "RingDelayAfterRemoving", (parser, x) => x.RingDelayAfterRemoving = parser.ParseInteger() },

            { "BecomeStealthedFX", (parser, x) => x.BecomeStealthedFX = parser.ParseAssetReference() },
            { "ExitStealthFX", (parser, x) => x.ExitStealthFX = parser.ParseAssetReference() },
            { "BecomeStealthedOneRingFX", (parser, x) => x.BecomeStealthedOneRingFX = parser.ParseAssetReference() },
            { "ExitStealthOneRingFX", (parser, x) => x.ExitStealthOneRingFX = parser.ParseAssetReference() },
             { "RequiredUpgradeNames", (parser, x) => x.RequiredUpgradeNames = parser.ParseAssetReferenceArray() },
        };

        public int StealthDelay { get; private set; }
        public BitArray<ModelConditionFlag> StealthForbiddenConditions { get; private set; }
        public BitArray<ModelConditionFlag> HintDetectableConditions { get; private set; }
        public Percentage FriendlyOpacityMin { get; private set; }
        public Percentage FriendlyOpacityMax { get; private set; }
        public int PulseFrequency { get; private set; }
        public int MoveThresholdSpeed { get; private set; }
        public bool InnateStealth { get; private set; }
        public bool OrderIdleEnemiesToAttackMeUponReveal { get; private set; }
        public bool DisguisesAsTeam { get; private set; }
        public float RevealDistanceFromTarget { get; private set; }
        public string DisguiseFX { get; private set; }
        public string DisguiseRevealFX { get; private set; }
        public int DisguiseTransitionTime { get; private set; }
        public int DisguiseRevealTransitionTime { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool GrantedBySpecialPower { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string EnemyDetectionEvaEvent { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string OwnDetectionEvaEvent { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool UseRiderStealth { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float DetectedByAnyoneRange { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string RemoveTerrainRestrictionOnUpgrade { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public WeaponSetConditions RevealWeaponSets { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool StartsActive { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool DetectedByFriendliesOnly { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string VoiceMoveToStealthyArea { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string VoiceEnterStateMoveToStealthyArea { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int OneRingDelayOn { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int OneRingDelayOff { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int RingAnimTimeOn { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int RingAnimTimeOff { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int RingDelayAfterRemoving { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string BecomeStealthedFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string ExitStealthFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string BecomeStealthedOneRingFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string ExitStealthOneRingFX { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string[] RequiredUpgradeNames { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new StealthUpdate();
        }
    }
}

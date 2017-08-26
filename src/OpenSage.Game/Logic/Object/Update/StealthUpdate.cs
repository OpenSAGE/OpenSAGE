using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the use of the <see cref="ObjectDefinition.SoundStealthOn"/> and 
    /// <see cref="ObjectDefinition.SoundStealthOff"/> parameters on the object and is hardcoded to 
    /// display MESSAGE:StealthNeutralized when the object has been discovered.
    /// </summary>
    public sealed class StealthUpdate : ObjectBehavior
    {
        internal static StealthUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StealthUpdate> FieldParseTable = new IniParseTable<StealthUpdate>
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
        };

        public int StealthDelay { get; private set; }
        public BitArray<ModelConditionFlag> StealthForbiddenConditions { get; private set; }
        public BitArray<ModelConditionFlag> HintDetectableConditions { get; private set; }
        public float FriendlyOpacityMin { get; private set; }
        public float FriendlyOpacityMax { get; private set; }
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
    }
}

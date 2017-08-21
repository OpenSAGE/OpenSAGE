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
            { "OrderIdleEnemiesToAttackMeUponReveal", (parser, x) => x.OrderIdleEnemiesToAttackMeUponReveal = parser.ParseBoolean() }
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
    }
}

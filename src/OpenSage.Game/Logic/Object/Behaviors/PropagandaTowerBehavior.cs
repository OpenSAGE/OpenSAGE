using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class PropagandaTowerBehavior : ObjectBehavior
    {
        internal static PropagandaTowerBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PropagandaTowerBehavior> FieldParseTable = new IniParseTable<PropagandaTowerBehavior>
        {
            { "Radius", (parser, x) => x.Radius = parser.ParseFloat() },
            { "DelayBetweenUpdates", (parser, x) => x.DelayBetweenUpdates = parser.ParseInteger() },
            { "HealPercentEachSecond", (parser, x) => x.HealPercentEachSecond = parser.ParsePercentage() },
            { "PulseFX", (parser, x) => x.PulseFX = parser.ParseAssetReference() },
            { "UpgradeRequired", (parser, x) => x.UpgradeRequired = parser.ParseAssetReference() },
            { "UpgradedHealPercentEachSecond", (parser, x) => x.UpgradedHealPercentEachSecond = parser.ParsePercentage() },
            { "UpgradedPulseFX", (parser, x) => x.UpgradedPulseFX = parser.ParseAssetReference() }
        };

        public float Radius { get; private set; }
        public int DelayBetweenUpdates { get; private set; }
        public float HealPercentEachSecond { get; private set; }
        public string PulseFX { get; private set; }
        public string UpgradeRequired { get; private set; }
        public float UpgradedHealPercentEachSecond { get; private set; }
        public string UpgradedPulseFX { get; private set; }
    }
}

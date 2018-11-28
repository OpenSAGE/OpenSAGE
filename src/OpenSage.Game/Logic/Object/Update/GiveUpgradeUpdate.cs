using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class GiveUpgradeUpdateModuleData : UpdateModuleData
    {
        internal static GiveUpgradeUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GiveUpgradeUpdateModuleData> FieldParseTable = new IniParseTable<GiveUpgradeUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseFloat() },
            { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
            { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
            { "PersistentPrepTime", (parser, x) => x.PersistentPrepTime = parser.ParseInteger() },
            { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
            { "ApproachRequiresLOS", (parser, x) => x.ApproachRequiresLos = parser.ParseBoolean() },
            { "SpawnOutFX", (parser, x) => x.SpawnOutFX = parser.ParseAssetReference() },
            { "DeliverUpgrade", (parser, x) => x.DeliverUpgrade = parser.ParseBoolean() },
            { "FadeOutSpeed", (parser, x) => x.FadeOutSpeed = parser.ParseFloat() },
        };

        public string SpecialPowerTemplate { get; private set; }
        public float StartAbilityRange { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int PersistentPrepTime { get; private set; }
        public int PackTime { get; private set; }
        public bool ApproachRequiresLos { get; private set; }
        public string SpawnOutFX { get; private set; }
        public bool DeliverUpgrade { get; private set; }
        public float FadeOutSpeed { get; private set; }

    }
}

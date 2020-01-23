using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class RousingSpeechUpdateModuleData : UpdateModuleData
    {
        internal static RousingSpeechUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RousingSpeechUpdateModuleData> FieldParseTable = new IniParseTable<RousingSpeechUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "RequiredConditions", (parser, x) => x.RequiredConditions = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseFloat() },
            { "UpdateInterval", (parser, x) => x.UpdateInterval = parser.ParseInteger() },
            { "ApproachRequiresLOS", (parser, x) => x.ApproachRequiresLos = parser.ParseBoolean() },
            { "BonusRadius", (parser, x) => x.BonusRadius = parser.ParseInteger() },
            { "SpeechDuration", (parser, x) => x.SpeechDuration = parser.ParseInteger() },
            { "LeaderFX", (parser, x) => x.LeaderFX = parser.ParseAssetReference() },
            { "FollowerFX", (parser, x) => x.FollowerFX = parser.ParseAssetReference() },
            { "CreateWave", (parser, x) => x.CreateWave = parser.ParseBoolean() },
            { "WaveWidth", (parser, x) => x.WaveWidth = parser.ParseInteger() },
            { "ModifierName", (parser, x) => x.ModifierName = parser.ParseIdentifier() },
            { "ObjectFilter", (parser, x) => x.ObjectFilter = ObjectFilter.Parse(parser) }
        };
        
        public string SpecialPowerTemplate { get; private set; }
        public BitArray<ModelConditionFlag> RequiredConditions { get; private set; }
        public float StartAbilityRange { get; private set; }
        public int UpdateInterval { get; private set; }
        public bool ApproachRequiresLos { get; private set; }
        public int BonusRadius { get; private set; }
        public int SpeechDuration { get; private set; }
        public string LeaderFX { get; private set; }
        public string FollowerFX { get; private set; }
        public bool CreateWave { get; private set; }
        public int WaveWidth { get; private set; }
        public string ModifierName { get; private set; }
        public ObjectFilter ObjectFilter { get; private set; }
    }
}

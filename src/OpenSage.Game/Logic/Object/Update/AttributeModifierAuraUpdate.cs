using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AttributeModifierAuraUpdateModuleData : UpdateModuleData
    {
        internal static AttributeModifierAuraUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AttributeModifierAuraUpdateModuleData> FieldParseTable = new IniParseTable<AttributeModifierAuraUpdateModuleData>
        {
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
            { "BonusName", (parser, x) => x.BonusName = parser.ParseString() },
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() },
            { "RefreshDelay", (parser, x) => x.RefreshDelay = parser.ParseLong() },
            { "Range", (parser, x) => x.Range = parser.ParseFloat() },
            { "TargetEnemy", (parser, x) => x.TargetEnemy = parser.ParseBoolean() },
            { "ObjectFilter", (parser, x) => x.ObjectFilter = parser.ParseIdentifier() },
            { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseAssetReferenceArray() },
            { "RunWhileDead", (parser, x) => x.RunWhileDead = parser.ParseBoolean() },
            { "RequiredConditions", (parser, x) => x.RequiredConditions = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "AntiCategory", (parser, x) => x.AntiCategory = parser.ParseEnum<ModifierCategory>() },
            { "AntiFX", (parser, x) => x.AntiFX = parser.ParseAssetReference() },
            { "AllowSelf", (parser, x) => x.AllowSelf = parser.ParseBoolean() },
            { "AllowPowerWhenAttacking", (parser, x) => x.AllowPowerWhenAttacking = parser.ParseBoolean() },
            { "MaxActiveRank", (parser, x) => x.MaxActiveRank = parser.ParseInteger() },
            { "AffectContainedOnly", (parser, x) => x.AffectContainedOnly = parser.ParseBoolean() },
            { "RequiresAllTriggers", (parser, x) => x.RequiresAllTriggers = parser.ParseBoolean() }
        };

        public bool StartsActive { get; private set; }
        public string BonusName	{ get; private set; }
        public string TriggeredBy { get; private set; }
        public long RefreshDelay { get; private set; }
        public float Range { get; private set; }
        public bool TargetEnemy	{ get; private set; }
        public string ObjectFilter { get; private set; }
        public string[] ConflictsWith { get; private set; }
        public bool RunWhileDead { get; private set; }
        public BitArray<ModelConditionFlag> RequiredConditions { get; private set; }
        public ModifierCategory AntiCategory { get; private set; }
        public string AntiFX { get; private set; }
        public bool AllowSelf { get; private set; }
        public bool AllowPowerWhenAttacking { get; private set; }
        public int MaxActiveRank { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool AffectContainedOnly { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool RequiresAllTriggers { get; private set; }
    }
}

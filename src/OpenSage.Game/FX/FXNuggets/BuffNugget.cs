using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.FX
{
    [AddedIn(SageGame.Bfme)]
    public sealed class BuffNugget : FXNugget
    {
        internal static BuffNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BuffNugget> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<BuffNugget>
        {
            { "BuffType", (parser, x) => x.BuffType = parser.ParseIdentifier() },
            { "BuffThingTemplate", (parser, x) => x.BuffThingTemplate = parser.ParseAssetReference() },
            { "BuffInfantryTemplate", (parser, x) => x.BuffInfantryTemplate = parser.ParseAssetReference() },
            { "BuffCavalryTemplate", (parser, x) => x.BuffCavalryTemplate = parser.ParseAssetReference() },
            { "BuffTrollTemplate", (parser, x) => x.BuffTrollTemplate = parser.ParseAssetReference() },
            { "BuffOrcTemplate", (parser, x) => x.BuffOrcTemplate = parser.ParseAssetReference() },
            { "IsComplexBuff", (parser, x) => x.IsComplexBuff = parser.ParseBoolean() },
            { "BuffLifeTime", (parser, x) => x.BuffLifeTime = parser.ParseLong() },
            { "Extrusion", (parser, x) => x.Extrusion = parser.ParseFloat() },
            { "Color", (parser, x) => x.Color = parser.ParseColorRgb() },
            { "BuffShipTemplate", (parser, x) => x.BuffShipTemplate = parser.ParseAssetReference() },
            { "BuffMonsterTemplate", (parser, x) => x.BuffMonsterTemplate = parser.ParseAssetReference() }
        });

        public string BuffType { get; private set; }
        public string BuffThingTemplate { get; private set; }
        public string BuffInfantryTemplate { get; private set; }
        public string BuffCavalryTemplate { get; private set; }
        public string BuffTrollTemplate { get; private set; }
        public string BuffOrcTemplate { get; private set; }
        public bool IsComplexBuff { get; private set; }
        public long BuffLifeTime { get; private set; }
        public float Extrusion { get; private set; }
        public ColorRgb Color { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string BuffShipTemplate { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string BuffMonsterTemplate { get; private set; }
    }
}

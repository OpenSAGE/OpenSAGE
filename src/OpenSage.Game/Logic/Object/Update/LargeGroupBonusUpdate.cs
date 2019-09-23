using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LargeGroupBonusUpdateModuleData : UpdateModuleData
    {
        internal static LargeGroupBonusUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LargeGroupBonusUpdateModuleData> FieldParseTable = new IniParseTable<LargeGroupBonusUpdateModuleData>
        {
           { "UpdateRate", (parser, x) => x.UpdateRate = parser.ParseInteger() },
           { "HordeMemberFilter", (parser, x) => x.HordeMemberFilter = ObjectFilter.Parse(parser) },
           { "Count", (parser, x) => x.Count = parser.ParseInteger() },
           { "Radius", (parser, x) => x.Radius = parser.ParseFloat() },
           { "RubOffRadius", (parser, x) => x.RubOffRadius = parser.ParseFloat() },
           { "AlliesOnly", (parser, x) => x.AlliesOnly = parser.ParseBoolean() },
           { "AttributeModifier", (parser, x) => x.AttributeModifier = parser.ParseAssetReference() }
        };

        public int UpdateRate { get; private set; }
        public ObjectFilter HordeMemberFilter { get; private set; }
        public int Count { get; private set; }
        public float Radius { get; private set; }
        public float RubOffRadius { get; private set; }
        public bool AlliesOnly { get; private set; }
        public string AttributeModifier { get; private set; }
    }
}

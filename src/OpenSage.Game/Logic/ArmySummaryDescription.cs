using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;

namespace OpenSage.Logic
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class ArmySummaryDescription  : BaseSingletonAsset
    {
        internal static void Parse(IniParser parser, ArmySummaryDescription value) => parser.ParseBlockContent(value, FieldParseTable);

        private static readonly IniParseTable<ArmySummaryDescription> FieldParseTable = new IniParseTable<ArmySummaryDescription>
        {
            { "HeroFilter", (parser, x) => x.HeroFilter = ObjectFilter.Parse(parser) },
            { "UnitCategory", (parser, x) => x.UnitCategories.Add(UnitCategory.Parse(parser)) }
        };

        public ObjectFilter HeroFilter { get; private set; }
        public List<UnitCategory> UnitCategories { get; } = new List<UnitCategory>();
    }

    public sealed class UnitCategory
    {
        internal static UnitCategory Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<UnitCategory> FieldParseTable = new IniParseTable<UnitCategory>
        {
            { "Name", (parser, x) => x.Name = parser.ParseLocalizedStringKey() },
            { "PluralName", (parser, x) => x.PluralName = parser.ParseLocalizedStringKey() },
            { "Filter", (parser, x) => x.Filter = ObjectFilter.Parse(parser) },
        };

        public string Name { get; private set; }
        public string PluralName { get; private set; }
        public ObjectFilter Filter { get; private set; }
    }
}

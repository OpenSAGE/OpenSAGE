using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;

namespace OpenSage.Logic
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class FormationAssistant
    {
        internal static FormationAssistant Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FormationAssistant> FieldParseTable = new IniParseTable<FormationAssistant>
        {
            { "DefaultPreviewObject", (parser, x) => x.DefaultPreviewObject = parser.ParseAssetReference() },
            { "RowPadding", (parser, x) => x.RowPadding = parser.ParseFloat() },
            { "ColumnPadding", (parser, x) => x.ColumnPadding = parser.ParseFloat() },
            { "FacingArrowHeadTemplate", (parser, x) => x.FacingArrowHeadTemplate = parser.ParseAssetReference() },
            { "FacingArrowBodyTemplate", (parser, x) => x.FacingArrowBodyTemplate = parser.ParseAssetReference() },
            { "FacingArrowBaseTemplate", (parser, x) => x.FacingArrowBaseTemplate = parser.ParseAssetReference() },
            { "ActivationDragDistance", (parser, x) => x.ActivationDragDistance = parser.ParseFloat() },
            { "ActivationTime", (parser, x) => x.ActivationTime = parser.ParseFloat() },
            { "ValidObjectFilter", (parser, x) => x.ValidObjectFilter = ObjectFilter.Parse(parser) },
            { "UnitDefinition", (parser, x) => x.UnitDefinitions.Add(UnitDefinition.Parse(parser)) },
            { "FormationTemplate", (parser, x) => x.FormationTemplates.Add(FormationTemplate.Parse(parser)) },
            { "FormationSelection", (parser, x) => x.FormationSelections.Add(FormationSelection.Parse(parser)) }
        };

        public string DefaultPreviewObject { get; private set; }
        public float RowPadding { get; private set; }
        public float ColumnPadding { get; private set; }
        public string FacingArrowHeadTemplate { get; private set; }
        public string FacingArrowBodyTemplate { get; private set; }
        public string FacingArrowBaseTemplate { get; private set; }
        public float ActivationDragDistance { get; private set; }
        public float ActivationTime { get; private set; }
        public ObjectFilter ValidObjectFilter { get; private set; }
        public List<UnitDefinition> UnitDefinitions { get; } = new List<UnitDefinition>();
        public List<FormationTemplate> FormationTemplates { get; } = new List<FormationTemplate>();
        public List<FormationSelection> FormationSelections { get; } = new List<FormationSelection>();
    }

    public class UnitDefinition
    {
        internal static UnitDefinition Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<UnitDefinition> FieldParseTable = new IniParseTable<UnitDefinition>
        {
            { "PreviewObject", (parser, x) => x.PreviewObject = parser.ParseAssetReference() },
            { "ObjectFilter", (parser, x) => x.ObjectFilter = ObjectFilter.Parse(parser) }
        };

        public string Name { get; private set; }

        public string PreviewObject { get; private set; }
        public ObjectFilter ObjectFilter { get; private set; }
    }

    public class FormationTemplate
    {
        internal static FormationTemplate Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<FormationTemplate> FieldParseTable = new IniParseTable<FormationTemplate>
        {
            { "Rows", (parser, x) => x.Rows = Rows.Parse(parser) }
        };

        public string Name { get; private set; }
        public Rows Rows { get; private set; }
    }

    public class Rows
    {
        internal static Rows Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<Rows> FieldParseTable = new IniParseTable<Rows>
        {
            { "Row", (parser, x) => x.RowDefinitions.Add(parser.ParseAssetReferenceArray()) }
        };

        public List<string[]> RowDefinitions { get; } = new List<string[]>();
    }

    public class FormationSelection
    {
        internal static FormationSelection Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<FormationSelection> FieldParseTable = new IniParseTable<FormationSelection>
        {
            { "MaxDragLength", (parser, x) => x.MaxDragLength = parser.ParseFloat() },
            { "MaxUnitsSelected", (parser, x) => x.MaxUnitsSelected = parser.ParseInteger() },
        };

        public string Name { get; private set; }

        public float MaxDragLength { get; private set; }
        public int MaxUnitsSelected { get; private set; }
    }
}

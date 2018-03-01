using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    public sealed class Credits
    {
        internal static Credits Parse(IniParser parser)
        {
            return parser.ParseTopLevelBlock(FieldParseTable);
        }

        private static readonly IniParseTable<Credits> FieldParseTable = new IniParseTable<Credits>
        {
            { "ScrollRate", (parser, x) => x.ScrollRate = parser.ParseInteger() },
            { "ScrollRateEveryFrames", (parser, x) => x.ScrollRateEveryFrames = parser.ParseInteger() },
            { "ScrollDown", (parser, x) => x.ScrollDown = parser.ParseBoolean() },
            { "TitleColor", (parser, x) => x.TitleColor = parser.ParseColorRgba() },
            { "MinorTitleColor", (parser, x) => x.MinorTitleColor = parser.ParseColorRgba() },
            { "NormalColor", (parser, x) => x.NormalColor = parser.ParseColorRgba() },

            { "Style", (parser, x) => x.Lines.Add(new CreditStyleLine(parser.ParseEnum<CreditStyle>())) },
            { "Text", (parser, x) => x.Lines.Add(new CreditTextLine(parser.ParseQuotedString())) },
            { "Blank", (parser, x) => x.Lines.Add(new CreditBlankLine()) }
        };

        public int ScrollRate { get; private set; }
        public int ScrollRateEveryFrames { get; private set; }
        public bool ScrollDown { get; private set; }
        public ColorRgba TitleColor { get; private set; }
        public ColorRgba MinorTitleColor { get; private set; }
        public ColorRgba NormalColor { get; private set; }

        public List<CreditLine> Lines { get; } = new List<CreditLine>();
    }

    public abstract class CreditLine
    {

    }

    public sealed class CreditStyleLine : CreditLine
    {
        public CreditStyle Style { get; }

        public CreditStyleLine(CreditStyle style)
        {
            Style = style;
        }
    }

    public sealed class CreditTextLine : CreditLine
    {
        public string Text { get; private set; }

        public CreditTextLine(string text)
        {
            Text = text;
        }
    }

    public sealed class CreditBlankLine : CreditLine
    {
        
    }

    public enum CreditStyle
    {
        [IniEnum("TITLE")]
        Title,

        [IniEnum("MINORTITLE")]
        MinorTitle,

        [IniEnum("NORMAL")]
        Normal,

        [IniEnum("COLUMN")]
        Column
    }
}

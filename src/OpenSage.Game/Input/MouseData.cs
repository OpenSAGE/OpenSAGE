﻿using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Input
{
    public sealed class MouseData : BaseSingletonAsset
    {
        internal static void Parse(IniParser parser, MouseData value) => parser.ParseBlockContent(value, FieldParseTable);

        private static readonly IniParseTable<MouseData> FieldParseTable = new IniParseTable<MouseData>
        {
            { "TooltipFontName", (parser, x) => x.TooltipFontName = parser.ParseString() },
            { "TooltipFontSize", (parser, x) => x.TooltipFontSize = parser.ParseInteger() },
            { "TooltipFontIsBold", (parser, x) => x.TooltipFontIsBold = parser.ParseBoolean() },
            { "TooltipAnimateBackground", (parser, x) => x.TooltipAnimateBackground = parser.ParseBoolean() },
            { "TooltipFillTime", (parser, x) => x.TooltipFillTime = parser.ParseInteger() },
            { "TooltipDelayTime", (parser, x) => x.TooltipDelayTime = parser.ParseInteger() },
            { "TooltipTextColor", (parser, x) => x.TooltipTextColor = parser.ParseColorRgba() },
            { "TooltipHighlightColor", (parser, x) => x.TooltipHighlightColor = parser.ParseColorRgba() },
            { "TooltipShadowColor", (parser, x) => x.TooltipShadowColor = parser.ParseColorRgba() },
            { "TooltipBorderColor", (parser, x) => x.TooltipBorderColor = parser.ParseColorRgba() },
            { "TooltipBackgroundColor", (parser, x) => x.TooltipBackgroundColor = parser.ParseColorRgba() },
            { "TooltipWidth", (parser, x) => x.TooltipWidth = parser.ParseInteger() },
            { "UseTooltipAltTextColor", (parser, x) => x.UseTooltipAltTextColor = parser.ParseBoolean() },
            { "UseTooltipAltBackColor", (parser, x) => x.UseTooltipAltBackColor = parser.ParseBoolean() },
            { "AdjustTooltipAltColor", (parser, x) => x.AdjustTooltipAltColor = parser.ParseBoolean() },

            { "OrthoCamera", (parser, x) => x.OrthoCamera = parser.ParseBoolean() },
            { "OrthoZoom", (parser, x) => x.OrthoZoom = parser.ParseFloat() },

            { "DragTolerance", (parser, x) => x.DragTolerance = parser.ParseInteger() },
            { "DragTolerance3D", (parser, x) => x.DragTolerance3D = parser.ParseInteger() },
            { "DragToleranceMS", (parser, x) => x.DragToleranceMS = parser.ParseInteger() }
        };

        public string TooltipFontName { get; private set; }
        public int TooltipFontSize { get; private set; }
        public bool TooltipFontIsBold { get; private set; }
        public bool TooltipAnimateBackground { get; private set; }
        public int TooltipFillTime { get; private set; }
        public int TooltipDelayTime { get; private set; }
        public ColorRgba TooltipTextColor { get; private set; }
        public ColorRgba TooltipHighlightColor { get; private set; }
        public ColorRgba TooltipShadowColor { get; private set; }
        public ColorRgba TooltipBorderColor { get; private set; }
        public ColorRgba TooltipBackgroundColor { get; private set; }
        public int TooltipWidth { get; private set; }
        public bool UseTooltipAltTextColor { get; private set; }
        public bool UseTooltipAltBackColor { get; private set; }
        public bool AdjustTooltipAltColor { get; private set; }

        public bool OrthoCamera { get; private set; }
        public float OrthoZoom { get; private set; }

        public int DragTolerance { get; private set; }
        public int DragTolerance3D { get; private set; }
        public int DragToleranceMS { get; private set; }
    }
}

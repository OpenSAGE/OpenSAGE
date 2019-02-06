using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class LivingWorldRegionEffects
    {
        internal static LivingWorldRegionEffects Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldRegionEffects> FieldParseTable = new IniParseTable<LivingWorldRegionEffects>
        {
            { "RegionObject", (parser, x) => x.RegionObject = parser.ParseAssetReference() },
            { "NeutralRegionColor", (parser, x) => x.NeutralRegionColor = parser.ParseColorRgb() },
            { "RegionBorderColor", (parser, x) => x.RegionBorderColor = parser.ParseColorRgb() },
            { "ShellStartPositionColor", (parser, x) => x.ShellStartPositionColor = parser.ParseColorRgb() },
            { "BordersEffect", (parser, x) => x.BordersEffect = RegionEffect.Parse(parser) },
            { "FilledOwnershipEffect", (parser, x) => x.FilledOwnershipEffect = RegionEffect.Parse(parser) },
            { "MouseoverEffectFlareup", (parser, x) => x.MouseoverEffectFlareup = RegionEffect.Parse(parser) },
            { "HomeRegionHighlight", (parser, x) => x.HomeRegionHighlight = RegionEffect.Parse(parser) },
            { "RegionSelectionEffect", (parser, x) => x.RegionSelectionEffect = RegionEffect.Parse(parser) },
            { "UnifiedEffect", (parser, x) => x.UnifiedEffect = RegionEffect.Parse(parser) }
        };

        public string Name { get; private set; }

        public string RegionObject { get; private set; }
        public ColorRgb NeutralRegionColor { get; private set; }
        public ColorRgb RegionBorderColor { get; private set; }
        public ColorRgb ShellStartPositionColor { get; private set; }
        public RegionEffect BordersEffect { get; private set; }
        public RegionEffect FilledOwnershipEffect { get; private set; }
        public RegionEffect MouseoverEffectFlareup { get; private set; }
        public RegionEffect HomeRegionHighlight { get; private set; }
        public RegionEffect RegionSelectionEffect { get; private set; }
        public RegionEffect UnifiedEffect { get; private set; }
    }

    public class RegionEffect
    {
        internal static RegionEffect Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<RegionEffect> FieldParseTable = new IniParseTable<RegionEffect>
        {
            { "Geometry", (parser, x) => x.Geometries.Add(parser.ParseAssetReference()) },
            { "LoadInShell", (parser, x) => x.LoadInShell = parser.ParseBoolean() },
            { "ColorIntensityControlPoint", (parser, x) => x.ColorIntensityControlPoints.Add(ColorIntensityControlPoint.Parse(parser)) }
        };

        public List<string> Geometries { get; } = new List<string>();
        public bool LoadInShell { get; private set; }
        public List<ColorIntensityControlPoint> ColorIntensityControlPoints { get; } = new List<ColorIntensityControlPoint>();
    }

    public class ColorIntensityControlPoint
    {
        internal static ColorIntensityControlPoint Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<ColorIntensityControlPoint> FieldParseTable = new IniParseTable<ColorIntensityControlPoint>
        {
            { "Intensity", (parser, x) => x.Intensity = parser.ParseFloat() },
            { "Time", (parser, x) => x.Time = parser.ParseFloat() }
        };

        public float Intensity { get; private set; }
        public float Time { get; private set; }
    }
}

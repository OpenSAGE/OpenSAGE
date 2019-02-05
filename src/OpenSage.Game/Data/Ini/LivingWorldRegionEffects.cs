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
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldRegionEffects> FieldParseTable = new IniParseTable<LivingWorldRegionEffects>
        {
            { "RegionObject", (parser, x) => x.RegionObject = parser.ParseAssetReference() },
            { "NeutralRegionColor", (parser, x) => x.NeutralRegionColor = parser.ParseColorRgb() },
            { "RegionBorderColor", (parser, x) => x.RegionBorderColor = parser.ParseColorRgb() },
            { "ShellStartPositionColor", (parser, x) => x.ShellStartPositionColor = parser.ParseColorRgb() },
            { "BordersEffect", (parser, x) => x.BordersEffect = BordersEffect.Parse(parser) },
            { "FilledOwnershipEffect", (parser, x) => x.FilledOwnershipEffect = FilledOwnershipEffect.Parse(parser) },
            { "MouseoverEffectFlareup", (parser, x) => x.MouseoverEffectFlareup = MouseoverEffectFlareup.Parse(parser) },
            { "HomeRegionHighlight", (parser, x) => x.HomeRegionHighlight = HomeRegionHighlight.Parse(parser) },
            { "RegionSelectionEffect", (parser, x) => x.RegionSelectionEffect = RegionSelectionEffect.Parse(parser) },
            { "UnifiedEffect", (parser, x) => x.UnifiedEffect = UnifiedEffect.Parse(parser) }
        };

        public string Name { get; private set; }

        public string RegionObject { get; private set; }
        public ColorRgb NeutralRegionColor { get; private set; }
        public ColorRgb RegionBorderColor { get; private set; }
        public ColorRgb ShellStartPositionColor { get; private set; }
        public BordersEffect BordersEffect { get; private set; }
        public FilledOwnershipEffect FilledOwnershipEffect { get; private set; }
        public MouseoverEffectFlareup MouseoverEffectFlareup { get; private set; }
        public HomeRegionHighlight HomeRegionHighlight { get; private set; }
        public RegionSelectionEffect RegionSelectionEffect { get; private set; }
        public UnifiedEffect UnifiedEffect { get; private set; }
    }

    public abstract class RegionEffect
    {
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

    public class BordersEffect : RegionEffect
    {
        internal static BordersEffect Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<BordersEffect> FieldParseTable = RegionEffect.FieldParseTable
            .Concat(new IniParseTable<BordersEffect>());
    }

    public class FilledOwnershipEffect : RegionEffect
    {
        internal static FilledOwnershipEffect Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<FilledOwnershipEffect> FieldParseTable = RegionEffect.FieldParseTable
            .Concat(new IniParseTable<FilledOwnershipEffect>());
    }

    public class MouseoverEffectFlareup : RegionEffect
    {
        internal static MouseoverEffectFlareup Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<MouseoverEffectFlareup> FieldParseTable = RegionEffect.FieldParseTable
            .Concat(new IniParseTable<MouseoverEffectFlareup>());
    }

    public class HomeRegionHighlight : RegionEffect
    {
        internal static HomeRegionHighlight Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<HomeRegionHighlight> FieldParseTable = RegionEffect.FieldParseTable
            .Concat(new IniParseTable<HomeRegionHighlight>());
    }

    public class RegionSelectionEffect : RegionEffect
    {
        internal static RegionSelectionEffect Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<RegionSelectionEffect> FieldParseTable = RegionEffect.FieldParseTable
            .Concat(new IniParseTable<RegionSelectionEffect>());
    }

    public class UnifiedEffect : RegionEffect
    {
        internal static UnifiedEffect Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<UnifiedEffect> FieldParseTable = RegionEffect.FieldParseTable
            .Concat(new IniParseTable<UnifiedEffect>());
    }
}

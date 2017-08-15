using System.Collections.Generic;
using OpenZH.Data.Ini.Parser;
using OpenZH.Data.Wnd;

namespace OpenZH.Data.Ini
{
    public sealed class InGameUI
    {
        internal static InGameUI Parse(IniParser parser)
        {
            return parser.ParseTopLevelBlock(FieldParseTable);
        }

        private static readonly IniParseTable<InGameUI> FieldParseTable = new IniParseTable<InGameUI>
        {
            { "MaxSelectionSize", (parser, x) => x.MaxSelectionSize = parser.ParseInteger() },

            { "MessageColor1", (parser, x) => x.MessageColor1 = IniColorRgb.Parse(parser) },
            { "MessageColor2", (parser, x) => x.MessageColor2 = IniColorRgb.Parse(parser) },
            { "MessagePosition", (parser, x) => x.MessagePosition = Coord2D.Parse(parser) },
            { "MessageFont", (parser, x) => x.MessageFont = parser.ParseString(allowWhitespace: true) },
            { "MessagePointSize", (parser, x) => x.MessagePointSize = parser.ParseInteger() },
            { "MessageBold", (parser, x) => x.MessageBold = parser.ParseBoolean() },
            { "MessageDelayMS", (parser, x) => x.MessageDelayMS = parser.ParseInteger() },

            { "MilitaryCaptionColor", (parser, x) => x.MilitaryCaptionColor = WndColor.Parse(parser) },
            { "MilitaryCaptionPosition", (parser, x) => x.MilitaryCaptionPosition = Coord2D.Parse(parser) },
            { "MilitaryCaptionTitleFont", (parser, x) => x.MilitaryCaptionTitleFont = parser.ParseString(allowWhitespace: true) },
            { "MilitaryCaptionTitlePointSize", (parser, x) => x.MilitaryCaptionTitlePointSize = parser.ParseInteger() },
            { "MilitaryCaptionTitleBold", (parser, x) => x.MilitaryCaptionTitleBold = parser.ParseBoolean() },

            { "MilitaryCaptionFont", (parser, x) => x.MilitaryCaptionFont = parser.ParseString(allowWhitespace: true) },
            { "MilitaryCaptionPointSize", (parser, x) => x.MilitaryCaptionPointSize = parser.ParseInteger() },
            { "MilitaryCaptionBold", (parser, x) => x.MilitaryCaptionBold = parser.ParseBoolean() },

            { "MilitaryCaptionRandomizeTyping", (parser, x) => x.MilitaryCaptionRandomizeTyping = parser.ParseBoolean() },
            { "MilitaryCaptionDelayMS", (parser, x) => x.MilitaryCaptionDelayMS = parser.ParseInteger() },

            { "SuperweaponCountdownPosition", (parser, x) => x.SuperweaponCountdownPosition = Coord2D.Parse(parser) },
            { "SuperweaponCountdownFlashDuration", (parser, x) => x.SuperweaponCountdownFlashDuration = parser.ParseInteger() },
            { "SuperweaponCountdownFlashColor", (parser, x) => x.SuperweaponCountdownFlashColor = IniColorRgb.Parse(parser) },

            { "SuperweaponCountdownNormalFont", (parser, x) => x.SuperweaponCountdownNormalFont = parser.ParseString(allowWhitespace: true) },
            { "SuperweaponCountdownNormalPointSize", (parser, x) => x.SuperweaponCountdownNormalPointSize = parser.ParseInteger() },
            { "SuperweaponCountdownNormalBold", (parser, x) => x.SuperweaponCountdownNormalBold = parser.ParseBoolean() },

            { "SuperweaponCountdownReadyFont", (parser, x) => x.SuperweaponCountdownReadyFont = parser.ParseString(allowWhitespace: true) },
            { "SuperweaponCountdownReadyPointSize", (parser, x) => x.SuperweaponCountdownReadyPointSize = parser.ParseInteger() },
            { "SuperweaponCountdownReadyBold", (parser, x) => x.SuperweaponCountdownReadyBold = parser.ParseBoolean() },

            { "NamedTimerCountdownPosition", (parser, x) => x.NamedTimerCountdownPosition = Coord2D.Parse(parser) },
            { "NamedTimerCountdownFlashDuration", (parser, x) => x.NamedTimerCountdownFlashDuration = parser.ParseInteger() },
            { "NamedTimerCountdownFlashColor", (parser, x) => x.NamedTimerCountdownFlashColor = IniColorRgb.Parse(parser) },

            { "NamedTimerCountdownNormalFont", (parser, x) => x.NamedTimerCountdownNormalFont = parser.ParseString(allowWhitespace: true) },
            { "NamedTimerCountdownNormalPointSize", (parser, x) => x.NamedTimerCountdownNormalPointSize = parser.ParseInteger() },
            { "NamedTimerCountdownNormalBold", (parser, x) => x.NamedTimerCountdownNormalBold = parser.ParseBoolean() },
            { "NamedTimerCountdownNormalColor", (parser, x) => x.NamedTimerCountdownNormalColor = IniColorRgb.Parse(parser) },

            { "NamedTimerCountdownReadyFont", (parser, x) => x.NamedTimerCountdownReadyFont = parser.ParseString(allowWhitespace: true) },
            { "NamedTimerCountdownReadyPointSize", (parser, x) => x.NamedTimerCountdownReadyPointSize = parser.ParseInteger() },
            { "NamedTimerCountdownReadyBold", (parser, x) => x.NamedTimerCountdownReadyBold = parser.ParseBoolean() },
            { "NamedTimerCountdownReadyColor", (parser, x) => x.NamedTimerCountdownReadyColor = IniColorRgb.Parse(parser) },

            { "FloatingTextTimeOut", (parser, x) => x.FloatingTextTimeOut = parser.ParseInteger() },
            { "FloatingTextMoveUpSpeed", (parser, x) => x.FloatingTextMoveUpSpeed = parser.ParseInteger() },
            { "FloatingTextVanishRate", (parser, x) => x.FloatingTextVanishRate = parser.ParseInteger() },

            { "DrawRMBScrollAnchor", (parser, x) => x.DrawRmbScrollAnchor = parser.ParseBoolean() },
            { "MoveRMBScrollAnchor", (parser, x) => x.MoveRmbScrollAnchor = parser.ParseBoolean() },
            { "PopupMessageColor", (parser, x) => x.PopupMessageColor = WndColor.Parse(parser) },

            { "SpyDroneRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.SpyDrone)) },
            { "AttackScatterAreaRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.AttackScatterArea)) },
            { "SuperweaponScatterAreaRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.SuperweaponScatterAreaRadiusCursor)) },
            { "AttackDamageAreaRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.AttackDamageArea)) },
            { "AttackContinueAreaRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.AttackContinueArea)) },
            { "GuardAreaRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.GuardArea)) },
            { "EmergencyRepairRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.EmergencyRepair)) },
            { "FriendlySpecialPowerRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.FriendlySpecialPower)) },
            { "OffensiveSpecialPowerRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.OffensiveSpecialPower)) },
            { "ParticleCannonRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.ParticleCannon)) },
            { "A10StrikeRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.A10Strike)) },
            { "CarpetBombRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.CarpetBomb)) },
            { "DaisyCutterRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.DaisyCutter)) },
            { "ParadropRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Paradrop)) },
            { "SpySatelliteRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.SpySatellite)) },
            { "NuclearMissileRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.NuclearMissile)) },
            { "EMPPulseRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.EmpPulse)) },
            { "ArtilleryRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.ArtilleryBarrage)) },
            { "NapalmStrikeRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.NapalmStrike)) },
            { "ClusterMinesRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.ClusterMines)) },
            { "ScudStormRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.ScudStorm)) },
            { "AnthraxBombRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.AnthraxBomb)) },
            { "AmbushRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Ambush)) },
            { "RadarRadiusCursor", (parser, x) => x.RadiusCursors.Add(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Radar)) }
        };

        public int MaxSelectionSize { get; private set; }

        public IniColorRgb MessageColor1 { get; private set; }
        public IniColorRgb MessageColor2 { get; private set; }
        public Coord2D MessagePosition { get; private set; }
        public string MessageFont { get; private set; }
        public int MessagePointSize { get; private set; }
        public bool MessageBold { get; private set; }
        public int MessageDelayMS { get; private set; }

        public WndColor MilitaryCaptionColor { get; private set; }
        public Coord2D MilitaryCaptionPosition { get; private set; }
        public string MilitaryCaptionTitleFont { get; private set; }
        public int MilitaryCaptionTitlePointSize { get; private set; }
        public bool MilitaryCaptionTitleBold { get; private set; }

        public string MilitaryCaptionFont { get; private set; }
        public int MilitaryCaptionPointSize { get; private set; }
        public bool MilitaryCaptionBold { get; private set; }

        public bool MilitaryCaptionRandomizeTyping { get; private set; }
        public int MilitaryCaptionDelayMS { get; private set; }

        public Coord2D SuperweaponCountdownPosition { get; private set; }
        public int SuperweaponCountdownFlashDuration { get; private set; }
        public IniColorRgb SuperweaponCountdownFlashColor { get; private set; }

        public string SuperweaponCountdownNormalFont { get; private set; }
        public int SuperweaponCountdownNormalPointSize { get; private set; }
        public bool SuperweaponCountdownNormalBold { get; private set; }

        public string SuperweaponCountdownReadyFont { get; private set; }
        public int SuperweaponCountdownReadyPointSize { get; private set; }
        public bool SuperweaponCountdownReadyBold { get; private set; }

        public Coord2D NamedTimerCountdownPosition { get; private set; }
        public int NamedTimerCountdownFlashDuration { get; private set; }
        public IniColorRgb NamedTimerCountdownFlashColor { get; private set; }

        public string NamedTimerCountdownNormalFont { get; private set; }
        public int NamedTimerCountdownNormalPointSize { get; private set; }
        public bool NamedTimerCountdownNormalBold { get; private set; }
        public IniColorRgb NamedTimerCountdownNormalColor { get; private set; }

        public string NamedTimerCountdownReadyFont { get; private set; }
        public int NamedTimerCountdownReadyPointSize { get; private set; }
        public bool NamedTimerCountdownReadyBold { get; private set; }
        public IniColorRgb NamedTimerCountdownReadyColor { get; private set; }

        public int FloatingTextTimeOut { get; private set; }
        public int FloatingTextMoveUpSpeed { get; private set; }
        public int FloatingTextVanishRate { get; private set; }

        public bool DrawRmbScrollAnchor { get; private set; }
        public bool MoveRmbScrollAnchor { get; private set; }
        public WndColor PopupMessageColor { get; private set; }

        public List<RadiusCursor> RadiusCursors { get; } = new List<RadiusCursor>();
    }

    public sealed class RadiusCursor
    {
        internal static RadiusCursor Parse(IniParser parser, CommandButtonRadiusCursorType type)
        {
            return new RadiusCursor
            {
                Type = type,
                DecalTemplate = RadiusDecalTemplate.Parse(parser)
            };
        }

        public CommandButtonRadiusCursorType Type { get; private set; }

        public RadiusDecalTemplate DecalTemplate { get; private set; }
    }

    public sealed class RadiusDecalTemplate
    {
        internal static RadiusDecalTemplate Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<RadiusDecalTemplate> FieldParseTable = new IniParseTable<RadiusDecalTemplate>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseFileName() },
            { "Style", (parser, x) => x.Style = parser.ParseEnum<RadiusDecalStyle>() },
            { "OpacityMin", (parser, x) => x.OpacityMin = parser.ParsePercentage() },
            { "OpacityMax", (parser, x) => x.OpacityMax = parser.ParsePercentage() },
            { "OpacityThrobTime", (parser, x) => x.OpacityThrobTime = parser.ParseInteger() },
            { "Color", (parser, x) => x.Color = WndColor.Parse(parser) },
            { "OnlyVisibleToOwningPlayer", (parser, x) => x.OnlyVisibleToOwningPlayer = parser.ParseBoolean() }
        };

        public string Texture { get; private set; }
        public RadiusDecalStyle Style { get; private set; }
        public float OpacityMin { get; private set; }
        public float OpacityMax { get; private set; }
        public int OpacityThrobTime { get; private set; }
        public WndColor Color { get; private set; }
        public bool OnlyVisibleToOwningPlayer { get; private set; }
    }

    public enum RadiusDecalStyle
    {
        [IniEnum("SHADOW_ALPHA_DECAL")]
        ShadowAlphaDecal
    }
}

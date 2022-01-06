using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Gui.ControlBar;
using OpenSage.Mathematics;

namespace OpenSage.Gui.InGame
{
    public sealed class InGameUI : BaseSingletonAsset, IPersistableObject
    {
        internal static void Parse(IniParser parser, InGameUI value) => parser.ParseBlockContent(value, FieldParseTable);

        private static readonly IniParseTable<InGameUI> FieldParseTable = new IniParseTable<InGameUI>
        {
            { "MaxSelectionSize", (parser, x) => x.MaxSelectionSize = parser.ParseInteger() },

            { "MessageColor1", (parser, x) => x.MessageColor1 = parser.ParseColorRgb() },
            { "MessageColor2", (parser, x) => x.MessageColor2 = parser.ParseColorRgb() },
            { "MessagePosition", (parser, x) => x.MessagePosition = parser.ParseVector2() },
            { "MessageFont", (parser, x) => x.MessageFont = parser.ParseString() },
            { "MessagePointSize", (parser, x) => x.MessagePointSize = parser.ParseInteger() },
            { "MessageBold", (parser, x) => x.MessageBold = parser.ParseBoolean() },
            { "MessageDelayMS", (parser, x) => x.MessageDelayMS = parser.ParseInteger() },

            { "MilitaryCaptionColor", (parser, x) => x.MilitaryCaptionColor = parser.ParseColorRgba() },
            { "MilitaryCaptionPosition", (parser, x) => x.MilitaryCaptionPosition = parser.ParseVector2() },
            { "MilitaryCaptionCentered", (parser, x) => x.MilitaryCaptionCentered = parser.ParseBoolean() },
            { "MilitaryCaptionTitleFont", (parser, x) => x.MilitaryCaptionTitleFont = parser.ParseString() },
            { "MilitaryCaptionTitlePointSize", (parser, x) => x.MilitaryCaptionTitlePointSize = parser.ParseInteger() },
            { "MilitaryCaptionTitleBold", (parser, x) => x.MilitaryCaptionTitleBold = parser.ParseBoolean() },

            { "MilitaryCaptionFont", (parser, x) => x.MilitaryCaptionFont = parser.ParseString() },
            { "MilitaryCaptionPointSize", (parser, x) => x.MilitaryCaptionPointSize = parser.ParseInteger() },
            { "MilitaryCaptionBold", (parser, x) => x.MilitaryCaptionBold = parser.ParseBoolean() },

            { "MilitaryCaptionRandomizeTyping", (parser, x) => x.MilitaryCaptionRandomizeTyping = parser.ParseBoolean() },
            { "MilitaryCaptionDelayMS", (parser, x) => x.MilitaryCaptionDelayMS = parser.ParseInteger() },

            { "DrawableCaptionFont", (parser, x) => x.DrawableCaptionFont = parser.ParseString() },
            { "DrawableCaptionPointSize", (parser, x) => x.DrawableCaptionPointSize = parser.ParseInteger() },
            { "DrawableCaptionBold", (parser, x) => x.DrawableCaptionBold = parser.ParseBoolean() },
            { "DrawableCaptionColor", (parser, x) => x.DrawableCaptionColor = parser.ParseColorRgb() },

            { "SuperweaponCountdownPosition", (parser, x) => x.SuperweaponCountdownPosition = parser.ParseVector2() },
            { "SuperweaponCountdownFlashDuration", (parser, x) => x.SuperweaponCountdownFlashDuration = parser.ParseInteger() },
            { "SuperweaponCountdownFlashColor", (parser, x) => x.SuperweaponCountdownFlashColor = parser.ParseColorRgb() },

            { "SuperweaponCountdownNormalFont", (parser, x) => x.SuperweaponCountdownNormalFont = parser.ParseString() },
            { "SuperweaponCountdownNormalPointSize", (parser, x) => x.SuperweaponCountdownNormalPointSize = parser.ParseInteger() },
            { "SuperweaponCountdownNormalBold", (parser, x) => x.SuperweaponCountdownNormalBold = parser.ParseBoolean() },

            { "SuperweaponCountdownReadyFont", (parser, x) => x.SuperweaponCountdownReadyFont = parser.ParseString() },
            { "SuperweaponCountdownReadyPointSize", (parser, x) => x.SuperweaponCountdownReadyPointSize = parser.ParseInteger() },
            { "SuperweaponCountdownReadyBold", (parser, x) => x.SuperweaponCountdownReadyBold = parser.ParseBoolean() },

            { "NamedTimerCountdownPosition", (parser, x) => x.NamedTimerCountdownPosition = parser.ParseVector2() },
            { "NamedTimerCountdownFlashDuration", (parser, x) => x.NamedTimerCountdownFlashDuration = parser.ParseInteger() },
            { "NamedTimerCountdownFlashColor", (parser, x) => x.NamedTimerCountdownFlashColor = parser.ParseColorRgb() },

            { "NamedTimerCountdownNormalFont", (parser, x) => x.NamedTimerCountdownNormalFont = parser.ParseString() },
            { "NamedTimerCountdownNormalPointSize", (parser, x) => x.NamedTimerCountdownNormalPointSize = parser.ParseInteger() },
            { "NamedTimerCountdownNormalBold", (parser, x) => x.NamedTimerCountdownNormalBold = parser.ParseBoolean() },
            { "NamedTimerCountdownNormalColor", (parser, x) => x.NamedTimerCountdownNormalColor = parser.ParseColorRgb() },

            { "NamedTimerCountdownReadyFont", (parser, x) => x.NamedTimerCountdownReadyFont = parser.ParseString() },
            { "NamedTimerCountdownReadyPointSize", (parser, x) => x.NamedTimerCountdownReadyPointSize = parser.ParseInteger() },
            { "NamedTimerCountdownReadyBold", (parser, x) => x.NamedTimerCountdownReadyBold = parser.ParseBoolean() },
            { "NamedTimerCountdownReadyColor", (parser, x) => x.NamedTimerCountdownReadyColor = parser.ParseColorRgb() },

            { "HelpBoxNameFont", (parser, x) => x.HelpBoxNameFont = parser.ParseString() },
            { "HelpBoxNamePointSize", (parser, x) => x.HelpBoxNamePointSize = parser.ParseInteger() },
            { "HelpBoxNameBold", (parser, x) => x.HelpBoxNameBold = parser.ParseBoolean() },
            { "HelpBoxNameColor", (parser, x) => x.HelpBoxNameColor = parser.ParseColorRgb() },

            { "HelpBoxCostFont", (parser, x) => x.HelpBoxCostFont = parser.ParseString() },
            { "HelpBoxCostPointSize", (parser, x) => x.HelpBoxCostPointSize = parser.ParseInteger() },
            { "HelpBoxCostBold", (parser, x) => x.HelpBoxCostBold = parser.ParseBoolean() },
            { "HelpBoxCostColor", (parser, x) => x.HelpBoxCostColor = parser.ParseColorRgb() },

            { "HelpBoxShortcutFont", (parser, x) => x.HelpBoxShortcutFont = parser.ParseString() },
            { "HelpBoxShortcutPointSize", (parser, x) => x.HelpBoxShortcutPointSize = parser.ParseInteger() },
            { "HelpBoxShortcutBold", (parser, x) => x.HelpBoxShortcutBold = parser.ParseBoolean() },
            { "HelpBoxShortcutColor", (parser, x) => x.HelpBoxShortcutColor = parser.ParseColorRgb() },

            { "HelpBoxDescriptionFont", (parser, x) => x.HelpBoxDescriptionFont = parser.ParseString() },
            { "HelpBoxDescriptionPointSize", (parser, x) => x.HelpBoxDescriptionPointSize = parser.ParseInteger() },
            { "HelpBoxDescriptionBold", (parser, x) => x.HelpBoxDescriptionBold = parser.ParseBoolean() },
            { "HelpBoxDescriptionColor", (parser, x) => x.HelpBoxDescriptionColor = parser.ParseColorRgb() },

            { "FloatingTextTimeOut", (parser, x) => x.FloatingTextTimeOut = parser.ParseInteger() },
            { "FloatingTextMoveUpSpeed", (parser, x) => x.FloatingTextMoveUpSpeed = parser.ParseInteger() },
            { "FloatingTextVanishRate", (parser, x) => x.FloatingTextVanishRate = parser.ParseInteger() },

            { "DrawRMBScrollAnchor", (parser, x) => x.DrawRmbScrollAnchor = parser.ParseBoolean() },
            { "MoveRMBScrollAnchor", (parser, x) => x.MoveRmbScrollAnchor = parser.ParseBoolean() },
            { "PopupMessageColor", (parser, x) => x.PopupMessageColor = parser.ParseColorRgba() },

            { "UnitHelpTextDelay", (parser, x) => x.UnitHelpTextDelay = parser.ParseFloat() },

            { "SpyDroneRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.SpyDrone.ToString())) },
            { "AttackScatterAreaRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.AttackScatterArea.ToString())) },
            { "SuperweaponScatterAreaRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.SuperweaponScatterArea.ToString())) },
            { "AttackDamageAreaRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.AttackDamageArea.ToString())) },
            { "AttackContinueAreaRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.AttackContinueArea.ToString())) },
            { "GuardAreaRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.GuardArea.ToString())) },
            { "EmergencyRepairRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.EmergencyRepair.ToString())) },
            { "FriendlySpecialPowerRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.FriendlySpecialPower.ToString())) },
            { "OffensiveSpecialPowerRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.OffensiveSpecialPower.ToString())) },
            { "ParticleCannonRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.ParticleCannon.ToString())) },
            { "A10StrikeRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.A10Strike.ToString())) },
            { "CarpetBombRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.CarpetBomb.ToString())) },
            { "DaisyCutterRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.DaisyCutter.ToString())) },
            { "ParadropRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Paradrop.ToString())) },
            { "SpySatelliteRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.SpySatellite.ToString())) },
            { "NuclearMissileRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.NuclearMissile.ToString())) },
            { "EMPPulseRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.EmpPulse.ToString())) },
            { "ArtilleryRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.ArtilleryBarrage.ToString())) },
            { "NapalmStrikeRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.NapalmStrike.ToString())) },
            { "ClusterMinesRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.ClusterMines.ToString())) },
            { "ScudStormRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.ScudStorm.ToString())) },
            { "AnthraxBombRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.AnthraxBomb.ToString())) },
            { "AmbushRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Ambush.ToString())) },
            { "RadarRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Radar.ToString())) },

            // Following set were added in Zero Hour.
            { "FrenzyRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Frenzy.ToString())) },
            { "SpectreGunshipRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.SpectreGunship.ToString())) },
            { "HelixNapalmBombRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.HelixNapalmBomb.ToString())) },
            { "ClearMinesRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.ClearMines.ToString())) },
            { "AmbulanceRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Ambulance.ToString())) },

            // Following set were added in BFME.
            { "ArrowStormRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.ArrowStorm.ToString())) },
            { "TrainingRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Training.ToString())) },
            { "ArcheryTrainingRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.ArcheryTraining.ToString())) },
            { "SummonBalrogRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.SummonBalrog.ToString())) },
            { "LightningSwordRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.LightningSword.ToString())) },
            { "FireBreathRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.FireBreath.ToString())) },
            { "LeapRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Leap.ToString())) },
            { "SummonOathBreakersRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.SummonOathBreakers.ToString())) },
            { "AthelasRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Athelas.ToString())) },
            { "FellBeastSwoopRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.FellBeastSwoop.ToString())) },
            { "EagleSwoopRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.EagleSwoop.ToString())) },
            { "IndustryRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Industry.ToString())) },
            { "DevastationRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Devastation.ToString())) },
            { "TaintRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Taint.ToString())) },
            { "EyeOfSauronRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.EyeOfSauron.ToString())) },
            { "HealRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Heal.ToString())) },
            { "ElvenAlliesRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.ElvenAllies.ToString())) },
            { "RohanAlliesRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.RohanAllies.ToString())) },
            { "ElvenWoodRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.ElvenWood.ToString())) },
            { "EntAlliesRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.EntAllies.ToString())) },
            { "ArmyOfTheDeadRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.ArmyOfTheDead.ToString())) },
            { "EagleAlliesRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.EagleAllies.ToString())) },
            { "KingsFavorRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.KingsFavor.ToString())) },
            { "DominateRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.Dominate.ToString())) },
            { "SpeechCraftRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.SpeechCraft.ToString())) },
            { "CaptainOfGondorRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.CaptainOfGondor.ToString())) },
            { "WarChantRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.WarChant.ToString())) },
            { "PalantirVisionRadiusCursor", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser, CommandButtonRadiusCursorType.PalantirVision.ToString())) },
            { "MessagePositionLW", (parser, x) => x.MessagePositionLW = parser.ParseVector2() },
            { "SelectNearestBuilderCycleTimeOut", (parser, x) => x.SelectNearestBuilderCycleTimeOut = parser.ParseInteger() },
            { "TerrainResourceClaimDecal", (parser, x) => x.TerrainResourceClaimDecal = parser.ParseAssetReference() },
            { "PlaceTerrainResourceClaimantDecal", (parser, x) => x.PlaceTerrainResourceClaimantDecal = parser.ParseAssetReference() },
            { "PlaceTerrainResourceClaimantFont", (parser, x) => x.PlaceTerrainResourceClaimantFont = parser.ParseAssetReference() },
            { "PlaceTerrainResourceClaimantFontColor", (parser, x) => x.PlaceTerrainResourceClaimantFontColor = parser.ParseColorRgba() },
            { "HeroInitialSpawnNotificationMessage", (parser, x) => x.HeroInitialSpawnNotificationMessage = parser.ParseLocalizedStringKey() },
            { "HeroInitialSpawnNotificationTimeout", (parser, x) => x.HeroInitialSpawnNotificationTimeout = parser.ParseFloat() },
            { "HeroRespawnNotificationMessage", (parser, x) => x.HeroRespawnNotificationMessage = parser.ParseLocalizedStringKey() },
            { "HeroRespawnNotificationTimeout", (parser, x) => x.HeroRespawnNotificationTimeout = parser.ParseFloat() },
            { "HeroDeathNotificationMessage", (parser, x) => x.HeroDeathNotificationMessage = parser.ParseLocalizedStringKey() },
            { "HeroDeathNotificationTimeout", (parser, x) => x.HeroDeathNotificationTimeout = parser.ParseFloat() },
            { "HeroEarnedAwardNotificationMessage", (parser, x) => x.HeroEarnedAwardNotificationMessage = parser.ParseLocalizedStringKey() },
            { "HeroEarnedAwardNotificationTimeout", (parser, x) => x.HeroEarnedAwardNotificationTimeout = parser.ParseFloat() },
            { "RadiusCursorUseWeaponScatterRadius", (parser, x) => x.RadiusCursorUseWeaponScatterRadius = parser.ParseAssetReference() },

            { "RadiusCursorTemplate", (parser, x) => x.AddRadiusCursor(RadiusCursor.Parse(parser)) }
        };

        private uint _unknown1;
        private bool _unknown2;
        private bool _unknown3;
        private bool _unknown4;
        private uint _unknown5;
        private readonly List<SuperweaponSomething> _superweaponSomethings = new();

        private void AddRadiusCursor(RadiusCursor radiusCursor)
        {
            RadiusCursors.Add(radiusCursor.Name, radiusCursor);
        }

        public int MaxSelectionSize { get; private set; }

        public ColorRgb MessageColor1 { get; private set; }
        public ColorRgb MessageColor2 { get; private set; }
        public Vector2 MessagePosition { get; private set; }
        public string MessageFont { get; private set; }
        public int MessagePointSize { get; private set; }
        public bool MessageBold { get; private set; }
        public int MessageDelayMS { get; private set; }

        public ColorRgba MilitaryCaptionColor { get; private set; }
        public Vector2 MilitaryCaptionPosition { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool MilitaryCaptionCentered { get; private set; }

        public string MilitaryCaptionTitleFont { get; private set; }
        public int MilitaryCaptionTitlePointSize { get; private set; }
        public bool MilitaryCaptionTitleBold { get; private set; }

        public string MilitaryCaptionFont { get; private set; }
        public int MilitaryCaptionPointSize { get; private set; }
        public bool MilitaryCaptionBold { get; private set; }

        public bool MilitaryCaptionRandomizeTyping { get; private set; }
        public int MilitaryCaptionDelayMS { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string DrawableCaptionFont { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int DrawableCaptionPointSize { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool DrawableCaptionBold { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ColorRgb DrawableCaptionColor { get; private set; }

        public Vector2 SuperweaponCountdownPosition { get; private set; }
        public int SuperweaponCountdownFlashDuration { get; private set; }
        public ColorRgb SuperweaponCountdownFlashColor { get; private set; }

        public string SuperweaponCountdownNormalFont { get; private set; }
        public int SuperweaponCountdownNormalPointSize { get; private set; }
        public bool SuperweaponCountdownNormalBold { get; private set; }

        public string SuperweaponCountdownReadyFont { get; private set; }
        public int SuperweaponCountdownReadyPointSize { get; private set; }
        public bool SuperweaponCountdownReadyBold { get; private set; }

        public Vector2 NamedTimerCountdownPosition { get; private set; }
        public int NamedTimerCountdownFlashDuration { get; private set; }
        public ColorRgb NamedTimerCountdownFlashColor { get; private set; }

        public string NamedTimerCountdownNormalFont { get; private set; }
        public int NamedTimerCountdownNormalPointSize { get; private set; }
        public bool NamedTimerCountdownNormalBold { get; private set; }
        public ColorRgb NamedTimerCountdownNormalColor { get; private set; }

        public string NamedTimerCountdownReadyFont { get; private set; }
        public int NamedTimerCountdownReadyPointSize { get; private set; }
        public bool NamedTimerCountdownReadyBold { get; private set; }
        public ColorRgb NamedTimerCountdownReadyColor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string HelpBoxNameFont { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int HelpBoxNamePointSize { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool HelpBoxNameBold { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ColorRgb HelpBoxNameColor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string HelpBoxCostFont { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int HelpBoxCostPointSize { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool HelpBoxCostBold { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ColorRgb HelpBoxCostColor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string HelpBoxShortcutFont { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int HelpBoxShortcutPointSize { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool HelpBoxShortcutBold { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ColorRgb HelpBoxShortcutColor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string HelpBoxDescriptionFont { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int HelpBoxDescriptionPointSize { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool HelpBoxDescriptionBold { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ColorRgb HelpBoxDescriptionColor { get; private set; }

        public int FloatingTextTimeOut { get; private set; }
        public int FloatingTextMoveUpSpeed { get; private set; }
        public int FloatingTextVanishRate { get; private set; }

        public bool DrawRmbScrollAnchor { get; private set; }
        public bool MoveRmbScrollAnchor { get; private set; }
        public ColorRgba PopupMessageColor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float UnitHelpTextDelay { get; private set; }

        public Dictionary<string, RadiusCursor> RadiusCursors { get; } = new Dictionary<string, RadiusCursor>();

        [AddedIn(SageGame.Bfme2)]
        public Vector2 MessagePositionLW { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int SelectNearestBuilderCycleTimeOut { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string TerrainResourceClaimDecal { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string PlaceTerrainResourceClaimantDecal { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string PlaceTerrainResourceClaimantFont { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ColorRgba PlaceTerrainResourceClaimantFontColor { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string HeroInitialSpawnNotificationMessage { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float HeroInitialSpawnNotificationTimeout { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string HeroRespawnNotificationMessage { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float HeroRespawnNotificationTimeout { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string HeroDeathNotificationMessage { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float HeroDeathNotificationTimeout { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string HeroEarnedAwardNotificationMessage { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float HeroEarnedAwardNotificationTimeout { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string RadiusCursorUseWeaponScatterRadius { get; private set; }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.PersistUInt32("Unknown1", ref _unknown1); // 0
            reader.PersistBoolean("Unknown2", ref _unknown2);
            reader.PersistBoolean("Unknown3", ref _unknown3);
            reader.PersistBoolean("Unknown4", ref _unknown4);
            reader.PersistUInt32("Unknown5", ref _unknown5); // 0

            // TODO: Superweapon something...
            reader.BeginArray("SuperweaponSomethings");
            if (reader.Mode == StatePersistMode.Read)
            {
                while (true)
                {
                    reader.BeginObject();

                    var something = 0u;
                    reader.PersistUInt32("Something", ref something);

                    // A way to store things the engine doesn't know the length of?
                    if (something == uint.MaxValue)
                    {
                        break;
                    }

                    var item = new SuperweaponSomething();
                    reader.PersistObject("Item", ref item);
                    _superweaponSomethings.Add(item);

                    reader.EndObject();
                }
            }
            else
            {
                for (var i = 0u; i < _superweaponSomethings.Count; i++)
                {
                    reader.BeginObject();

                    reader.PersistUInt32("Something", ref i);

                    var item = _superweaponSomethings[(int)i];
                    reader.PersistObject("Item", ref item);

                    reader.EndObject();
                }

                reader.BeginObject();

                var somethingEnd = uint.MaxValue;
                reader.PersistUInt32("Something", ref somethingEnd);

                reader.EndObject();
            }
            reader.EndArray();
        }
    }

    internal struct SuperweaponSomething : IPersistableObject
    {
        public string UnknownString1;
        public string UnknownString2;
        public uint UnknownInt1;
        public uint UnknownInt2;
        public bool UnknownBool1;
        public bool UnknownBool2;
        public bool UnknownBool3;
        public uint UnknownInt3;

        public void Persist(StatePersister reader)
        {
            reader.PersistAsciiString("UnknownString1", ref UnknownString1);
            reader.PersistAsciiString("UnknownString2", ref UnknownString2);

            reader.PersistUInt32("UnknownInt1", ref UnknownInt1);
            reader.PersistUInt32("UnknownInt2", ref UnknownInt2); // 0xFFFFFFFF
            reader.PersistBoolean("UnknownBool1", ref UnknownBool1);
            reader.PersistBoolean("UnknownBool2", ref UnknownBool2);
            reader.PersistBoolean("UnknownBool3", ref UnknownBool3);
        }
    }

    public sealed class RadiusCursor
    {
        internal static RadiusCursor Parse(IniParser parser)
        {
            return  new RadiusCursor
            {
                Name = parser.ParseString(),
                DecalTemplate = RadiusDecalTemplate.Parse(parser)
            };
        }

        internal static RadiusCursor Parse(IniParser parser, string name)
        {
            return new RadiusCursor
            {
                Name = name,
                DecalTemplate = RadiusDecalTemplate.Parse(parser)
            };
        }

        public string Name { get; private set; }

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
            { "Texture", (parser, x) => x.Texture = parser.ParseTextureReference() },
            { "Texture2", (parser, x) => x.Texture2 = parser.ParseTextureReference() },
            { "Style", (parser, x) => x.Style = parser.ParseEnum<RadiusDecalStyle>() },
            { "OpacityMin", (parser, x) => x.OpacityMin = parser.ParsePercentage() },
            { "OpacityMax", (parser, x) => x.OpacityMax = parser.ParsePercentage() },
            { "OpacityThrobTime", (parser, x) => x.OpacityThrobTime = parser.ParseTimeMilliseconds() },
            { "Color", (parser, x) => x.Color = parser.ParseColorRgba() },
            { "OnlyVisibleToOwningPlayer", (parser, x) => x.OnlyVisibleToOwningPlayer = parser.ParseBoolean() },
            { "MinRadius", (parser, x) => x.MinRadius = parser.ParseFloat() },
            { "MaxRadius", (parser, x) => x.MaxRadius = parser.ParseFloat() },
            { "MaxSelectedUnits", (parser, x) => x.MaxSelectedUnits = parser.ParseInteger() },
            { "RotationsPerMinute", (parser, x) => x.RotationsPerMinute = parser.ParseFloat() },
            { "SpiralAcceleration", (parser, x) => x.SpiralAcceleration = parser.ParseFloat() },
        };

        public LazyAssetReference<TextureAsset> Texture { get; private set; }
        public LazyAssetReference<TextureAsset> Texture2 { get; private set; }
        public RadiusDecalStyle Style { get; private set; }
        public Percentage OpacityMin { get; private set; } = new Percentage(1.0f);
        public Percentage OpacityMax { get; private set; } = new Percentage(1.0f);
        public TimeSpan OpacityThrobTime { get; private set; } = TimeSpan.FromSeconds(1);
        public ColorRgba Color { get; private set; }
        public bool OnlyVisibleToOwningPlayer { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float MinRadius { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float MaxRadius { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MaxSelectedUnits { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float RotationsPerMinute { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float SpiralAcceleration { get; private set; }
    }

    public enum RadiusDecalStyle
    {
        [IniEnum("SHADOW_ALPHA_DECAL")]
        ShadowAlphaDecal,

        [IniEnum("SHADOW_ADDITIVE_DECAL")]
        ShadowAdditiveDecal,

        [IniEnum("SHADOW_MERGE_DECAL"), AddedIn(SageGame.Bfme)]
        ShadowMergeDecal
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Gui.ControlBar
{
    public sealed class ControlBarSchemeCollection : Collection<ControlBarScheme>
    {
        public ControlBarScheme FindBySide(string side)
        {
            // Based on a comment in PlayerTemplate.ini about how control bar schemes are chosen.
            return this.FirstOrDefault(x => x.Side == side)
                ?? this.FirstOrDefault(x => x.Side == "Observer")
                ?? throw new InvalidOperationException("No ControlBarScheme could be found for the specified side, and no ControlBarScheme for the Observer side could be found either.");
        }
    }

    public sealed class ControlBarScheme
    {
        internal static ControlBarScheme Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                 (x, name) => x.Name = name,
                 FieldParseTable);
        }

        private static readonly IniParseTable<ControlBarScheme> FieldParseTable = new IniParseTable<ControlBarScheme>
        {
            { "ScreenCreationRes", (parser, x) => x.ScreenCreationRes = parser.ParseSize() },
            { "Side", (parser, x) => x.Side = parser.ParseAssetReference() },
            { "QueueButtonImage", (parser, x) => x.QueueButtonImage = parser.ParseFileName() },
            { "RightHUDImage", (parser, x) => x.RightHudImage = parser.ParseAssetReference() },
            { "CommandBarBorderColor", (parser, x) => x.CommandBarBorderColor = parser.ParseColorRgba() },
            { "BuildUpClockColor", (parser, x) => x.BuildUpClockColor = parser.ParseColorRgba() },
            { "ButtonBorderBuildColor", (parser, x) => x.ButtonBorderBuildColor = parser.ParseColorRgba() },
            { "ButtonBorderActionColor", (parser, x) => x.ButtonBorderActionColor = parser.ParseColorRgba() },
            { "ButtonBorderUpgradeColor", (parser, x) => x.ButtonBorderUpgradeColor = parser.ParseColorRgba() },
            { "ButtonBorderSystemColor", (parser, x) => x.ButtonBorderSystemColor = parser.ParseColorRgba() },
            { "ButtonBorderAlteredColor", (parser, x) => x.ButtonBorderAlteredColor = parser.ParseColorRgba() },

            { "GenBarButtonIn", (parser, x) => x.GenBarButtonIn = parser.ParseAssetReference() },
            { "GenBarButtonOn", (parser, x) => x.GenBarButtonOn = parser.ParseAssetReference() },

            { "ToggleButtonUpIn", (parser, x) => x.ToggleButtonUpIn = parser.ParseAssetReference() },
            { "ToggleButtonUpOn", (parser, x) => x.ToggleButtonUpOn = parser.ParseAssetReference() },
            { "ToggleButtonUpPushed", (parser, x) => x.ToggleButtonUpPushed = parser.ParseAssetReference() },

            { "ToggleButtonDownIn", (parser, x) => x.ToggleButtonDownIn = parser.ParseAssetReference() },
            { "ToggleButtonDownOn", (parser, x) => x.ToggleButtonDownOn = parser.ParseAssetReference() },
            { "ToggleButtonDownPushed", (parser, x) => x.ToggleButtonDownPushed = parser.ParseAssetReference() },

            { "OptionsButtonEnable", (parser, x) => x.OptionsButtonEnable = parser.ParseAssetReference() },
            { "OptionsButtonHightlited", (parser, x) => x.OptionsButtonHighlighted = parser.ParseAssetReference() },
            { "OptionsButtonPushed", (parser, x) => x.OptionsButtonPushed = parser.ParseAssetReference() },
            { "OptionsButtonDisabled", (parser, x) => x.OptionsButtonDisabled = parser.ParseAssetReference() },

            { "IdleWorkerButtonEnable", (parser, x) => x.IdleWorkerButtonEnable = parser.ParseAssetReference() },
            { "IdleWorkerButtonHightlited", (parser, x) => x.IdleWorkerButtonHighlighted = parser.ParseAssetReference() },
            { "IdleWorkerButtonPushed", (parser, x) => x.IdleWorkerButtonPushed = parser.ParseAssetReference() },
            { "IdleWorkerButtonDisabled", (parser, x) => x.IdleWorkerButtonDisabled = parser.ParseAssetReference() },

            { "BuddyButtonEnable", (parser, x) => x.BuddyButtonEnable = parser.ParseAssetReference() },
            { "BuddyButtonHightlited", (parser, x) => x.BuddyButtonHighlighted = parser.ParseAssetReference() },
            { "BuddyButtonPushed", (parser, x) => x.BuddyButtonPushed = parser.ParseAssetReference() },
            { "BuddyButtonDisabled", (parser, x) => x.BuddyButtonDisabled = parser.ParseAssetReference() },

            { "BeaconButtonEnable", (parser, x) => x.BeaconButtonEnable = parser.ParseAssetReference() },
            { "BeaconButtonHightlited", (parser, x) => x.BeaconButtonHighlighted = parser.ParseAssetReference() },
            { "BeaconButtonPushed", (parser, x) => x.BeaconButtonPushed = parser.ParseAssetReference() },
            { "BeaconButtonDisabled", (parser, x) => x.BeaconButtonDisabled = parser.ParseAssetReference() },

            { "GeneralButtonEnable", (parser, x) => x.GeneralButtonEnable = parser.ParseAssetReference() },
            { "GeneralButtonHightlited", (parser, x) => x.GeneralButtonHighlighted = parser.ParseAssetReference() },
            { "GeneralButtonPushed", (parser, x) => x.GeneralButtonPushed = parser.ParseAssetReference() },
            { "GeneralButtonDisabled", (parser, x) => x.GeneralButtonDisabled = parser.ParseAssetReference() },

            { "UAttackButtonEnable", (parser, x) => x.UAttackButtonEnable = parser.ParseAssetReference() },
            { "UAttackButtonHightlited", (parser, x) => x.UAttackButtonHighlighted = parser.ParseAssetReference() },
            { "UAttackButtonPushed", (parser, x) => x.UAttackButtonPushed = parser.ParseAssetReference() },

            { "MinMaxButtonEnable", (parser, x) => x.MinMaxButtonEnable = parser.ParseAssetReference() },
            { "MinMaxButtonHightlited", (parser, x) => x.MinMaxButtonHighlighted = parser.ParseAssetReference() },
            { "MinMaxButtonPushed", (parser, x) => x.MinMaxButtonPushed = parser.ParseAssetReference() },

            { "MinMaxUL", (parser, x) => x.MinMaxUL = parser.ParsePoint() },
            { "MinMaxLR", (parser, x) => x.MinMaxLR = parser.ParsePoint() },

            { "GeneralUL", (parser, x) => x.GeneralUL = parser.ParsePoint() },
            { "GeneralLR", (parser, x) => x.GeneralLR = parser.ParsePoint() },

            { "UAttackUL", (parser, x) => x.UAttackUL = parser.ParsePoint() },
            { "UAttackLR", (parser, x) => x.UAttackLR = parser.ParsePoint() },

            { "OptionsUL", (parser, x) => x.OptionsUL = parser.ParsePoint() },
            { "OptionsLR", (parser, x) => x.OptionsLR = parser.ParsePoint() },

            { "WorkerUL", (parser, x) => x.WorkerUL = parser.ParsePoint() },
            { "WorkerLR", (parser, x) => x.WorkerLR = parser.ParsePoint() },

            { "ChatUL", (parser, x) => x.ChatUL = parser.ParsePoint() },
            { "ChatLR", (parser, x) => x.ChatLR = parser.ParsePoint() },

            { "BeaconUL", (parser, x) => x.BeaconUL = parser.ParsePoint() },
            { "BeaconLR", (parser, x) => x.BeaconLR = parser.ParsePoint() },

            { "PowerBarUL", (parser, x) => x.PowerBarUL = parser.ParsePoint() },
            { "PowerBarLR", (parser, x) => x.PowerBarLR = parser.ParsePoint() },

            { "ExpBarForegroundImage", (parser, x) => x.ExpBarForegroundImage = parser.ParseAssetReference() },

            { "MoneyUL", (parser, x) => x.MoneyUL = parser.ParsePoint() },
            { "MoneyLR", (parser, x) => x.MoneyLR = parser.ParsePoint() },

            { "GenArrow", (parser, x) => x.GenArrow = parser.ParseAssetReference() },
            { "CommandMarkerImage", (parser, x) => x.CommandMarkerImage = parser.ParseAssetReference() },

            { "ImagePart", (parser, x) => x.ImageParts.Add(ControlBarImagePart.Parse(parser)) },

            { "PowerPurchaseImage", (parser, x) => x.PowerPurchaseImage = parser.ParseAssetReference() },
        };

        public string Name { get; private set; }

        public Size ScreenCreationRes { get; private set; }
        public string Side { get; private set; }
        public string QueueButtonImage { get; private set; }
        public string RightHudImage { get; private set; }
        public ColorRgba CommandBarBorderColor { get; private set; }
        public ColorRgba BuildUpClockColor { get; private set; }
        public ColorRgba ButtonBorderBuildColor { get; private set; }
        public ColorRgba ButtonBorderActionColor { get; private set; }
        public ColorRgba ButtonBorderUpgradeColor { get; private set; }
        public ColorRgba ButtonBorderSystemColor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ColorRgba ButtonBorderAlteredColor { get; private set; }

        public string GenBarButtonIn { get; private set; }
        public string GenBarButtonOn { get; private set; }

        public string ToggleButtonUpIn { get; private set; }
        public string ToggleButtonUpOn { get; private set; }
        public string ToggleButtonUpPushed { get; private set; }

        public string ToggleButtonDownIn { get; private set; }
        public string ToggleButtonDownOn { get; private set; }
        public string ToggleButtonDownPushed { get; private set; }

        public string OptionsButtonEnable { get; private set; }
        public string OptionsButtonHighlighted { get; private set; }
        public string OptionsButtonPushed { get; private set; }
        public string OptionsButtonDisabled { get; private set; }

        public string IdleWorkerButtonEnable { get; private set; }
        public string IdleWorkerButtonHighlighted { get; private set; }
        public string IdleWorkerButtonPushed { get; private set; }
        public string IdleWorkerButtonDisabled { get; private set; }

        public string BuddyButtonEnable { get; private set; }
        public string BuddyButtonHighlighted { get; private set; }
        public string BuddyButtonPushed { get; private set; }
        public string BuddyButtonDisabled { get; private set; }

        public string BeaconButtonEnable { get; private set; }
        public string BeaconButtonHighlighted { get; private set; }
        public string BeaconButtonPushed { get; private set; }
        public string BeaconButtonDisabled { get; private set; }

        public string GeneralButtonEnable { get; private set; }
        public string GeneralButtonHighlighted { get; private set; }
        public string GeneralButtonPushed { get; private set; }
        public string GeneralButtonDisabled { get; private set; }

        public string UAttackButtonEnable { get; private set; }
        public string UAttackButtonHighlighted { get; private set; }
        public string UAttackButtonPushed { get; private set; }

        public string MinMaxButtonEnable { get; private set; }
        public string MinMaxButtonHighlighted { get; private set; }
        public string MinMaxButtonPushed { get; private set; }

        public Point2D MinMaxUL { get; private set; }
        public Point2D MinMaxLR { get; private set; }
        public Point2D GeneralUL { get; private set; }
        public Point2D GeneralLR { get; private set; }
        public Point2D UAttackUL { get; private set; }
        public Point2D UAttackLR { get; private set; }

        public Point2D OptionsUL { get; private set; }
        public Point2D  OptionsLR { get; private set; }
        public Point2D  WorkerUL { get; private set; }
        public Point2D  WorkerLR { get; private set; }
        public Point2D  ChatUL { get; private set; }
        public Point2D  ChatLR { get; private set; }
        public Point2D  BeaconUL { get; private set; }
        public Point2D  BeaconLR { get; private set; }

        public Point2D PowerBarUL { get; private set; }
        public Point2D PowerBarLR { get; private set; }
        public string ExpBarForegroundImage { get; private set; }
        public Point2D MoneyUL { get; private set; }
        public Point2D MoneyLR { get; private set; }

        public string GenArrow { get; private set; }
        public string CommandMarkerImage { get; private set; }

        public List<ControlBarImagePart> ImageParts { get; } = new List<ControlBarImagePart>();

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string PowerPurchaseImage { get; private set; }
    }

    public sealed class ControlBarImagePart
    {
        internal static ControlBarImagePart Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ControlBarImagePart> FieldParseTable = new IniParseTable<ControlBarImagePart>
        {
            { "Position", (parser, x) => x.Position = parser.ParsePoint() },
            { "Size", (parser, x) => x.Size = parser.ParseSize() },
            { "ImageName", (parser, x) => x.ImageName = parser.ParseAssetReference() },
            { "Layer", (parser, x) => x.Layer = parser.ParseInteger() }
        };

        public Point2D Position { get; private set; }
        public Size Size { get; private set; }
        public string ImageName { get; private set; }
        public int Layer { get; private set; }
    }
}

using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Gui.ControlBar
{
    public sealed class ControlBarScheme : BaseAsset
    {
        internal static ControlBarScheme Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                 (x, name) => x.SetNameAndInstanceId("ControlBarScheme", name),
                 FieldParseTable);
        }

        private static readonly IniParseTable<ControlBarScheme> FieldParseTable = new IniParseTable<ControlBarScheme>
        {
            { "ScreenCreationRes", (parser, x) => x.ScreenCreationRes = parser.ParseSize() },
            { "Side", (parser, x) => x.Side = parser.ParseAssetReference() },
            { "QueueButtonImage", (parser, x) => x.QueueButtonImage = parser.ParseMappedImageReference() },
            { "RightHUDImage", (parser, x) => x.RightHudImage = parser.ParseMappedImageReference() },
            { "CommandBarBorderColor", (parser, x) => x.CommandBarBorderColor = parser.ParseColorRgba() },
            { "BuildUpClockColor", (parser, x) => x.BuildUpClockColor = parser.ParseColorRgba() },
            { "ButtonBorderBuildColor", (parser, x) => x.ButtonBorderBuildColor = parser.ParseColorRgba() },
            { "ButtonBorderActionColor", (parser, x) => x.ButtonBorderActionColor = parser.ParseColorRgba() },
            { "ButtonBorderUpgradeColor", (parser, x) => x.ButtonBorderUpgradeColor = parser.ParseColorRgba() },
            { "ButtonBorderSystemColor", (parser, x) => x.ButtonBorderSystemColor = parser.ParseColorRgba() },
            { "ButtonBorderAlteredColor", (parser, x) => x.ButtonBorderAlteredColor = parser.ParseColorRgba() },

            { "GenBarButtonIn", (parser, x) => x.GenBarButtonIn = parser.ParseMappedImageReference() },
            { "GenBarButtonOn", (parser, x) => x.GenBarButtonOn = parser.ParseMappedImageReference() },

            { "ToggleButtonUpIn", (parser, x) => x.ToggleButtonUpIn = parser.ParseMappedImageReference() },
            { "ToggleButtonUpOn", (parser, x) => x.ToggleButtonUpOn = parser.ParseMappedImageReference() },
            { "ToggleButtonUpPushed", (parser, x) => x.ToggleButtonUpPushed = parser.ParseMappedImageReference() },

            { "ToggleButtonDownIn", (parser, x) => x.ToggleButtonDownIn = parser.ParseMappedImageReference() },
            { "ToggleButtonDownOn", (parser, x) => x.ToggleButtonDownOn = parser.ParseMappedImageReference() },
            { "ToggleButtonDownPushed", (parser, x) => x.ToggleButtonDownPushed = parser.ParseMappedImageReference() },

            { "OptionsButtonEnable", (parser, x) => x.OptionsButtonEnable = parser.ParseMappedImageReference() },
            { "OptionsButtonHightlited", (parser, x) => x.OptionsButtonHighlighted = parser.ParseMappedImageReference() },
            { "OptionsButtonPushed", (parser, x) => x.OptionsButtonPushed = parser.ParseMappedImageReference() },
            { "OptionsButtonDisabled", (parser, x) => x.OptionsButtonDisabled = parser.ParseMappedImageReference() },

            { "IdleWorkerButtonEnable", (parser, x) => x.IdleWorkerButtonEnable = parser.ParseMappedImageReference() },
            { "IdleWorkerButtonHightlited", (parser, x) => x.IdleWorkerButtonHighlighted = parser.ParseMappedImageReference() },
            { "IdleWorkerButtonPushed", (parser, x) => x.IdleWorkerButtonPushed = parser.ParseMappedImageReference() },
            { "IdleWorkerButtonDisabled", (parser, x) => x.IdleWorkerButtonDisabled = parser.ParseMappedImageReference() },

            { "BuddyButtonEnable", (parser, x) => x.BuddyButtonEnable = parser.ParseMappedImageReference() },
            { "BuddyButtonHightlited", (parser, x) => x.BuddyButtonHighlighted = parser.ParseMappedImageReference() },
            { "BuddyButtonPushed", (parser, x) => x.BuddyButtonPushed = parser.ParseMappedImageReference() },
            { "BuddyButtonDisabled", (parser, x) => x.BuddyButtonDisabled = parser.ParseMappedImageReference() },

            { "BeaconButtonEnable", (parser, x) => x.BeaconButtonEnable = parser.ParseMappedImageReference() },
            { "BeaconButtonHightlited", (parser, x) => x.BeaconButtonHighlighted = parser.ParseMappedImageReference() },
            { "BeaconButtonPushed", (parser, x) => x.BeaconButtonPushed = parser.ParseMappedImageReference() },
            { "BeaconButtonDisabled", (parser, x) => x.BeaconButtonDisabled = parser.ParseMappedImageReference() },

            { "GeneralButtonEnable", (parser, x) => x.GeneralButtonEnable = parser.ParseMappedImageReference() },
            { "GeneralButtonHightlited", (parser, x) => x.GeneralButtonHighlighted = parser.ParseMappedImageReference() },
            { "GeneralButtonPushed", (parser, x) => x.GeneralButtonPushed = parser.ParseMappedImageReference() },
            { "GeneralButtonDisabled", (parser, x) => x.GeneralButtonDisabled = parser.ParseMappedImageReference() },

            { "UAttackButtonEnable", (parser, x) => x.UAttackButtonEnable = parser.ParseMappedImageReference() },
            { "UAttackButtonHightlited", (parser, x) => x.UAttackButtonHighlighted = parser.ParseMappedImageReference() },
            { "UAttackButtonPushed", (parser, x) => x.UAttackButtonPushed = parser.ParseMappedImageReference() },

            { "MinMaxButtonEnable", (parser, x) => x.MinMaxButtonEnable = parser.ParseMappedImageReference() },
            { "MinMaxButtonHightlited", (parser, x) => x.MinMaxButtonHighlighted = parser.ParseMappedImageReference() },
            { "MinMaxButtonPushed", (parser, x) => x.MinMaxButtonPushed = parser.ParseMappedImageReference() },

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

            { "ExpBarForegroundImage", (parser, x) => x.ExpBarForegroundImage = parser.ParseMappedImageReference() },

            { "MoneyUL", (parser, x) => x.MoneyUL = parser.ParsePoint() },
            { "MoneyLR", (parser, x) => x.MoneyLR = parser.ParsePoint() },

            { "GenArrow", (parser, x) => x.GenArrow = parser.ParseMappedImageReference() },
            { "CommandMarkerImage", (parser, x) => x.CommandMarkerImage = parser.ParseMappedImageReference() },

            { "ImagePart", (parser, x) => x.ImageParts.Add(ControlBarImagePart.Parse(parser)) },

            { "PowerPurchaseImage", (parser, x) => x.PowerPurchaseImage = parser.ParseMappedImageReference() },
        };

        public Size ScreenCreationRes { get; private set; }
        public string Side { get; private set; }
        public LazyAssetReference<MappedImage> QueueButtonImage { get; private set; }
        public LazyAssetReference<MappedImage> RightHudImage { get; private set; }
        public ColorRgba CommandBarBorderColor { get; private set; }
        public ColorRgba BuildUpClockColor { get; private set; }
        public ColorRgba ButtonBorderBuildColor { get; private set; }
        public ColorRgba ButtonBorderActionColor { get; private set; }
        public ColorRgba ButtonBorderUpgradeColor { get; private set; }
        public ColorRgba ButtonBorderSystemColor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ColorRgba ButtonBorderAlteredColor { get; private set; }

        public LazyAssetReference<MappedImage> GenBarButtonIn { get; private set; }
        public LazyAssetReference<MappedImage> GenBarButtonOn { get; private set; }

        public LazyAssetReference<MappedImage> ToggleButtonUpIn { get; private set; }
        public LazyAssetReference<MappedImage> ToggleButtonUpOn { get; private set; }
        public LazyAssetReference<MappedImage> ToggleButtonUpPushed { get; private set; }

        public LazyAssetReference<MappedImage> ToggleButtonDownIn { get; private set; }
        public LazyAssetReference<MappedImage> ToggleButtonDownOn { get; private set; }
        public LazyAssetReference<MappedImage> ToggleButtonDownPushed { get; private set; }

        public LazyAssetReference<MappedImage> OptionsButtonEnable { get; private set; }
        public LazyAssetReference<MappedImage> OptionsButtonHighlighted { get; private set; }
        public LazyAssetReference<MappedImage> OptionsButtonPushed { get; private set; }
        public LazyAssetReference<MappedImage> OptionsButtonDisabled { get; private set; }

        public LazyAssetReference<MappedImage> IdleWorkerButtonEnable { get; private set; }
        public LazyAssetReference<MappedImage> IdleWorkerButtonHighlighted { get; private set; }
        public LazyAssetReference<MappedImage> IdleWorkerButtonPushed { get; private set; }
        public LazyAssetReference<MappedImage> IdleWorkerButtonDisabled { get; private set; }

        public LazyAssetReference<MappedImage> BuddyButtonEnable { get; private set; }
        public LazyAssetReference<MappedImage> BuddyButtonHighlighted { get; private set; }
        public LazyAssetReference<MappedImage> BuddyButtonPushed { get; private set; }
        public LazyAssetReference<MappedImage> BuddyButtonDisabled { get; private set; }

        public LazyAssetReference<MappedImage> BeaconButtonEnable { get; private set; }
        public LazyAssetReference<MappedImage> BeaconButtonHighlighted { get; private set; }
        public LazyAssetReference<MappedImage> BeaconButtonPushed { get; private set; }
        public LazyAssetReference<MappedImage> BeaconButtonDisabled { get; private set; }

        public LazyAssetReference<MappedImage> GeneralButtonEnable { get; private set; }
        public LazyAssetReference<MappedImage> GeneralButtonHighlighted { get; private set; }
        public LazyAssetReference<MappedImage> GeneralButtonPushed { get; private set; }
        public LazyAssetReference<MappedImage> GeneralButtonDisabled { get; private set; }

        public LazyAssetReference<MappedImage> UAttackButtonEnable { get; private set; }
        public LazyAssetReference<MappedImage> UAttackButtonHighlighted { get; private set; }
        public LazyAssetReference<MappedImage> UAttackButtonPushed { get; private set; }

        public LazyAssetReference<MappedImage> MinMaxButtonEnable { get; private set; }
        public LazyAssetReference<MappedImage> MinMaxButtonHighlighted { get; private set; }
        public LazyAssetReference<MappedImage> MinMaxButtonPushed { get; private set; }

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
        public LazyAssetReference<MappedImage> ExpBarForegroundImage { get; private set; }
        public Point2D MoneyUL { get; private set; }
        public Point2D MoneyLR { get; private set; }

        public LazyAssetReference<MappedImage> GenArrow { get; private set; }
        public LazyAssetReference<MappedImage> CommandMarkerImage { get; private set; }

        public List<ControlBarImagePart> ImageParts { get; } = new List<ControlBarImagePart>();

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public LazyAssetReference<MappedImage> PowerPurchaseImage { get; private set; }
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
            { "ImageName", (parser, x) => x.ImageName = parser.ParseMappedImageReference() },
            { "Layer", (parser, x) => x.Layer = parser.ParseInteger() }
        };

        public Point2D Position { get; private set; }
        public Size Size { get; private set; }
        public LazyAssetReference<MappedImage> ImageName { get; private set; }
        public int Layer { get; private set; }
    }
}

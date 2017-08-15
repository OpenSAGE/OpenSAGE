using System.Collections.Generic;
using OpenZH.Data.Ini.Parser;
using OpenZH.Data.Wnd;

namespace OpenZH.Data.Ini
{
    public sealed class ControlBarScheme
    {
        internal static ControlBarScheme Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                 (x, name) => x.Name = name,
                 FieldParseTable);
        }

        private static readonly IniParseTable<ControlBarScheme> FieldParseTable = new IniParseTable<ControlBarScheme>
        {
            { "ScreenCreationRes", (parser, x) => x.ScreenCreationRes = WndPoint.Parse(parser) },
            { "Side", (parser, x) => x.Side = parser.ParseAssetReference() },
            { "QueueButtonImage", (parser, x) => x.QueueButtonImage = parser.ParseFileName() },
            { "RightHUDImage", (parser, x) => x.RightHudImage = parser.ParseAssetReference() },
            { "CommandBarBorderColor", (parser, x) => x.CommandBarBorderColor = WndColor.Parse(parser) },
            { "BuildUpClockColor", (parser, x) => x.BuildUpClockColor = WndColor.Parse(parser) },
            { "ButtonBorderBuildColor", (parser, x) => x.ButtonBorderBuildColor = WndColor.Parse(parser) },
            { "ButtonBorderActionColor", (parser, x) => x.ButtonBorderActionColor = WndColor.Parse(parser) },
            { "ButtonBorderUpgradeColor", (parser, x) => x.ButtonBorderUpgradeColor = WndColor.Parse(parser) },
            { "ButtonBorderSystemColor", (parser, x) => x.ButtonBorderSystemColor = WndColor.Parse(parser) },

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

            { "MinMaxUL", (parser, x) => x.MinMaxUL = WndPoint.Parse(parser) },
            { "MinMaxLR", (parser, x) => x.MinMaxLR = WndPoint.Parse(parser) },

            { "GeneralUL", (parser, x) => x.GeneralUL = WndPoint.Parse(parser) },
            { "GeneralLR", (parser, x) => x.GeneralLR = WndPoint.Parse(parser) },

            { "UAttackUL", (parser, x) => x.UAttackUL = WndPoint.Parse(parser) },
            { "UAttackLR", (parser, x) => x.UAttackLR = WndPoint.Parse(parser) },

            { "OptionsUL", (parser, x) => x.OptionsUL = WndPoint.Parse(parser) },
            { "OptionsLR", (parser, x) => x.OptionsLR = WndPoint.Parse(parser) },

            { "WorkerUL", (parser, x) => x.WorkerUL = WndPoint.Parse(parser) },
            { "WorkerLR", (parser, x) => x.WorkerLR = WndPoint.Parse(parser) },

            { "ChatUL", (parser, x) => x.ChatUL = WndPoint.Parse(parser) },
            { "ChatLR", (parser, x) => x.ChatLR = WndPoint.Parse(parser) },

            { "BeaconUL", (parser, x) => x.BeaconUL = WndPoint.Parse(parser) },
            { "BeaconLR", (parser, x) => x.BeaconLR = WndPoint.Parse(parser) },

            { "PowerBarUL", (parser, x) => x.PowerBarUL = WndPoint.Parse(parser) },
            { "PowerBarLR", (parser, x) => x.PowerBarLR = WndPoint.Parse(parser) },

            { "ExpBarForegroundImage", (parser, x) => x.ExpBarForegroundImage = parser.ParseAssetReference() },

            { "MoneyUL", (parser, x) => x.MoneyUL = WndPoint.Parse(parser) },
            { "MoneyLR", (parser, x) => x.MoneyLR = WndPoint.Parse(parser) },

            { "GenArrow", (parser, x) => x.GenArrow = parser.ParseAssetReference() },
            { "CommandMarkerImage", (parser, x) => x.CommandMarkerImage = parser.ParseAssetReference() },

            { "ImagePart", (parser, x) => x.ImageParts.Add(ControlBarImagePart.Parse(parser)) },
        };

        public string Name { get; private set; }

        public WndPoint ScreenCreationRes { get; private set; }
        public string Side { get; private set; }
        public string QueueButtonImage { get; private set; }
        public string RightHudImage { get; private set; }
        public WndColor CommandBarBorderColor { get; private set; }
        public WndColor BuildUpClockColor { get; private set; }
        public WndColor ButtonBorderBuildColor { get; private set; }
        public WndColor ButtonBorderActionColor { get; private set; }
        public WndColor ButtonBorderUpgradeColor { get; private set; }
        public WndColor ButtonBorderSystemColor { get; private set; }

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

        public WndPoint MinMaxUL { get; private set; }
        public WndPoint MinMaxLR { get; private set; }
        public WndPoint GeneralUL { get; private set; }
        public WndPoint GeneralLR { get; private set; }
        public WndPoint UAttackUL { get; private set; }
        public WndPoint UAttackLR { get; private set; }

        public WndPoint OptionsUL { get; private set; }
        public WndPoint OptionsLR { get; private set; }
        public WndPoint WorkerUL { get; private set; }
        public WndPoint WorkerLR { get; private set; }
        public WndPoint ChatUL { get; private set; }
        public WndPoint ChatLR { get; private set; }
        public WndPoint BeaconUL { get; private set; }
        public WndPoint BeaconLR { get; private set; }

        public WndPoint PowerBarUL { get; private set; }
        public WndPoint PowerBarLR { get; private set; }
        public string ExpBarForegroundImage { get; private set; }
        public WndPoint MoneyUL { get; private set; }
        public WndPoint MoneyLR { get; private set; }

        public string GenArrow { get; private set; }
        public string CommandMarkerImage { get; private set; }

        public List<ControlBarImagePart> ImageParts { get; } = new List<ControlBarImagePart>();
    }

    public sealed class ControlBarImagePart
    {
        internal static ControlBarImagePart Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ControlBarImagePart> FieldParseTable = new IniParseTable<ControlBarImagePart>
        {
            { "Position", (parser, x) => x.Position = WndPoint.Parse(parser) },
            { "Size", (parser, x) => x.Size = WndPoint.Parse(parser) },
            { "ImageName", (parser, x) => x.ImageName = parser.ParseAssetReference() },
            { "Layer", (parser, x) => x.Layer = parser.ParseInteger() }
        };

        public WndPoint Position { get; private set; }
        public WndPoint Size { get; private set; }
        public string ImageName { get; private set; }
        public int Layer { get; private set; }
    }
}

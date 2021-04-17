using System;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;

namespace OpenSage.Mods.Generals.Gui
{
    class CommandButtonUtils
    {
        public static void SetCommandButton(Button buttonControl, CommandButton commandButton, GeneralsControlBar controlBar)
        {
            buttonControl.BackgroundImage = buttonControl.Window.ImageLoader.CreateFromMappedImageReference(commandButton.ButtonImage);

            buttonControl.DisabledBackgroundImage = buttonControl.BackgroundImage?.WithGrayscale(true);

            buttonControl.BorderColor = GetBorderColor(commandButton.ButtonBorderType, controlBar.Scheme).ToColorRgbaF();
            buttonControl.BorderWidth = 1;

            buttonControl.HoverOverlayImage = controlBar.CommandButtonHover;
            buttonControl.PushedOverlayImage = controlBar.CommandButtonPush;
        }

        private static ColorRgba GetBorderColor(CommandButtonBorderType borderType, ControlBarScheme scheme)
        {
            switch (borderType)
            {
                case CommandButtonBorderType.None:
                    return ColorRgba.Transparent;

                case CommandButtonBorderType.Action:
                    return scheme.ButtonBorderActionColor;

                case CommandButtonBorderType.Build:
                    return scheme.ButtonBorderBuildColor;

                case CommandButtonBorderType.Upgrade:
                    return scheme.ButtonBorderUpgradeColor;

                case CommandButtonBorderType.System:
                    return scheme.ButtonBorderSystemColor;

                default:
                    throw new ArgumentOutOfRangeException(nameof(borderType));
            }
        }
    }
}

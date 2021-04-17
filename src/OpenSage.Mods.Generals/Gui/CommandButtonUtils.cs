using System;
using OpenSage.Gui;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;

namespace OpenSage.Mods.Generals.Gui
{
    class CommandButtonUtils
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void SetCommandButton(Button buttonControl, CommandButton commandButton, GeneralsControlBar controlBar)
        {
            buttonControl.BackgroundImage = buttonControl.Window.ImageLoader.CreateFromMappedImageReference(commandButton.ButtonImage);

            buttonControl.DisabledBackgroundImage = buttonControl.BackgroundImage?.WithGrayscale(true);

            buttonControl.BorderColor = GetBorderColor(commandButton.ButtonBorderType, controlBar.Scheme).ToColorRgbaF();
            buttonControl.BorderWidth = 1;

            buttonControl.HoverOverlayImage = controlBar.CommandButtonHover;
            buttonControl.PushedOverlayImage = controlBar.CommandButtonPush;

            var objectDefinition = commandButton.Object?.Value;
            buttonControl.SystemCallback = (control, message, context) =>
            {
                Logger.Debug($"Button callback: {control.Name}, {commandButton.Command}");
                Logger.Debug($"Relevant object: {objectDefinition?.Name}");

                CommandButtonCallback.HandleCommand(context.Game, commandButton, objectDefinition, false);
            };

            //buttonControl.InputCallback = (control, message, context) =>
            //{
            //    //TODO: fix the commandbutton description
            //    //var windowManager = buttonControl.Window.Game.Scene2D.WndWindowManager;
            //    //if (message.MessageType == WndWindowMessageType.MouseEnter)
            //    //{
            //    //    var popupDescription = windowManager.PushWindow("ControlBarPopupDescription.wnd");
            //    //    var parent = popupDescription.Controls.FindControl("ControlBarPopupDescription.wnd:Parent");
            //    //    var label = parent.Controls[0] as Label;
            //    //    label.Text = context.Game.ContentManager.TranslationManager.GetString(commandButton.DescriptLabel);
            //    //}
            //    //else if(message.MessageType == WndWindowMessageType.MouseExit)
            //    //{
            //    //    windowManager.PopWindow();
            //    //}
            //};
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

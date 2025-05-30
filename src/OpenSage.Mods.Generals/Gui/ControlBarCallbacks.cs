﻿using System.IO;
using System.Linq;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Logic.Orders;

namespace OpenSage.Mods.Generals.Gui;

[WndCallbacks]
public static class ControlBarCallbacks
{
    private const string GeneralsExpPointsWnd = "GeneralsExpPoints.wnd";

    public static void W3DCommandBarBackgroundDraw(Control control, DrawingContext2D drawingContext)
    {

    }

    public static void ControlBarSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
    {
        switch (message.MessageType)
        {
            case WndWindowMessageType.SelectedButton:
                switch (message.Element.Name)
                {
                    case "ControlBar.wnd:ButtonLarge":
                        ((GeneralsControlBar)context.Game.Scene2D.ControlBar).ToggleSize();
                        break;
                    case "ControlBar.wnd:ButtonOptions":
                        context.WindowManager.PushWindow(Path.Combine("Menus", "QuitMenu.wnd"));
                        break;
                    case "ControlBar.wnd:ButtonGeneral":
                        if (context.WindowManager.TopWindow?.Name == GeneralsExpPointsWnd)
                        {
                            GeneralsExpPointsCallbacks.SetWindow(null);
                            context.WindowManager.PopWindow();
                        }
                        else
                        {
                            var window = context.WindowManager.PushWindow(GeneralsExpPointsWnd);
                            window.Name = GeneralsExpPointsWnd;
                            GeneralsExpPointsCallbacks.SetWindow(window);
                        }

                        break;
                }
                break;
        }
    }

    public static void LeftHUDInput(Control control, WndWindowMessage message, ControlCallbackContext context)
    {
        if (message.MessageType != WndWindowMessageType.MouseDown
            && message.MessageType != WndWindowMessageType.MouseRightDown)
        {
            return;
        }

        var terrainPosition = context.Game.Scene3D.RadarDrawUtil.RadarToWorldSpace(
            message.MousePosition,
            control.ClientRectangle);

        // TODO: Fix left/right mouse handling
        // - If user has selected units, left mouse should move them and right mouse moves the camera
        // - If user has not selected units, both mouse buttons move the camera
        switch (message.MessageType)
        {
            case WndWindowMessageType.MouseDown: // Left mouse moves selected units
                var unit = context.Game.Scene3D.LocalPlayer.SelectedUnits.LastOrDefault();
                if (unit != null)
                {
                    // TODO: Duplicated in OrderGeneratorSystem
                    unit.OnLocalMove(context.Game.Audio);
                    var order = Order.CreateMoveOrder(context.Game.Scene3D.GetPlayerIndex(context.Game.Scene3D.LocalPlayer), terrainPosition);
                    context.Game.NetworkMessageBuffer.AddLocalOrder(order);
                }
                break;

            case WndWindowMessageType.MouseRightDown: // Right mouse moves camera.
                context.Game.Scene3D.TacticalView.LookAt(terrainPosition);
                break;
        }
    }

    public static void W3DLeftHUDDraw(Control control, DrawingContext2D drawingContext)
    {
        control.Window.Game.Scene3D.RadarDrawUtil.Draw(
            drawingContext,
            control.ClientRectangle);
    }
}

﻿using OpenSage.Gui.Apt;
using Veldrid;

namespace OpenSage.Diagnostics;

internal sealed class DiagnosticViewContext
{
    public IGame Game { get; }
    public GameWindow Window { get; }
    public ImGuiRenderer ImGuiRenderer { get; }

    public AptWindow SelectedAptWindow { get; set; }

    public object SelectedObject { get; set; }

    public DiagnosticViewContext(IGame game, GameWindow window, ImGuiRenderer imGuiRenderer)
    {
        Game = game;
        Window = window;
        ImGuiRenderer = imGuiRenderer;
    }
}

public interface IInspectable
{
    string Name { get; }

    void DrawInspector();
}

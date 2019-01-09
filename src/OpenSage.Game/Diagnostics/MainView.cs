using System;
using System.Collections.Generic;
using ImGuiNET;
using OpenSage.Logic;
using OpenSage.Mathematics;
using OpenSage.Network;

namespace OpenSage.Diagnostics
{
    internal sealed class MainView : DisposableBase
    {
        private readonly DiagnosticViewContext _context;
        private readonly List<DiagnosticView> _views;

        private bool _isVSyncEnabled;

        public MainView(DiagnosticViewContext context)
        {
            _context = context;

            _isVSyncEnabled = context.Game.GraphicsDevice.SyncToVerticalBlank;

            _views = new List<DiagnosticView>();

            void AddView(DiagnosticView view)
            {
                _views.Add(AddDisposable(view));
            }

            AddView(new GameView(context) { IsVisible = true });
            AddView(new MapSettingsView(context));
            AddView(new RenderSettingsView(context));
            AddView(new ScriptingView(context));
            AddView(new StringsView(context));
            AddView(new AptView(context));
            AddView(new WndView(context));
        }

        public void Draw(ref bool isGameViewFocused)
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Exit", "Alt+F4", false, true))
                    {
                        Environment.Exit(0);
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Jump"))
                {
                    if (ImGui.BeginMenu("Map"))
                    {
                        foreach (var mapCache in _context.Game.ContentManager.IniDataContext.MapCaches)
                        {
                            if (ImGui.MenuItem(mapCache.DisplayName))
                            {
                                _context.Game.StartGame(
                                    mapCache.Name,
                                    new EchoConnection(),
                                    new[]
                                    {
                                        new PlayerSetting("America", new ColorRgb(255, 0, 0)),
                                        new PlayerSetting("GLA", new ColorRgb(255, 255, 255)),
                                    },
                                    0);
                            }
                        }

                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Windows"))
                {
                    foreach (var view in _views)
                    {
                        if (ImGui.MenuItem(view.DisplayName, null, view.IsVisible, true))
                        {
                            view.IsVisible = true;

                            ImGui.SetWindowFocus(view.Name);
                        }
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Preferences"))
                {
                    bool isVSyncEnabled = _isVSyncEnabled;
                    if (ImGui.MenuItem("VSync", null, ref isVSyncEnabled, true))
                    {
                        SetVSync(isVSyncEnabled);
                    }
                    ImGui.EndMenu();
                }

                var fpsText = $"{ImGui.GetIO().Framerate:N2} FPS";
                var fpsTextSize = ImGui.CalcTextSize(fpsText).X;
                ImGui.SetCursorPosX(ImGui.GetWindowContentRegionWidth() - fpsTextSize);
                ImGui.Text(fpsText);

                ImGui.EndMainMenuBar();
            }

            foreach (var view in _views)
            {
                view.Draw(ref isGameViewFocused);
            }
        }

        private void SetVSync(bool isEnabled)
        {
            if (_isVSyncEnabled != isEnabled)
            {
                _isVSyncEnabled = isEnabled;
                _context.Game.GraphicsDevice.SyncToVerticalBlank = _isVSyncEnabled;
            }
        }
    }
}

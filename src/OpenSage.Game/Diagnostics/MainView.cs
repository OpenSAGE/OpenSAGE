using System;
using System.Collections.Generic;
using System.Numerics;
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

        public MainView(DiagnosticViewContext context)
        {
            _context = context;

            _views = new List<DiagnosticView>();

            void AddView(DiagnosticView view)
            {
                _views.Add(AddDisposable(view));
            }

            AddView(new GameView(context) { IsVisible = true });
            AddView(new AssetListView(context));
            AddView(new MapSettingsView(context));
            AddView(new RenderSettingsView(context));
            AddView(new ScriptingView(context));
            AddView(new StatisticsView(context));
            AddView(new StringsView(context));
            AddView(new AptView(context));
            AddView(new AptConstantsView(context));
            AddView(new AptGeometryView(context));
            AddView(new WndView(context));
            AddView(new GameLoopView(context));
        }

        private void DrawTimingControls()
        {
            var (playPauseText, playPauseColor) = _context.Game.IsLogicRunning
                ? ("Pause (F9)", new Vector4(0.980f, 0, 0.243f, 1))
                : ("Play (F9)", new Vector4(0.066f, 0.654f, 0.066f, 1));

            var buttonSize = new Vector2(80.0f, ImGui.GetWindowHeight());

            ImGui.SetCursorPosX(ImGui.GetWindowContentRegionWidth() - 250);
            ImGui.PushStyleColor(ImGuiCol.Button, playPauseColor);

            if (ImGui.Button(playPauseText, buttonSize))
            {
                _context.Game.IsLogicRunning = !_context.Game.IsLogicRunning;
            }

            ImGui.PopStyleColor();

            ImGui.SetCursorPosX(ImGui.GetWindowContentRegionWidth() - 160);

            if (!_context.Game.IsLogicRunning)
            {
                if (ImGui.Button("Step (F10)", buttonSize))
                {
                    _context.Game.Step();
                }
            }
            else
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                ImGui.Button("Step (F10)", buttonSize);
                ImGui.PopStyleVar();
            }
        }

        public void Draw(ref bool isGameViewFocused)
        {
            float menuBarHeight = 0;

            if (ImGui.BeginMainMenuBar())
            {
                menuBarHeight = ImGui.GetWindowHeight();

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
                            if (ImGui.MenuItem($"{mapCache.DisplayName} ({mapCache.Name})"))
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
                    var isVSyncEnabled = _context.Game.GraphicsDevice.SyncToVerticalBlank;
                    if (ImGui.MenuItem("VSync", null, ref isVSyncEnabled, true))
                    {
                        _context.Game.GraphicsDevice.SyncToVerticalBlank = isVSyncEnabled;
                    }
                    ImGui.EndMenu();
                }

                DrawTimingControls();

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

            var launcherImage = _context.Game.LauncherImage;
            if (launcherImage != null)
            {
                var launcherImageMaxSize = new Vector2(150, 150);

                var launcherImageSize = SizeF.CalculateSizeFittingAspectRatio(
                    new SizeF(launcherImage.Width, launcherImage.Height),
                    new Size((int) launcherImageMaxSize.X, (int) launcherImageMaxSize.Y));

                const int launcherImagePadding = 10;
                ImGui.SetNextWindowPos(new Vector2(
                    _context.Game.Window.ClientBounds.Width - launcherImageSize.Width - launcherImagePadding,
                    menuBarHeight + launcherImagePadding));

                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                ImGui.Begin("LauncherImage", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration);

                ImGui.Image(
                    _context.ImGuiRenderer.GetOrCreateImGuiBinding(_context.Game.GraphicsDevice.ResourceFactory, launcherImage),
                    new Vector2(launcherImageSize.Width, launcherImageSize.Height),
                    Vector2.Zero,
                    Vector2.One,
                    Vector4.One,
                    Vector4.Zero);

                ImGui.End();
                ImGui.PopStyleVar();
            }
        }
    }
}

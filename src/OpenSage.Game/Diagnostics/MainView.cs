using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Content;
using OpenSage.Content.Translation;
using OpenSage.Logic;
using OpenSage.Mathematics;
using OpenSage.Network;
using Veldrid;

namespace OpenSage.Diagnostics
{
    internal sealed class MainView : DisposableBase
    {
        private readonly DiagnosticViewContext _context;
        private readonly List<DiagnosticView> _views;
        private readonly IReadOnlyDictionary<MapCache, string> _maps;

        public MainView(DiagnosticViewContext context)
        {
            _context = context;

            _views = new List<DiagnosticView>();

            _maps = _context.Game.AssetStore.MapCaches
                .Where(m => _context.Game.ContentManager.GetMapEntry(m.Name) != null)
                .Select(m => (mapCache: m, mapName: m.GetNameKey().Translate()))
                .OrderBy(m => m.mapName)
                .ToDictionary(m => m.mapCache, m => m.mapName);

            void AddView(DiagnosticView view)
            {
                _views.Add(AddDisposable(view));
            }

            AddView(new GameView(context) { IsVisible = true });
            AddView(new ObjectListView(context));
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
            AddView(new GameSettingsView(context));
            AddView(new LuaScriptConsole(context));
            AddView(new LogView(context));
            AddView(new InspectorView(context));
            AddView(new PreviewView(context));
            AddView(new CameraView(context));
        }

        private void DrawTimingControls()
        {
            var (playPauseText, playPauseColor) = _context.Game.IsLogicRunning
                ? ("Pause (F9)", new Vector4(0.980f, 0, 0.243f, 1))
                : ("Play (F9)", new Vector4(0.066f, 0.654f, 0.066f, 1));

            var buttonSize = new Vector2(80.0f, 0);

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
            var viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(viewport.GetWorkPos());
            ImGui.SetNextWindowSize(viewport.GetWorkSize());
            ImGui.SetNextWindowViewport(viewport.ID);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

            var windowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking;
            windowFlags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
            windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
            //windowFlags |= ImGuiWindowFlags.NoBackground;

            ImGui.Begin("Root", windowFlags);

            ImGui.PopStyleVar(3);

            ImGui.DockSpace(ImGui.GetID("DockSpace"), Vector2.Zero, ImGuiDockNodeFlags.None);

            float menuBarHeight = 0;

            if (ImGui.BeginMenuBar())
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
                        foreach (var mapCache in _maps)
                        {                           
                            if (ImGui.MenuItem($"{mapCache.Value} ({mapCache.Key.Name})"))
                            {
                                var random = new Random();
                                var playableSides = _context.Game.GetPlayableSides().ToList();
                                var faction1 = playableSides[random.Next(0, playableSides.Count())];
                                var faction2 = playableSides[random.Next(0, playableSides.Count())];

                                if (mapCache.Key.IsMultiplayer)
                                {
                                    _context.Game.StartMultiPlayerGame(
                                        mapCache.Key.Name,
                                        new EchoConnection(),
                                        new PlayerSetting?[]
                                        {
                                            new PlayerSetting(null, faction1, new ColorRgb(255, 0, 0)),
                                            new PlayerSetting(null, faction2, new ColorRgb(255, 255, 255)),
                                        },
                                        0
                                    );
                                }
                                else
                                {
                                    _context.Game.StartSinglePlayerGame(mapCache.Key.Name);
                                }
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
                    var isFullscreen = _context.Game.Window.Fullscreen;
                    if (ImGui.MenuItem("Fullscreen", "Alt+Enter", ref isFullscreen, true))
                    {
                        _context.Game.Window.Fullscreen = isFullscreen;
                    }
                    ImGui.EndMenu();
                }

                if (_context.Game.Configuration.UseRenderDoc && ImGui.BeginMenu("RenderDoc"))
                {
                    var renderDoc = Game.RenderDoc;

                    if (ImGui.MenuItem("Trigger Capture"))
                    {
                        renderDoc.TriggerCapture();
                    }
                    if (ImGui.BeginMenu("Options"))
                    {
                        bool allowVsync = renderDoc.AllowVSync;
                        if (ImGui.Checkbox("Allow VSync", ref allowVsync))
                        {
                            renderDoc.AllowVSync = allowVsync;
                        }
                        bool validation = renderDoc.APIValidation;
                        if (ImGui.Checkbox("API Validation", ref validation))
                        {
                            renderDoc.APIValidation = validation;
                        }
                        int delayForDebugger = (int) renderDoc.DelayForDebugger;
                        if (ImGui.InputInt("Debugger Delay", ref delayForDebugger))
                        {
                            delayForDebugger = Math.Clamp(delayForDebugger, 0, int.MaxValue);
                            renderDoc.DelayForDebugger = (uint) delayForDebugger;
                        }
                        bool verifyBufferAccess = renderDoc.VerifyBufferAccess;
                        if (ImGui.Checkbox("Verify Buffer Access", ref verifyBufferAccess))
                        {
                            renderDoc.VerifyBufferAccess = verifyBufferAccess;
                        }
                        bool overlayEnabled = renderDoc.OverlayEnabled;
                        if (ImGui.Checkbox("Overlay Visible", ref overlayEnabled))
                        {
                            renderDoc.OverlayEnabled = overlayEnabled;
                        }
                        bool overlayFrameRate = renderDoc.OverlayFrameRate;
                        if (ImGui.Checkbox("Overlay Frame Rate", ref overlayFrameRate))
                        {
                            renderDoc.OverlayFrameRate = overlayFrameRate;
                        }
                        bool overlayFrameNumber = renderDoc.OverlayFrameNumber;
                        if (ImGui.Checkbox("Overlay Frame Number", ref overlayFrameNumber))
                        {
                            renderDoc.OverlayFrameNumber = overlayFrameNumber;
                        }
                        bool overlayCaptureList = renderDoc.OverlayCaptureList;
                        if (ImGui.Checkbox("Overlay Capture List", ref overlayCaptureList))
                        {
                            renderDoc.OverlayCaptureList = overlayCaptureList;
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.MenuItem("Launch Replay UI"))
                    {
                        renderDoc.LaunchReplayUI();
                    }
                  
                    ImGui.EndMenu();
                }

                DrawTimingControls();

                var fpsText = $"{ImGui.GetIO().Framerate:N2} FPS";
                var fpsTextSize = ImGui.CalcTextSize(fpsText).X;
                ImGui.SetCursorPosX(ImGui.GetWindowContentRegionWidth() - fpsTextSize);
                ImGui.Text(fpsText);

                ImGui.EndMenuBar();
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

            ImGui.End();
        }
    }
}

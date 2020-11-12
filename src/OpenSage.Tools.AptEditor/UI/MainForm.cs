using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data;
using OpenSage.Tools.AptEditor.UI.Widgets;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.UI
{
    internal sealed class MainForm
    {
        private readonly List<IWidget> _widgets = new List<IWidget>
        {
            new CharacterList(),
            new SceneTransform(),
            new FrameList(),
            new FrameItemList()
        };
        private readonly AptFileSelector _aptFileSelector;
        private readonly SearchPathAdder _searchPathAdder;
        private readonly List<ImGuiModalPopUp> _popups;
        private readonly GameWindow _window;
        private readonly AptSceneManager _manager;
        
        private string? _lastErrorMessageForModalPopUp;
        private string? _lastSeriousError;
        private double _lastFps;
        private DateTime _lastUpdate;
        private DateTime _lastFpsUpdate;

        public MainForm(Game game)
        {
            _window = game.Window;
            _manager = new AptSceneManager(game);
            _aptFileSelector = new AptFileSelector(game.ContentManager.FileSystem);
            _searchPathAdder = new SearchPathAdder(game);
            
            _popups = new List<ImGuiModalPopUp>
            {
                new ImGuiModalPopUp("CriticalErrorPrompt",
                                    () => _lastSeriousError != null,
                                    DrawCriticalErrorPrompt,
                                    ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar),
                new ImGuiModalPopUp("ErrorPrompt", () => _lastErrorMessageForModalPopUp != null, DrawErrorPrompt)
            };
            _lastUpdate = DateTime.Now;
        }

        public void Draw()
        {
            if ((DateTime.Now - _lastFpsUpdate).TotalMilliseconds > 300)
            {
                _lastFpsUpdate = DateTime.Now;
                _lastFps = 1000 / (DateTime.Now - _lastUpdate).TotalMilliseconds;
            }
            _lastUpdate = DateTime.Now;

            ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always, Vector2.Zero);
            ImGui.SetNextWindowSize(new Vector2(_window.ClientBounds.Width, 0), ImGuiCond.Always);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            var open = false;
            ImGui.Begin("OpenSAGE Apt Editor", ref open, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoTitleBar);

            foreach (var popup in _popups)
            {
                if (popup.Update())
                {
                    break;
                }
            }

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open...", "Ctrl+O", false, true))
                    {
                        _aptFileSelector.Visible = true;
                    }

                    if (ImGui.MenuItem("Close", null, false, _manager.AptManager != null))
                    {
                        _manager.UnloadApt();
                    }

                    if (ImGui.MenuItem("Add Search paths..."))
                    {
                        _searchPathAdder.Visible = true;
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Exit", "Alt+F4", false, true))
                    {
                        _window.Close();
                    }

                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }


            if (_aptFileSelector.GetValue() is string inputAptPath)
            {
                try
                {
                    _manager.LoadApt(inputAptPath);
                }
                catch (AptLoadFailure loadFailure)// when (!Debugger.IsAttached)
                {
                    _lastErrorMessageForModalPopUp =
                        $"Failed to open apt file {loadFailure.Message}.\n" +
                        "Consider adding more search paths (File Menu > Add Search Path).";
                }
                catch (Exception unhandleled) when (!Debugger.IsAttached)
                {
                    _lastSeriousError = unhandleled.Message + '\n' + unhandleled.StackTrace;
                }
            }

            if (_manager.AptManager == null)
            {
                ImGui.Text("Open a .apt file to see its contents.");
            }
            else
            {
                ImGui.Text($"Currently loaded Apt file: {_manager.CurrentAptPath}");
            }

            ImGui.SameLine(ImGui.GetWindowWidth() - 100);
            ImGui.Text($"FPS: {_lastFps,5:N2}");

            ImGui.End();

            try
            {
                _aptFileSelector.Draw();
                _searchPathAdder.Draw();
                if (_lastSeriousError == null && _lastErrorMessageForModalPopUp == null && _manager.AptManager != null)
                {
                    foreach (var widget in _widgets)
                    {
                        widget.Draw(_manager);
                    }
                }
            }
            catch (Exception unhandleled) when (!Debugger.IsAttached)
            {
                _lastSeriousError = unhandleled.Message + '\n' + unhandleled.StackTrace;
            }

            ImGui.PopStyleVar();
        }

        

        private void DrawErrorPrompt()
        {
            ImGui.TextColored(new Vector4(1, 0.2f, 0.2f, 1), _lastErrorMessageForModalPopUp);

            if (ImGui.Button("Ok"))
            {
                _lastErrorMessageForModalPopUp = null;
                ImGui.CloseCurrentPopup();
            }

            ImGui.SetItemDefaultFocus();
        }

        private void DrawCriticalErrorPrompt()
        {
            ImGui.TextColored(new Vector4(1, 0.2f, 0.2f, 1), "Unhandleled exception:");
            ImGui.Text(_lastSeriousError);
            ImGui.TextColored(new Vector4(1, 0.2f, 0.2f, 1), "Press Ok to exit");

            if (ImGui.Button("Ok"))
            {
                _lastSeriousError = null;
                ImGui.CloseCurrentPopup();
                _window.Close();
            }

            ImGui.SetItemDefaultFocus();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Gui.Apt;
using OpenSage.Tools.AptEditor.Apt;
using OpenSage.Tools.AptEditor.UI.Widgets;
using OpenSage.Tools.AptEditor.Util;
using Veldrid;
using Veldrid.Sdl2;

namespace OpenSage.Tools.AptEditor.UI
{
    internal sealed class MainForm : DisposableBase
    {
        private byte[] _filePathBuffer;
        private AptGameSystem _system;
        private AptSceneManager _manager;
        private List<IWidget> _widgets;
        private List<ImGuiUtility.ModalPopUp> _popUps;
        private bool _menuOpenClicked;
        private string _inputAptPath;
        private string _lastErrorMessageForModalPopUp;
        private string _lastSeriousError;
        private double _lastFps;
        private DateTime _lastUpdate;
        private DateTime _lastFpsUpdate;
        public MainForm(AptGameSystem system)
        {
            _filePathBuffer = new byte[1024];
            var defaultValue = System.Text.Encoding.UTF8.GetBytes("fe_shared_mainMenuLib.apt");
            defaultValue.CopyTo(_filePathBuffer, 0);

            _system = system;
            _manager = new AptSceneManager(system);
            _widgets = new List<IWidget>();
            _popUps = new List<ImGuiUtility.ModalPopUp>();

            _widgets.Add(new CharacterList());
            _widgets.Add(new SceneTransform());
            _widgets.Add(new FrameList());
            _widgets.Add(new FrameItemList());

            _popUps.Add(new ImGuiUtility.ModalPopUp("OpenAptFilePopUp", () => _menuOpenClicked, DrawOpenFileDialog));
            _popUps.Add(new ImGuiUtility.ModalPopUp("ErrorPrompt", () => _lastErrorMessageForModalPopUp != null, DrawErrorPrompt));
            _popUps.Add(new ImGuiUtility.ModalPopUp("CriticalErrorPrompt",
                                                    () => _lastSeriousError != null,
                                                    DrawCriticalErrorPrompt,
                                                    ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar));

            _lastUpdate = DateTime.Now;
        }

        public void Draw(CommandList commandList)
        {
            if((DateTime.Now - _lastFpsUpdate).TotalMilliseconds > 300)
            {
                _lastFpsUpdate = DateTime.Now;
                _lastFps = 1000 / (DateTime.Now - _lastUpdate).TotalMilliseconds;
            }
            _lastUpdate = DateTime.Now;

            ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always, Vector2.Zero);
            ImGui.SetNextWindowSize(new Vector2(_system.Game.Window.ClientBounds.Width, 0), ImGuiCond.Always);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            bool open = false;
            ImGui.Begin("OpenSAGE Apt Editor", ref open, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoTitleBar);


            _menuOpenClicked = false;


            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open...", "Ctrl+O", false, true))
                    {
                        _menuOpenClicked = true;
                    }

                    if (ImGui.MenuItem("Close", null, false, _manager.AptManager != null))
                    {
                        _manager.UnloadApt();
                    }

                    if (ImGui.MenuItem("Add Search paths..."))
                    {
                        
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Exit", "Alt+F4", false, true))
                    {
                        Environment.Exit(0);
                    }

                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }


            if(_inputAptPath != null)
            {
                try
                {
                    _manager.LoadApt(_inputAptPath);
                }
                catch(AptLoadFailure loadFailure)
                {
                    _lastErrorMessageForModalPopUp =
                        $"Failed to open apt file {loadFailure.Message}.\n" +
                        "Consider add more search paths (File Menu > Add Search Path).";
                }
                catch(Exception unhandleled)
                {
                    _lastSeriousError = unhandleled.Message + '\n' + unhandleled.StackTrace;
                }
                _inputAptPath = null;
            }

            if(_manager.AptManager == null)
            {
                ImGui.Text("Open a .apt file to see its contents.");
            }
            else
            {
                ImGui.Text($"Currently loaded Apt file: {_manager.CurrentAptPath}");
            }

            ImGui.SameLine(ImGui.GetWindowWidth() - 100);
            ImGui.Text($"FPS: {_lastFps, 5:N2}");

            ImGui.End();

            try
            {
                if(_lastSeriousError == null && _lastErrorMessageForModalPopUp == null && _manager.AptManager != null)
                {
                    _manager.Render(commandList);

                    foreach(var widget in _widgets)
                    {
                        widget.Draw(_manager);
                    }
                }
            }
            catch(Exception unhandleled)
            {
                _lastSeriousError = unhandleled.Message + '\n' + unhandleled.StackTrace;
            }

            foreach(var popUp in _popUps)
            {
                popUp.Update();
            }

            ImGui.PopStyleVar();
        }

        private void DrawOpenFileDialog()
        {
            ImGuiUtility.InputText("Apt File Path", _filePathBuffer, out var inputPath);

            if (ImGui.Button("Open"))
            {
                _inputAptPath = inputPath;
                ImGui.CloseCurrentPopup();
            }

            ImGui.SetItemDefaultFocus();

            ImGui.SameLine();

            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }
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
                Environment.Exit(1);
            }

            ImGui.SetItemDefaultFocus();
        }
    }
}

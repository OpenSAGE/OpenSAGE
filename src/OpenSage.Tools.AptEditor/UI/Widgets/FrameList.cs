using System;
using System.Numerics;
using ImGuiNET;
using OpenSage.Tools.AptEditor.UI;
using OpenSage.Tools.AptEditor.Apt.Editor;

namespace OpenSage.Tools.AptEditor.UI.Widgets
{
    internal class FrameList : IWidget
    {
        const string Name = "Frame List";
        private readonly FrameListUtilities _frameListUtilities;
        private int _inputFrameNumber;
        private DateTime _lastPlayUpdate;
        private bool _playing;

        public FrameList()
        {
            _frameListUtilities = new FrameListUtilities();
        }

        public void Draw(AptSceneManager manager)
        {
            if(_frameListUtilities.Reset(manager))
            {
                // if this is a different frame (imply _utilities.Active == true)
                ImGui.SetNextWindowSize(new Vector2(0, 0));
                _inputFrameNumber = (int)manager.CurrentFrame;
                _playing = false;
            }
            else if(!_frameListUtilities.Active)
            {
                return;
            }

            if(ImGui.Begin(Name))
            {
                ImGui.Text($"Current Frame: {manager.CurrentFrameWrapped}");
                ProcessPlay(manager);

                ImGui.Separator();

                ImGui.InputInt($" / {manager.NumberOfFrames}", ref _inputFrameNumber);
                _inputFrameNumber = Math.Abs(_inputFrameNumber);
                if(ImGui.Button("Play to frame"))
                {
                    manager.PlayToFrame(_inputFrameNumber);
                }

                ImGui.Separator();

                if(ImGui.BeginChild("Real Frame List"))
                {
                    var digits = 1;
                    if(manager.NumberOfFrames.Value > 10)
                    {
                        digits = (int)Math.Log10(manager.NumberOfFrames.Value - 1) + 1;
                    }
                    for (var i = 0; i < manager.NumberOfFrames; ++i)
                    {
                        var selected = (i == manager.CurrentFrameWrapped);
                        ImGui.Selectable($"Frame {i.ToString($"D{digits}")}", ref selected);
                        if(selected && i != manager.CurrentFrameWrapped)
                        {
                            manager.PlayToFrame(i);
                        }
                    }
                }
                ImGui.EndChild();

            }
            ImGui.End();
        }

        private void ProcessPlay(AptSceneManager manager)
        {
            if(!_playing)
            {
                if(ImGui.Button("Play"))
                {
                    _playing = true;
                    _lastPlayUpdate = DateTime.UtcNow;
                }
            }
            else
            {
                if(ImGui.Button("Stop"))
                {
                    _playing = false;
                }

                var interval = (DateTime.UtcNow - _lastPlayUpdate).TotalMilliseconds;
                for (var i = 0; i < interval; i += manager.MillisecondsPerFrame)
                {
                    ++_inputFrameNumber;
                    manager.PlayToFrame(_inputFrameNumber);
                    _lastPlayUpdate = DateTime.UtcNow;
                }
                
            }
            ImGui.TextWrapped("Played frames might differ from real ones because AptEditor won't execute any actionscript.");
        }
    }
}

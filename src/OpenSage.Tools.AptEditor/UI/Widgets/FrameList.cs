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
        private float _playSpeed;

        public FrameList()
        {
            _frameListUtilities = new FrameListUtilities();
        }

        public void Draw(AptSceneManager manager)
        {
            if (_frameListUtilities.Reset(manager))
            {
                // if this is a different frame (imply _utilities.Active == true)
                ImGui.SetNextWindowSize(Vector2.Zero);
                _inputFrameNumber = manager.CurrentFrame;
                _playing = false;
                _playSpeed = 1f;
            }
            else if (!_frameListUtilities.Active)
            {
                return;
            }

            if (ImGui.Begin(Name))
            {
                ImGui.InputFloat("Speed multiplier", ref _playSpeed);
                ImGui.Text($"Current Frame: {manager.CurrentFrameWrapped}");
                ProcessPlay(manager);

                ImGui.Separator();

                ImGui.InputInt($" / {manager.NumberOfFrames}", ref _inputFrameNumber);
                _inputFrameNumber = Math.Abs(_inputFrameNumber);
                if (ImGui.Button("Play to frame"))
                {
                    manager.PlayToFrame(_inputFrameNumber);
                }

                ImGui.Separator();

                if (ImGui.BeginChild("Real Frame List"))
                {
                    var digits = 1;
                    if (manager.NumberOfFrames > 10)
                    {
                        digits = (int) Math.Log10(manager.NumberOfFrames - 1) + 1;
                    }
                    for (var i = 0; i < manager.NumberOfFrames; ++i)
                    {
                        var selected = (i == manager.CurrentFrameWrapped);
                        ImGui.Selectable($"Frame {i.ToString($"D{digits}")}", ref selected);
                        if (selected && i != manager.CurrentFrameWrapped)
                        {
                            manager.PlayToFrame(i);
                            _inputFrameNumber = i;
                        }
                    }
                }
                ImGui.EndChild();

            }
            ImGui.End();
        }

        private void ProcessPlay(AptSceneManager manager)
        {
            if (!_playing)
            {
                if (ImGui.Button("Play"))
                {
                    _playing = true;
                    _lastPlayUpdate = DateTime.UtcNow;
                    if (manager.CurrentFrame != _inputFrameNumber)
                    {
                        manager.PlayToFrame(0);
                        _inputFrameNumber = 0;
                    }
                }
            }
            else
            {
                if (ImGui.Button("Stop"))
                {
                    _playing = false;
                }
            }

            var now = DateTime.UtcNow;
            var mspf = manager.MillisecondsPerFrame / _playSpeed;
            while ((now - _lastPlayUpdate).TotalMilliseconds >= mspf)
            {
                if (manager.CurrentFrame != _inputFrameNumber)
                {
                    _playing = false;
                }
                if (!_playing)
                {
                    break;
                }

                ++_inputFrameNumber;
                manager.NextFrame();
                _lastPlayUpdate += TimeSpan.FromMilliseconds(mspf);
            }

            ImGui.TextWrapped("Played frames might differ from real ones because AptEditor won't execute any actionscript.");
        }
    }
}

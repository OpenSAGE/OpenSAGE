using ImGuiNET;
using OpenSage.Audio;
using OpenSage.Data;
using SharpAudio.Util;

namespace OpenSage.Diagnostics.AssetViews
{
    internal sealed class SoundView : AssetView
    {
        private readonly SoundStream _source;
        private bool _playing;

        public SoundView(DiagnosticViewContext context, AudioFile audioFile)
            : base(context)
        {
            _source = AddDisposable(context.Game.Audio.GetStream(audioFile.Entry));
        }

        public override void Draw()
        {
            ImGui.Spacing();
            ImGui.Text("Channels: " + _source.Format.Channels);
            ImGui.Text("Bits per Sample: " + _source.Format.BitsPerSample);
            ImGui.Text("SampleRate: " + _source.Format.SampleRate);
            ImGui.Spacing();
            ImGui.Text("Duration: " + _source.Duration);
            var progress = (float) (_source.Position / _source.Duration);
            ImGui.SliderFloat("", ref progress, 0.0f, 1.0f, "Position", 1.0f);
            ImGui.Spacing();

            if (_playing)
            {
                // TODO: We should reset the stream at this point
                if (!_source.IsPlaying)
                {
                    _playing = false;
                }

                if (ImGui.Button("Stop"))
                {
                    _source.Stop();
                    _playing = false;
                }
            }
            else
            {
                if (ImGui.Button("Play"))
                {
                    _source.Play();
                    _playing = true;
                }
            }
        }
    }
}

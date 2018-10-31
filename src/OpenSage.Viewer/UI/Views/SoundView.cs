using ImGuiNET;
using OpenSage.Audio;
using SharpAudio;
using SharpAudio.Util;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class SoundView : AssetView
    {
        private readonly SoundStream _source;
        private bool _playing;

        public SoundView(AssetViewContext context)
        {
            _source = context.Game.Audio.GetStream(context.Entry.FilePath);

            AddDisposeAction(() => _source.Dispose());
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGui.Spacing();
            ImGui.Text("Channels: " + _source.Format.Channels);
            ImGui.Text("Bits per Sample: " + _source.Format.BitsPerSample);
            ImGui.Text("SampleRate: " + _source.Format.SampleRate);
            ImGui.Spacing();
            ImGui.Text("Duration: " + _source.Duration);
            ImGui.Spacing();

            if (_playing)
            {
                //We should reset the stream at this point
                if(!_source.IsPlaying)
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

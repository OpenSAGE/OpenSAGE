using System;
using System.Collections.Generic;
using System.Text;
using Eto.Forms;
using OpenSage.Audio;
using OpenSage.Data;
using OpenSage.Data.Wav;
using OpenSage.DataViewer.Controls;

namespace OpenSage.DataViewer.UI.Viewers
{
    class WavView : GameControl
    {
        WavFile _file;
        AudioSource _source;
        Game _game;

        public WavView(FileSystemEntry entry, Func<IntPtr, Game> createGame)
        {
            _file = WavFile.FromFileSystemEntry(entry);
            CreateGame = h =>
            {
                _game = createGame(h);

                var wavFile = _game.ContentManager.Load<AudioBuffer>(entry.FilePath);
                var source = _game.Audio.AddSource(wavFile, true);
                source.Play();

                return _game;
            };         
        }

        ~WavView()
        {
            _game.Audio.RemoveSource(_source);
        }
    }
}

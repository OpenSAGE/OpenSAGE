using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Gui.Apt;
using OpenSage.Mathematics;
using OpenSage.Tools.AptEditor.Apt;
using OpenSage.Tools.AptEditor.UI.SpriteItemExtensions;

namespace OpenSage.Tools.AptEditor.UI
{
    internal class AptLoadFailure : Exception
    {
        public AptLoadFailure(string message) : base(message) { }
    }

    internal class AptSceneManager : DisposableBase
    {
        public int MillisecondsPerFrame => (int) (_renderAptFile?.Movie.MillisecondsPerFrame ?? 30);
        public bool HasApt => AptManager != null;
        public Character? CurrentCharacter { get; private set; }
        public string? CurrentAptPath { get; private set; }
        public int? NumberOfFrames { get; private set; }
        public int? CurrentFrameWrapped => CurrentFrame % NumberOfFrames;
        public int? CurrentFrame { get; private set; }
        public Vector2 CurrentOffset { get; private set; }
        public float CurrentScale { get; private set; }
        public Game Game { get; }
        public AptObjectsManager? AptManager { get; private set; }
        private AptFile? _renderAptFile;
        private AptWindow? _currentWindow;

        public AptSceneManager(Game game)
        {
            UnloadApt();
            Game = game;
        }

        public void UnloadApt()
        {
            CurrentAptPath = null;
            NumberOfFrames = null;
            CurrentFrame = null;
            CurrentOffset = Vector2.Zero;
            CurrentScale = 1;
            AptManager = null;
            _renderAptFile = null;
            _currentWindow = null;
        }

        public void LoadApt(string path)
        {
            UnloadApt();
            var entry = Game.ContentManager.FileSystem.GetFile(path);
            if (entry == null)
            {
                throw new AptLoadFailure(path);
            }

            AptFile aptFile;
            try
            {
                aptFile = AptFile.FromFileSystemEntry(entry);
            }
            catch (FileNotFoundException fileNotFound)
            {
                throw new AptLoadFailure(fileNotFound.Message);
            }

            CurrentAptPath = path;
            AptManager = new AptObjectsManager(aptFile);
            _renderAptFile = AptManager.AptFile.ShallowClone(true);
            _renderAptFile.Movie.Frames.Clear();
        }

        public void SetCharacter(Character character)
        {
            if(_renderAptFile is null)
            {
                throw new InvalidOperationException();
            }

            CopyFrames(_renderAptFile, character);
            NumberOfFrames = _renderAptFile.Movie.Frames.Count;

            var manager = Game.Scene2D.AptWindowManager;
            while (manager.OpenWindowCount > 0)
            {
                manager.PopWindow();
            }
            manager.PushWindow(AptEditorBackgroundSource.CreateBackgroundAptWindow(Game, ColorRgba.DimGray));
            _currentWindow = new AptWindow(Game, Game.ContentManager, _renderAptFile);
            _currentWindow.Root.Create(character, _currentWindow.Context);
            manager.PushWindow(_currentWindow);

            CurrentCharacter = character;
            CurrentFrame = 0;
            Transform(0.5f, new Vector2(200, 200));
            if (NumberOfFrames > 0)
            {
                PlayToFrame(0);
            }
        }

        public void PlayToFrame(int frame)
        {

            if (!CurrentFrame.HasValue)
            {
                throw new InvalidOperationException();
            }

            if (_currentWindow == null)
            {
                throw new InvalidOperationException();
            }

            _currentWindow.Root.PlayToFrameNoActions(frame);
        }

        public void Transform(float scale, Vector2 offset)
        {
        }

        public void SubmitError(string message)
        {
            throw new NotImplementedException();
        }

        static private void CopyFrames(AptFile renderAptFile, Character character)
        {
            List<Frame> frames;
            switch (character)
            {
                case Playable playable:
                    frames = playable.Frames;
                    break;
                default:
                    var characterIndex = renderAptFile.Movie.Characters.IndexOf(character);
                    if (characterIndex == -1)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    var placeObject = PlaceObject.CreatePlace(1, characterIndex);
                    var frameItems = new List<FrameItem>
                    {
                        placeObject
                    };
                    frames = new List<Frame>
                    {
                        Frame.Create(frameItems)
                    };
                    break;
            }

            renderAptFile.Movie.Frames.Clear();
            renderAptFile.Movie.Frames.AddRange(frames);
        }
    }
}

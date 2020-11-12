using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public string? File { get; }
        public AptLoadFailure(string? file) : base(file is null ? "Apt Load failed" : $"Cannot load {file}")
        {
            File = file;
        }
    }

    internal class AptSceneManager
    {
        public int MillisecondsPerFrame => (int) (AptManager?.AptFile.Movie.MillisecondsPerFrame ?? 30);
        public Character? CurrentCharacter { get; private set; }
        public string? CurrentAptPath { get; private set; }
        public int NumberOfFrames { get; private set; }
        public int? CurrentFrameWrapped => NumberOfFrames == 0 ? new int?() : CurrentFrame % NumberOfFrames;
        public int CurrentFrame { get; private set; }
        public Vector2 CurrentOffset
        {
            get => _currentWindow?.WindowTransform.GeometryTranslation ?? Vector2.Zero;
            set => SetTransform((ref ItemTransform t) => t.GeometryTranslation = value);
        }
        public float CurrentScale
        {
            get => _currentWindow?.WindowTransform.GeometryRotation.M11 ?? 1;
            set => SetTransform((ref ItemTransform t) => t.GeometryRotation = Matrix3x2.CreateScale(value));
        }
        public ColorRgbaF DisplayBackgroundColor { get; private set; }
        public Game Game { get; }
        public AptObjectsManager? AptManager { get; private set; }
        private AptWindow? _currentWindow;

        public AptSceneManager(Game game)
        {
            UnloadApt();
            Game = game;
        }

        public void UnloadApt()
        {
            CurrentAptPath = null;
            NumberOfFrames = 0;
            CurrentFrame = 0;
            CurrentOffset = Vector2.Zero;
            CurrentScale = 1;
            AptManager = null;
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
                throw new AptLoadFailure(fileNotFound.FileName);
            }

            CurrentAptPath = path;
            AptManager = new AptObjectsManager(aptFile);
        }

        public void ChangeDisplayBackgroundColor(ColorRgbaF newColor)
        {
            if (DisplayBackgroundColor == newColor)
            {
                return;
            }

            DisplayBackgroundColor = newColor;
            ResetAptWindow();
        }

        public void ResetAptWindow()
        {
            _currentWindow = null;

            var windows = Game.Scene2D.AptWindowManager;
            while (windows.OpenWindowCount > 0)
            {
                windows.PopWindow();
            }

            var multiplied = DisplayBackgroundColor.ToVector4() * 255;
            var color = new ColorRgba((byte) multiplied.X, (byte) multiplied.Y, (byte) multiplied.Z, (byte) multiplied.W);
            windows.PushWindow(AptEditorBackgroundSource.CreateBackgroundAptWindow(Game, color));
            if (AptManager != null)
            {
                _currentWindow = new AptWindow(Game, Game.ContentManager, AptManager.AptFile);
                windows.PushWindow(_currentWindow);
                _currentWindow.Root.PlayToFrameNoActions(0);
            }
        }

        public void SetCharacter(Character character)
        {
            if (AptManager is null)
            {
                throw new InvalidOperationException();
            }
            ShowCharacter(character);
            NumberOfFrames = AptManager.AptFile.Movie.Frames.Count;
            ResetAptWindow();

            CurrentCharacter = character;
            CurrentFrame = 0;
            CurrentScale = 0.5f;
            CurrentOffset = new Vector2(200, 200);
            if (NumberOfFrames > 0)
            {
                PlayToFrame(0);
            }
        }

        public void PlayToFrame(int frame)
        {
            if (_currentWindow == null)
            {
                throw new InvalidOperationException();
            }

            CurrentFrame = frame;
            _currentWindow.Root.PlayToFrameNoActions(CurrentFrame);
        }

        public void NextFrame()
        {
            if (_currentWindow == null)
            {
                throw new InvalidOperationException();
            }

            ++CurrentFrame;
            _currentWindow.Root.UpdateNextFrameNoActions();
        }

        public void SubmitError(string message)
        {
            throw new NotImplementedException();
        }

        private delegate void TransformAction(ref ItemTransform t);
        private void SetTransform(TransformAction action)
        {
            if (_currentWindow == null)
            {
                return;
            }

            action(ref _currentWindow.WindowTransform);
        }

        /// <summary>
        /// Display the <paramref name="character"/> by replacing frames in
        /// the current <see cref="AptFile.Movie"/> with character's frames.
        /// <br/>
        /// If <paramref name="character"/> isn't a <see cref="Playable"/>,
        /// i.e. it doesn't have <see cref="Playable.Frames"/>,
        /// then a <see cref="Frame"/> containing the character as the
        /// <see cref="FrameItem"/> will be automatically created.
        /// </summary>
        /// <param name="character">
        /// The <see cref="Character"/> which needs to be displayed.
        /// </param>
        private void ShowCharacter(Character character)
        {
            var movie = AptManager!.AptFile.Movie;
            if (ReferenceEquals(character, movie))
            {
                movie.Frames.Clear();
                movie.Frames.AddRange(AptManager.RealMovieFrames);
            }

            List<Frame> frames;
            switch (character)
            {
                case Playable playable:
                    frames = playable.Frames.ToList();
                    break;
                default:
                    var characterIndex = movie.Characters.IndexOf(character);
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

            movie.Frames.Clear();
            movie.Frames.AddRange(frames);
        }
    }
}

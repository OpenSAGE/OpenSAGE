using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Mathematics;
using OpenSage.Tools.AptEditor.Apt;
using Veldrid;

namespace OpenSage.Tools.AptEditor.UI
{
    internal class AptRendererWrapper
    {
        public ColorRgbaF Background;
        public SpriteItem Sprite { get; private set; }
        public ItemTransform Transform;
        private AptRenderer _renderer;
        private DrawingContext2D _drawingContext;
        private Sampler _linearClampSampler;
        private float _aspectRatio;
        public AptRendererWrapper(AptFile aptFile, ContentManager contentManager, in Size newWindowSize)
        {
            Background = ColorRgbaF.Transparent;

            var aptContext = new AptContext(aptFile, contentManager);
            Sprite = new SpriteItem();
            Sprite.Create(aptFile.Movie, aptContext);
            aptContext.Root = Sprite;

            Transform = ItemTransform.None;
            Transform.ColorTransform = ColorRgbaF.White;
            Sprite.Transform = Transform;

            _renderer = new AptRenderer(contentManager);

            var outputDescription = contentManager.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription;
            _drawingContext = new DrawingContext2D(contentManager, BlendStateDescription.SingleAlphaBlend, outputDescription);

            _linearClampSampler = contentManager.LinearClampSampler;

            _aspectRatio = (float)aptFile.Movie.ScreenWidth / aptFile.Movie.ScreenHeight;
        }

        public Size CalculateAptSceneSize(in Size windowSize)
        {
            var expectedWidth = (int)(windowSize.Height * _aspectRatio);
            if(expectedWidth <= windowSize.Width)
            {
                return new Size(expectedWidth, windowSize.Height);
            }
            else
            {
                return new Size(windowSize.Width, (int)(windowSize.Width / _aspectRatio));
            }
        }

        public void Render(CommandList commandList, in Size windowSize)
        {

            var aptSceneSize = CalculateAptSceneSize(windowSize);

            _renderer.Resize(aptSceneSize);

            _drawingContext.Begin(commandList, _linearClampSampler, new SizeF(windowSize.Width, windowSize.Height));

            _drawingContext.FillRectangle(new Mathematics.Rectangle(Point2D.Zero, aptSceneSize), Background);

            Sprite.Render(_renderer, Transform, _drawingContext);

            _drawingContext.End();
        }
    }

    internal class AptLoadFailure : Exception
    {
        public AptLoadFailure(string message) : base(message) { }
    }

    internal class AptSceneManager : DisposableBase
    {
        public bool HasApt => AptManager != null;
        public Character CurrentCharacter { get; private set; }
        public bool IsActive => _currentRenderer != null;
        public string CurrentAptPath { get; private set; }
        public uint? NumberOfFrames { get; private set; }
        public uint? CurrentFrameWrapped => CurrentFrame % NumberOfFrames;
        public uint? CurrentFrame { get; private set; }
        public Vector2 CurrentOffset { get; private set; }
        public float CurrentScale { get; private set; }
        public ColorRgbaF DisplayBackgroundColor { 
            get { return (_currentRenderer?.Background).GetValueOrDefault(ColorRgbaF.Transparent); }
            set { if (_currentRenderer != null) { _currentRenderer.Background = value; } }
        }
        public AptGameSystem System;
        public AptObjectsManager AptManager { get; private set; }
        private AptFile _renderAptFile;
        private AptRendererWrapper _currentRenderer;

        public AptSceneManager(AptGameSystem system)
        {
            UnloadApt();
            System = system;
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
            _currentRenderer = null;
        }

        public void LoadApt(string path)
        {
            UnloadApt();
            var entry = System.ContentManager.FileSystem.GetFile(path);
            if(entry == null)
            {
                throw new AptLoadFailure(path);
            }

            var aptFile = (AptFile)null;
            try
            {
                
                aptFile = AptFile.FromFileSystemEntry(entry);
            }
            catch(FileNotFoundException fileNotFound)
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
            CopyFrames(_renderAptFile, character);
            NumberOfFrames = (uint)_renderAptFile.Movie.Frames.Count;
            _currentRenderer = new AptRendererWrapper(_renderAptFile, System.ContentManager, System.Game.Window.ClientBounds.Size);
            CurrentCharacter = character;
            CurrentFrame = 0;
            Transform(0.5f, new Vector2(200, 200));
            if(NumberOfFrames > 0)
            {
                PlayToFrame(0);
            }
        }

        public void PlayToFrame(uint frame)
        {
            if(!CurrentFrame.HasValue)
            {
                throw new InvalidOperationException();
            }

            if(_currentRenderer == null)
            {
                throw new InvalidOperationException();
            }

            _currentRenderer.Sprite.PlayToFrameNoActions(frame);
            CurrentFrame = frame;
        }

        public void SwitchToFrame(uint frame)
        {
            if(!CurrentFrame.HasValue)
            {
                throw new InvalidOperationException();
            }

            if(_currentRenderer == null)
            {
                throw new InvalidOperationException();
            }

            _currentRenderer.Sprite.SwitchToFrameNoActions(frame);
            CurrentFrame = frame;
            //_currentWindow.Root.GotoFrame(frame);
        }

        public void Transform(float scale, Vector2 offset)
        {
            if(_currentRenderer != null)
            {
                // Currently scaling does not work correctly
                // _currentRenderer.Transform.GeometryRotation = Matrix3x2.CreateScale(scale);

                _currentRenderer.Transform.GeometryTranslation = offset;
                // CurrentScale = scale;

                // Actually offset might also have problem,
                // but we need it, otherwise we can't see anything outside Movie's bounds
                // so keep it
                CurrentOffset = offset;
            }
        }

        public void SubmitError(string message)
        {
            throw new NotImplementedException();
        }

        public void Render(CommandList commandList) => _currentRenderer?.Render(commandList, System.Game.Window.ClientBounds.Size);

        static private void CopyFrames(AptFile renderAptFile, Character character)
        {
            List<Frame> frames = null;
            switch(character)
            {
                case Playable playable:
                    frames = playable.Frames;
                    break;
                default:
                    var characterIndex = renderAptFile.Movie.Characters.IndexOf(character);
                    if(characterIndex == -1)
                    {
                        throw new System.IndexOutOfRangeException();
                    }
                    var placeObject = PlaceObject.CreatePlace(1, characterIndex);
                    var frameItems = new List<FrameItem>();
                    frameItems.Add(placeObject);
                    frames = new List<Frame>();
                    frames.Add(Frame.Create(frameItems));
                    break;
            }

            renderAptFile.Movie.Frames.Clear();
            renderAptFile.Movie.Frames.AddRange(frames);
        }
    }
}
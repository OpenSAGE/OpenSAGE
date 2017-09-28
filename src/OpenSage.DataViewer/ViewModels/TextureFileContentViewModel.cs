using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.DataViewer.Framework;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class TextureFileContentViewModel : FileContentViewModel
    {
        private readonly Game _game;
        private readonly SpriteComponent _spriteComponent;

        public int TextureWidth => _spriteComponent.Texture.Width;
        public int TextureHeight => _spriteComponent.Texture.Height;

        public IEnumerable<uint> MipMapLevels => Enumerable
            .Range(0, _spriteComponent.Texture.MipMapCount)
            .Select(x => (uint) x);

        public uint SelectedMipMapLevel
        {
            get { return _spriteComponent.SelectedMipMapLevel; }
            set
            {
                _spriteComponent.SelectedMipMapLevel = value;
                NotifyOfPropertyChange();
            }
        }

        public TextureFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            _spriteComponent = new SpriteComponent();

            var graphicsDevice = IoC.Get<GraphicsDeviceManager>().GraphicsDevice;

            _game = new Game(graphicsDevice, File.FileSystem);

            _spriteComponent.Texture = AddDisposable(_game.ContentManager.Load<Texture>(
                File.FilePath,
                uploadBatch: null,
                options: new TextureLoadOptions
                {
                    GenerateMipMaps = false
                }));

            var scene = new Scene();

            var entity = new Entity();
            scene.Entities.Add(entity);

            entity.Components.Add(new PerspectiveCameraComponent());

            entity.Components.Add(_spriteComponent);

            _game.Scene = scene;
        }

        public void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            _game.Initialize(swapChain);
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            _game.Tick();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class TextureFileContentViewModel : FileContentViewModel
    {
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
            _spriteComponent = new SpriteComponent
            {
                Texture = Game.ContentManager.Load<Texture>(
                    File.FilePath,
                    uploadBatch: null,
                    options: new TextureLoadOptions
                    {
                        GenerateMipMaps = false
                    })
            };

            var scene = new Scene();

            var entity = new Entity();
            entity.Components.Add(new PerspectiveCameraComponent());
            entity.Components.Add(_spriteComponent);
            scene.Entities.Add(entity);

            Game.Scene = scene;
        }

        public void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            Game.SetSwapChain(swapChain);
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            Game.Tick();
        }
    }
}

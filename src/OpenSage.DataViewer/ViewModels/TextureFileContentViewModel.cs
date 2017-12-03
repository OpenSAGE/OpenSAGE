using System.Collections.Generic;
using System.Linq;
using LL.Graphics3D;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class TextureFileContentViewModel : FileContentViewModel, IGameViewModel
    {
        private SpriteComponent _spriteComponent;

        public int TextureWidth => _spriteComponent?.Texture.Width ?? 0;
        public int TextureHeight => _spriteComponent?.Texture.Height ?? 0;

        public IEnumerable<uint> MipMapLevels
        {
            get
            {
                if (_spriteComponent == null)
                    return Enumerable.Empty<uint>();

                return Enumerable
                    .Range(0, _spriteComponent.Texture.MipMapCount)
                    .Select(x => (uint) x);
            }
        }

        public uint SelectedMipMapLevel
        {
            get { return _spriteComponent?.SelectedMipMapLevel ?? 0; }
            set
            {
                _spriteComponent.SelectedMipMapLevel = value;
                NotifyOfPropertyChange();
            }
        }

        public TextureFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
        }

        void IGameViewModel.LoadScene(Game game)
        {
            _spriteComponent = new SpriteComponent
            {
                Texture = game.ContentManager.Load<Texture>(
                    File.FilePath,
                    options: new TextureLoadOptions
                    {
                        GenerateMipMaps = false
                    })
            };

            var scene = new Scene();

            var entity = new Entity();
            entity.Components.Add(_spriteComponent);
            scene.Entities.Add(entity);

            game.Scene = scene;

            NotifyOfPropertyChange(nameof(TextureWidth));
            NotifyOfPropertyChange(nameof(TextureHeight));
            NotifyOfPropertyChange(nameof(MipMapLevels));
            NotifyOfPropertyChange(nameof(SelectedMipMapLevel));
        }
    }
}

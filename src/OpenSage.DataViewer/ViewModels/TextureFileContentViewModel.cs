using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.DataViewer.Framework;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Effects;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class TextureFileContentViewModel : FileContentViewModel
    {
        private readonly Game _game;
        private readonly SpriteComponent _spriteComponent;
        private readonly Texture _texture;

        public int TextureWidth => _texture.Width;
        public int TextureHeight => _texture.Height;

        public IEnumerable<uint> MipMapLevels => Enumerable
            .Range(0, _texture.MipMapCount)
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

            _texture = AddDisposable(_game.ContentManager.Load<Texture>(
                File.FilePath,
                uploadBatch: null,
                options: new TextureLoadOptions
                {
                    GenerateMipMaps = false
                }));

            _spriteComponent.TextureView = AddDisposable(ShaderResourceView.Create(graphicsDevice, _texture));

            var scene = new Scene();

            var entity = new Entity();
            scene.Entities.Add(entity);

            entity.Components.Add(new PerspectiveCameraComponent
            {
                
            });

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

        private sealed class SpriteComponent : RenderableComponent
        {
            private SpriteEffect _effect;

            private EffectPipelineStateHandle _pipelineStateHandle;

            public ShaderResourceView TextureView { get; set; }
            public uint SelectedMipMapLevel { get; set; }

            protected override void Start()
            {
                base.Start();

                _effect = ContentManager.GetEffect<SpriteEffect>();

                var rasterizerState = RasterizerStateDescription.CullBackSolid;
                rasterizerState.IsFrontCounterClockwise = false;

                _pipelineStateHandle = new EffectPipelineState(
                    rasterizerState,
                    DepthStencilStateDescription.None,
                    BlendStateDescription.Opaque)
                    .GetHandle();
            }

            protected override void Render(CommandEncoder commandEncoder)
            {
                _effect.Begin(commandEncoder);

                _effect.SetPipelineState(_pipelineStateHandle);
                _effect.SetTexture(TextureView);
                _effect.SetMipMapLevel(SelectedMipMapLevel);

                _effect.Apply(commandEncoder);

                commandEncoder.Draw(PrimitiveType.TriangleList, 0, 3);
            }
        }
    }
}

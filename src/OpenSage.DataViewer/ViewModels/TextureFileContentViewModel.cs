using System.Collections.Generic;
using System.Linq;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Graphics.Effects;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class TextureFileContentViewModel : FileContentViewModel
    {
        private SpriteEffect _spriteEffect;
        private Texture _texture;
        private ShaderResourceView _textureView;
        private EffectPipelineStateHandle _pipelineStateHandle;

        public int TextureWidth => _texture?.Width ?? 0;
        public int TextureHeight => _texture?.Height ?? 0;

        public IEnumerable<uint> MipMapLevels => Enumerable
            .Range(0, _texture?.MipMapCount ?? 0)
            .Select(x => (uint) x);

        private uint _selectedMipMapLevel;
        public uint SelectedMipMapLevel
        {
            get { return _selectedMipMapLevel; }
            set
            {
                _selectedMipMapLevel = value;
                NotifyOfPropertyChange();
            }
        }

        public TextureFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
        }

        public void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            var contentManager = AddDisposable(new ContentManager(File.FileSystem, graphicsDevice));

            _texture = AddDisposable(contentManager.Load<Texture>(
                File.FilePath,
                uploadBatch: null, 
                options: new TextureLoadOptions
                {
                    GenerateMipMaps = false
                }));

            NotifyOfPropertyChange(nameof(TextureWidth));
            NotifyOfPropertyChange(nameof(TextureHeight));
            NotifyOfPropertyChange(nameof(MipMapLevels));

            _spriteEffect = AddDisposable(new SpriteEffect(graphicsDevice));

            _textureView = AddDisposable(ShaderResourceView.Create(graphicsDevice, _texture));

            var rasterizerState = RasterizerStateDescription.CullBackSolid;
            rasterizerState.IsFrontCounterClockwise = false;

            _pipelineStateHandle = new EffectPipelineState(
                rasterizerState,
                DepthStencilStateDescription.None,
                BlendStateDescription.Opaque)
                .GetHandle();
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            var renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.SetRenderTargetDescriptor(
                swapChain.GetNextRenderTarget(),
                LoadAction.Clear,
                new ColorRgba(0.5f, 0.5f, 0.5f, 1));

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            _spriteEffect.Begin(commandEncoder);

            _spriteEffect.SetPipelineState(_pipelineStateHandle);
            _spriteEffect.SetTexture(_textureView);
            _spriteEffect.SetMipMapLevel(_selectedMipMapLevel);

            _spriteEffect.Apply(commandEncoder);

            commandEncoder.SetViewport(new Viewport
            {
                X = 0,
                Y = 0,
                Width = swapChain.BackBufferWidth,
                Height = swapChain.BackBufferHeight,
                MinDepth = 0,
                MaxDepth = 1
            });

            commandEncoder.Draw(PrimitiveType.TriangleList, 0, 3);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }
    }
}

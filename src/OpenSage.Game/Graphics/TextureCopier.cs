using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics
{
    internal sealed class TextureCopier : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly CommandList _commandList;
        private readonly SpriteBatch _intermediateSpriteBatch;

        public TextureCopier(Game game, in OutputDescription outputDescription)
        {
            _graphicsDevice = game.GraphicsDevice;

            _commandList = AddDisposable(_graphicsDevice.ResourceFactory.CreateCommandList());

            _intermediateSpriteBatch = AddDisposable(new SpriteBatch(
                game.ContentManager,
                BlendStateDescription.SingleDisabled,
                outputDescription));
        }

        public void Execute(Texture source, Framebuffer destination)
        {
            _commandList.Begin();

            _commandList.PushDebugGroup("Blitting to framebuffer");

            _commandList.SetFramebuffer(destination);

            _intermediateSpriteBatch.Begin(
                _commandList,
                _graphicsDevice.PointSampler,
                new SizeF(source.Width, source.Height),
                ignoreAlpha: true);

            _intermediateSpriteBatch.DrawImage(
                source,
                null,
                new RectangleF(0, 0, (int) source.Width, (int) source.Height),
                ColorRgbaF.White);

            _intermediateSpriteBatch.End();

            _commandList.PopDebugGroup();

            _commandList.End();

            _graphicsDevice.SubmitCommands(_commandList);
        }
    }
}

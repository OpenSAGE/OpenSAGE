using System;

namespace OpenZH.Graphics
{
    public sealed partial class ResourceUploadBatch
    {
        private bool _inBeginEndBlock;

        public ResourceUploadBatch(GraphicsDevice graphicsDevice)
        {
            PlatformConstruct(graphicsDevice);
        }

        public void Begin()
        {
            if (_inBeginEndBlock)
            {
                throw new InvalidOperationException();
            }

            PlatformBegin();

            _inBeginEndBlock = true;
        }

        public void End(CommandQueue commandQueue)
        {
            if (!_inBeginEndBlock)
            {
                throw new InvalidOperationException();
            }

            PlatformEnd(commandQueue);

            _inBeginEndBlock = false;
        }
    }
}

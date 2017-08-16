using System;

namespace LLGfx
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

        public void End()
        {
            if (!_inBeginEndBlock)
            {
                throw new InvalidOperationException();
            }

            PlatformEnd();

            _inBeginEndBlock = false;
        }
    }
}

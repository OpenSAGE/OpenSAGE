using System.Collections.Generic;
using LLGfx;

namespace OpenSage.Graphics.Util
{
    public sealed class PipelineStateCache : GraphicsObject
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<PipelineStateDescription, PipelineState> _cachedStates;

        public PipelineStateCache(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _cachedStates = new Dictionary<PipelineStateDescription, PipelineState>();
        }

        public PipelineState GetPipelineState(PipelineStateDescription description)
        {
            if (!_cachedStates.TryGetValue(description, out var result))
            {
                _cachedStates[description] = result = AddDisposable(new PipelineState(_graphicsDevice, description));
            }
            return result;
        }
    }
}

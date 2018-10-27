using OpenSage.Graphics.Shaders;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    internal sealed class EffectParameter : DisposableBase
    {
        private readonly uint _slot;

        private ResourceSet _data;

        private bool _dirty;

        public string Name => ResourceBinding.Name;

        public ResourceBinding ResourceBinding { get; }
        public ResourceLayout ResourceLayout { get; }

        public EffectParameter(
            GraphicsDevice graphicsDevice,
            ResourceBinding resourceBinding,
            in ResourceLayoutElementDescription layoutDescription,
            uint slot)
        {
            _slot = slot;

            ResourceBinding = resourceBinding;

            var description = new ResourceLayoutDescription(layoutDescription);

            ResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                ref description));
        }

        public void SetData(ResourceSet resourceSet)
        {
            if (ReferenceEquals(_data, resourceSet))
            {
                return;
            }

            _data = resourceSet;
            _dirty = true;
        }

        internal void SetDirty()
        {
            _dirty = true;
        }

        public void ApplyChanges(CommandList commandEncoder)
        {
            if (!_dirty)
            {
                return;
            }

            if (_data != null)
            {
                commandEncoder.SetGraphicsResourceSet(_slot, _data);
            }

            _dirty = false;
        }
    }
}

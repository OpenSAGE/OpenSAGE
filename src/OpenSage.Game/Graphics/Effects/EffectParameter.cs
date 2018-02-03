using Veldrid;

namespace OpenSage.Graphics.Effects
{
    internal sealed class EffectParameter : DisposableBase
    {
        private readonly ResourceLayoutElementDescription _layoutDescription;
        private readonly uint _slot;

        private ResourceSet _data;

        private bool _dirty;

        public string Name => _layoutDescription.Name;

        public ResourceLayout ResourceLayout { get; }

        public EffectParameter(GraphicsDevice graphicsDevice, in ResourceLayoutElementDescription layoutDescription, uint slot)
        {
            _layoutDescription = layoutDescription;
            _slot = slot;

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

            commandEncoder.SetGraphicsResourceSet(_slot, _data);

            _dirty = false;
        }
    }
}

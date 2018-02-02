using Veldrid;

namespace OpenSage.Graphics.Effects
{
    internal sealed class EffectParameter : DisposableBase
    {
        private readonly ResourceLayoutElementDescription _layoutDescription;
        private readonly uint _slot;

        private ResourceSet _data;

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
            _data = resourceSet;
        }

        public void ApplyChanges(CommandList commandEncoder)
        {
            commandEncoder.SetGraphicsResourceSet(_slot, _data);
        }
    }
}

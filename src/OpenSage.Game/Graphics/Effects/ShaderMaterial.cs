namespace OpenSage.Graphics.Effects
{
    public sealed class ShaderMaterial : MeshMaterial
    {
        protected override uint SlotSampler { get; }
        protected override uint SlotSkinningBuffer { get; }
        protected override uint SlotMeshConstants { get; }

        public ShaderMaterial(
            Effect effect,
            uint slotSampler,
            uint slotSkinningBuffer,
            uint slotMeshConstants) 
            : base(effect)
        {
            SlotSampler = slotSampler;
            SlotSkinningBuffer = slotSkinningBuffer;
            SlotMeshConstants = slotMeshConstants;
        }
    }
}

using LLGfx.Util;
using D3D12 = SharpDX.Direct3D12;

namespace LLGfx
{
    partial class VertexDescriptor
    {
        private VertexLayoutDescription[] _layoutDescriptions;

        internal D3D12.InputLayoutDescription DeviceInputLayoutDescription { get; private set; }

        private void PlatformConstruct(
            VertexAttributeDescription[] attributeDescriptions,
            VertexLayoutDescription[] layoutDescriptions)
        {
            var inputElements = new D3D12.InputElement[attributeDescriptions.Length];
            for (var i = 0; i < attributeDescriptions.Length; i++)
            {
                var desc = attributeDescriptions[i];

                inputElements[i] = new D3D12.InputElement
                {
                    AlignedByteOffset = desc.Offset,
                    Classification = desc.Classification.ToInputClassification(),
                    Format = desc.Format.ToDxgiFormat(),
                    InstanceDataStepRate = (desc.Classification == InputClassification.PerInstanceData) ? 1 : 0,
                    SemanticIndex = desc.SemanticIndex,
                    SemanticName = desc.SemanticName,
                    Slot = desc.BufferIndex
                };
            }

            DeviceInputLayoutDescription = new D3D12.InputLayoutDescription(inputElements);

            _layoutDescriptions = layoutDescriptions;
        }

        internal int GetStride(int bufferIndex)
        {
            return _layoutDescriptions[bufferIndex].Stride;
        }
    }
}

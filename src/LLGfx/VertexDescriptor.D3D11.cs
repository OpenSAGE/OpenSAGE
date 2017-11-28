using LLGfx.Util;
using D3D11 = SharpDX.Direct3D11;

namespace LLGfx
{
    partial class VertexDescriptor
    {
        private VertexLayoutDescription[] _layoutDescriptions;

        internal D3D11.InputElement[] DeviceInputElements { get; private set; }

        private void PlatformConstruct(
            VertexAttributeDescription[] attributeDescriptions,
            VertexLayoutDescription[] layoutDescriptions)
        {
            DeviceInputElements = new D3D11.InputElement[attributeDescriptions.Length];
            for (var i = 0; i < attributeDescriptions.Length; i++)
            {
                DeviceInputElements[i] = attributeDescriptions[i].ToInputElement();
            }

            _layoutDescriptions = layoutDescriptions;
        }

        internal int GetStride(int bufferIndex)
        {
            return _layoutDescriptions[bufferIndex].Stride;
        }
    }
}

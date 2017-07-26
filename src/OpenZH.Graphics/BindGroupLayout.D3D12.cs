using OpenZH.Graphics.Platforms.Direct3D12;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class BindGroupLayout
    {
        internal RootParameter RootParameter { get; private set; }

        private void PlatformConstruct(BindGroupLayoutDescription description)
        {
            RootParameter = new RootParameter(
                description.Visibility.ToShaderVisibility(),
                ToDescriptorRanges(description.Bindings));
        }

        private static DescriptorRange[] ToDescriptorRanges(BindingDescription[] bindings)
        {
            var result = new DescriptorRange[bindings.Length];

            var offset = 0;
            for (var i = 0; i < bindings.Length; i++)
            {
                var binding = bindings[i];

                result[i] = new DescriptorRange(
                    binding.BindingType.ToDescriptorRangeType(),
                    binding.Count,
                    0, // We will set this later
                    offset);

                offset += binding.Count;
            }

            return result;
        }
    }
}

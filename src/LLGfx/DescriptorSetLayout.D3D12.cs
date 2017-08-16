using LLGfx.Util;
using SharpDX.Direct3D12;

namespace LLGfx
{
    partial class DescriptorSetLayout
    {
        internal RootParameter DeviceRootParameter { get; private set; }

        private void PlatformConstruct(DescriptorSetLayoutDescription description)
        {
            DeviceRootParameter = new RootParameter(
                description.Visibility.ToShaderVisibility(),
                ToDescriptorRanges(description.Bindings));
        }

        private static DescriptorRange[] ToDescriptorRanges(DescriptorSetLayoutBinding[] bindings)
        {
            var result = new DescriptorRange[bindings.Length];

            var offset = 0;
            for (var i = 0; i < bindings.Length; i++)
            {
                var binding = bindings[i];

                result[i] = new DescriptorRange(
                    binding.DescriptorType.ToDescriptorRangeType(),
                    binding.DescriptorCount,
                    binding.BaseShaderRegister,
                    offsetInDescriptorsFromTableStart: offset);

                offset += binding.DescriptorCount;
            }

            return result;
        }
    }
}

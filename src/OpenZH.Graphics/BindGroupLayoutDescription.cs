namespace OpenZH.Graphics
{
    public struct BindGroupLayoutDescription
    {
        public ShaderStageVisibility Visibility;

        public BindingDescription[] Bindings;
    }

    public struct BindingDescription
    {
        public BindingType BindingType;
        public int Count;

        public BindingDescription(BindingType bindingType, int count)
        {
            BindingType = bindingType;
            Count = count;
        }
    }
}

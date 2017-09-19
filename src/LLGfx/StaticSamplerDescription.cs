namespace LLGfx
{
    public struct StaticSamplerDescription
    {
        public ShaderStageVisibility Visibility;
        public int ShaderRegister;
        public SamplerStateDescription SamplerStateDescription;

        public StaticSamplerDescription(
            ShaderStageVisibility visibility,
            int shaderRegister,
            SamplerStateDescription description)
        {
            Visibility = visibility;
            ShaderRegister = shaderRegister;
            SamplerStateDescription = description;
        }
    }
}

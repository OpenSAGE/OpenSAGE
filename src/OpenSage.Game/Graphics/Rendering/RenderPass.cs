namespace OpenSage.Graphics.Rendering
{
    /// <summary>
    /// Specifies which render pass a given render effect pass should be applied to.
    /// Also used within render pipelines to store the current render pass.
    /// </summary>
    public enum RenderPass
    {
        /// <summary>
        /// Generate shadow map.
        /// </summary>
        ShadowMap,

        /// <summary>
        /// Forward pass.
        /// </summary>
        Forward,
    }
}

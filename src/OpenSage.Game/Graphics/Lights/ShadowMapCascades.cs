namespace OpenSage.Graphics.Lights
{
    /// <summary>
    /// Defines how many shadow cascades to render in the shadow map
    /// for a <see cref="DirectionalLightComponent"/>.
    /// </summary>
    public enum ShadowMapCascades
    {
        /// <summary>
        /// Render shadow maps using one cascade. This is the lowest quality and fastest option.
        /// </summary>
        OneCascade = 1,

        /// <summary>
        /// Render shadow maps using two cascades.
        /// </summary>
        TwoCascades = 2,

        /// <summary>
        /// Render shadow maps using four cascades. This is the best quality and slowest option.
        /// </summary>
        FourCascades = 4
    }
}

namespace OpenSage.Data.W3d
{
    public enum W3dShaderDestBlendFunc : byte
    {
        /// <summary>
        /// destination pixel doesn't affect blending (default)
        /// </summary>
        Zero = 0,

        /// <summary>
        /// destination pixel added unmodified
        /// </summary>
        One,

        /// <summary>
        /// destination pixel multiplied by fragment RGB components
        /// </summary>
        SrcColor,

        /// <summary>
        /// destination pixel multiplied by one minus (i.e. inverse) fragment RGB components
        /// </summary>
        OneMinusSrcColor,

        /// <summary>
        /// destination pixel multiplied by fragment alpha component
        /// </summary>
        SrcAlpha,

        /// <summary>
        /// destination pixel multiplied by fragment inverse alpha
        /// </summary>
        OneMinusSrcAlpha,

        /// <summary>
        /// destination pixel multiplied by fragment RGB components prior to fogging
        /// </summary>
        SrcColorPreFog
    }
}

namespace OpenZH.Data.W3d
{
    public enum W3dShaderSrcBlendFunc : byte
    {
        /// <summary>
        /// fragment not added to color buffer
        /// </summary>
        Zero = 0,

        /// <summary>
        /// fragment added unmodified to color buffer (default)
        /// </summary>
        One,

        /// <summary>
        /// fragment RGB components multiplied by fragment A
        /// </summary>
        SrcAlpha,

        /// <summary>
        /// fragment RGB components multiplied by fragment inverse (one minus) A
        /// </summary>
        OneMinusSrcAlpha
    }
}

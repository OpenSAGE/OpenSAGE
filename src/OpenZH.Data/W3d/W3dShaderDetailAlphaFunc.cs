namespace OpenZH.Data.W3d
{
    public enum W3dShaderDetailAlphaFunc : byte
    {
        /// <summary>
        /// local (default)
        /// </summary>
        Disable = 0,

        /// <summary>
        /// other
        /// </summary>
        Detail,

        /// <summary>
        /// local * other
        /// </summary>
        Scale,

        /// <summary>
        /// ~(~local * ~other) = local + (1-local)*other
        /// </summary>
        InvScale
    }
}

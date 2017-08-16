namespace OpenSage.Data.W3d
{
    public enum W3dShaderDetailColorFunc : byte
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
        InvScale,

        /// <summary>
        /// local + other
        /// </summary>
        Add,

        /// <summary>
        /// local - other
        /// </summary>
        Sub,

        /// <summary>
        /// other - local
        /// </summary>
        SubR,

        /// <summary>
        /// (localAlpha)*local + (~localAlpha)*other
        /// </summary>
        Blend,

        /// <summary>
        /// (otherAlpha)*local + (~otherAlpha)*other
        /// </summary>
        DetailBlend
    }
}

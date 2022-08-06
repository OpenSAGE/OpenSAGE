namespace OpenSage.FileFormats.W3d
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
        DetailBlend,

        // Used in "bogeril_dor01.w3d" in EnB, no idea What the Enum should be though.
        Alt,

        // Only used by skybox01.w3d in BFME II, no idea why.
        DetailAlt = 10,

        // Used only by armyantsglow.w3d in BFME, no idea why.
        ScaleAlt = 11,
        InvScaleAlt = 12
    }
}

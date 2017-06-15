namespace OpenZH.Data.W3d
{
    public enum W3dShaderPrimaryGradient : byte
    {
        /// <summary>
        /// disable primary gradient (same as OpenGL 'decal' texture blend)
        /// </summary>
        Disable = 0,

        /// <summary>
        /// modulate fragment ARGB by gradient ARGB (default)
        /// </summary>
        Modulate,

        /// <summary>
        /// add gradient RGB to fragment RGB, copy gradient A to fragment A
        /// </summary>
        Add,

        /// <summary>
        /// environment-mapped bump mapping
        /// </summary>
        BumpEnvMap
    }
}

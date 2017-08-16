namespace OpenSage.Data.W3d
{
    public enum W3dShaderSecondaryGradient : byte
    {
        /// <summary>
        /// don't draw secondary gradient (default)
        /// </summary>
        Disable = 0,

        /// <summary>
        /// add secondary gradient RGB to fragment RGB 
        /// </summary>
        Enable
    }
}

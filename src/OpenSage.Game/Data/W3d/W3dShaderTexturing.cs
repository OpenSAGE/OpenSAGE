namespace OpenSage.Data.W3d
{
    public enum W3dShaderTexturing : byte
    {
        /// <summary>
        /// no texturing (treat fragment initial color as 1,1,1,1) (default)
        /// </summary>
        Disable = 0,

        /// <summary>
        /// enable texturing
        /// </summary>
        Enable
    }
}

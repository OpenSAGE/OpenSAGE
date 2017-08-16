namespace OpenSage.Data.W3d
{
    public enum W3dShaderDepthCompare : byte
    {
        /// <summary>
        /// pass never (i.e. always fail depth comparison test)
        /// </summary>
        PassNever = 0,

        /// <summary>
        /// pass if incoming less than stored
        /// </summary>
        PassLess,

        /// <summary>
        /// pass if incoming equal to stored
        /// </summary>
        PassEqual,

        /// <summary>
        /// pass if incoming less than or equal to stored (default)
        /// </summary>
        PassLEqual,

        /// <summary>
        /// pass if incoming greater than stored	
        /// </summary>
        PassGreater,

        /// <summary>
        /// pass if incoming not equal to stored
        /// </summary>
        PassNotEqual,

        /// <summary>
        /// pass if incoming greater than or equal to stored
        /// </summary>
        PassGEqual,

        /// <summary>
        /// pass always
        /// </summary>
        PassAlways
    }
}

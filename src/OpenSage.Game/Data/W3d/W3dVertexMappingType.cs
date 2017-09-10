namespace OpenSage.Data.W3d
{
    public enum W3dVertexMappingType : uint
    {
        Uv = 0,

        /// <summary>
        /// Uses the Reflection direction to look up the environment map.
        /// </summary>
        Environment,

        CheapEnvironment,
        Screen,

        /// <summary>
        /// Makes the texture scroll at the speed specified.
        /// </summary>
        LinearOffset,

        Silhouette,
        Scale,

        /// <summary>
        /// Given a texture that is divided up in to a grid, it animates the texture by looking
        /// up the texture from the topleft to the bottom right, going left to right and then
        /// top to bottom (the same way you would read English text). The texture map must be divided
        /// up evenly.
        /// </summary>
        Grid,

        Rotate,
        SineLinearOffset,
        StepLinearOffset,
        ZigZagLinearOffset,
        WsClassicEnv,
        WsEnvironment,
        GridClassicEnv,
        GridEnvironment,
        Random,
        Edge,
        BumpEnv
    }
}

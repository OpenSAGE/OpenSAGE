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

        /// <summary>
        /// Projects the screen coordinate as the UV coordinate.
        /// </summary>
        Screen,

        /// <summary>
        /// Makes the texture scroll at the speed specified.
        /// </summary>
        LinearOffset,

        Silhouette,

        /// <summary>
        /// Scales the UV coordinates. Useful for detail mapping.
        /// </summary>
        Scale,

        /// <summary>
        /// Given a texture that is divided up in to a grid, it animates the texture by looking
        /// up the texture from the topleft to the bottom right, going left to right and then
        /// top to bottom (the same way you would read English text). The texture map must be divided
        /// up evenly.
        /// </summary>
        Grid,

        /// <summary>
        /// Rotates a texture map counterclockwise about a specified center then scales the texture.
        /// </summary>
        Rotate,

        /// <summary>
        /// Moves the texture map in the shape of a Lissajous figure.
        /// </summary>
        SineLinearOffset,

        StepLinearOffset,
        ZigZagLinearOffset,
        WsClassicEnv,
        WsEnvironment,
        GridClassicEnv,
        GridEnvironment,
        Random,
        Edge,

        /// <summary>
        /// Sets up and possibly animates the bump matrix, also has the LinearOffset features
        /// NOTE: even if you don't want to animate the bump matrix, you should use this mapper
        /// so that the matrix gets set up with the identity settings.
        /// </summary>
        BumpEnv,

        Mask,
        Stage1_Uv,
    }
}

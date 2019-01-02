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

    /*
      /////////////////////////////////////////////////////////////////////////////////////////////
      //	 MATERIALS
      //
      //	 
      //	 The VertexMaterial defines parameters which control the calculation of the primary
      //	 and secondary gradients. The shader defines how those gradients are combined with
      //	 the texel and the frame buffer contents.
      //
      /////////////////////////////////////////////////////////////////////////////////////////////

      protected static final int W3DVERTMAT_USE_DEPTH_CUE	=							0x00000001;
      protected static final int W3DVERTMAT_ARGB_EMISSIVE_ONLY =						0x00000002;
      protected static final int W3DVERTMAT_COPY_SPECULAR_TO_DIFFUSE =				0x00000004;
      protected static final int W3DVERTMAT_DEPTH_CUE_TO_ALPHA =						0x00000008;

      protected static final int W3DVERTMAT_STAGE0_MAPPING_MASK =						0x00FF0000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_UV	=						0x00000000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_ENVIRONMENT =				0x00010000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_CHEAP_ENVIRONMENT =		0x00020000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_SCREEN =					0x00030000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_LINEAR_OFFSET =			0x00040000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_SILHOUETTE	=				0x00050000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_SCALE =					0x00060000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_GRID =						0x00070000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_ROTATE	=					0x00080000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_SINE_LINEAR_OFFSET	=		0x00090000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_STEP_LINEAR_OFFSET	=		0x000A0000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_ZIGZAG_LINEAR_OFFSET =		0x000B0000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_WS_CLASSIC_ENV	=			0x000C0000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_WS_ENVIRONMENT	=			0x000D0000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_GRID_CLASSIC_ENV =			0x000E0000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_GRID_ENVIRONMENT =			0x000F0000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_RANDOM	=					0x00100000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_EDGE =						0x00110000;
      protected static final int W3DVERTMAT_STAGE0_MAPPING_BUMPENV =					0x00120000;

      protected static final int W3DVERTMAT_STAGE1_MAPPING_MASK =						0x0000FF00;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_UV	=						0x00000000;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_ENVIRONMENT =				0x00000100;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_CHEAP_ENVIRONMENT =		0x00000200;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_SCREEN	=					0x00000300;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_LINEAR_OFFSET =			0x00000400;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_SILHOUETTE	=				0x00000500;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_SCALE =					0x00000600;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_GRID =						0x00000700;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_ROTATE	=					0x00000800;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_SINE_LINEAR_OFFSET	=		0x00000900;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_STEP_LINEAR_OFFSET	=		0x00000A00;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_ZIGZAG_LINEAR_OFFSET =		0x00000B00;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_WS_CLASSIC_ENV	=			0x00000C00;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_WS_ENVIRONMENT	=			0x00000D00;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_GRID_CLASSIC_ENV =			0x00000E00;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_GRID_ENVIRONMENT =			0x00000F00;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_RANDOM	=					0x00001000;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_EDGE =						0x00001100;
      protected static final int W3DVERTMAT_STAGE1_MAPPING_BUMPENV =					0x00001200;

      protected static final int W3DVERTMAT_PSX_MASK =								0xFF000000;
      protected static final int W3DVERTMAT_PSX_TRANS_MASK = 							0x07000000;
      protected static final int W3DVERTMAT_PSX_TRANS_NONE = 							0x00000000;
      protected static final int W3DVERTMAT_PSX_TRANS_100	= 							0x01000000;
      protected static final int W3DVERTMAT_PSX_TRANS_50 = 							0x02000000;
      protected static final int W3DVERTMAT_PSX_TRANS_25 = 							0x03000000;
      protected static final int W3DVERTMAT_PSX_TRANS_MINUS_100 = 					0x04000000;
      protected static final int W3DVERTMAT_PSX_NO_RT_LIGHTING = 						0x08000000;
      */
}

namespace OpenSage.Data.W3d
{
    // From https://raw.githubusercontent.com/mikolalysenko/w3d2ply/master/w3d_file.h
    public enum W3dChunkType
    {
        W3D_CHUNK_MESH = 0x00000000,    // Mesh definition 
            W3D_CHUNK_VERTICES = 0x00000002,    // array of vertices (array of W3dVectorStruct's)
            W3D_CHUNK_VERTEX_NORMALS = 0x00000003,  // array of normals (array of W3dVectorStruct's)
            W3D_CHUNK_MESH_USER_TEXT = 0x0000000C,  // Text from the MAX comment field (Null terminated string)
            W3D_CHUNK_VERTEX_INFLUENCES = 0x0000000E,   // Mesh Deformation vertex connections (array of W3dVertInfStruct's)
            W3D_CHUNK_MESH_HEADER3 = 0x0000001F,    //	mesh header contains general info about the mesh. (W3dMeshHeader3Struct)
            W3D_CHUNK_TRIANGLES = 0x00000020,   // New improved triangles chunk (array of W3dTriangleStruct's)
            W3D_CHUNK_VERTEX_SHADE_INDICES = 0x00000022,    // shade indexes for each vertex (array of uint32's)

            W3D_CHUNK_PRELIT_UNLIT = 0x00000023,    // optional unlit material chunk wrapper
            W3D_CHUNK_PRELIT_VERTEX = 0x00000024,   // optional vertex-lit material chunk wrapper
            W3D_CHUNK_PRELIT_LIGHTMAP_MULTI_PASS = 0x00000025,  // optional lightmapped multi-pass material chunk wrapper
            W3D_CHUNK_PRELIT_LIGHTMAP_MULTI_TEXTURE = 0x00000026,   // optional lightmapped multi-texture material chunk wrapper

                W3D_CHUNK_MATERIAL_INFO = 0x00000028,   // materials information, pass count, etc (contains W3dMaterialInfoStruct)

                W3D_CHUNK_SHADERS = 0x00000029, // shaders (array of W3dShaderStruct's)

                W3D_CHUNK_VERTEX_MATERIALS = 0x0000002A,    // wraps the vertex materials
                    W3D_CHUNK_VERTEX_MATERIAL = 0x0000002B,
                        W3D_CHUNK_VERTEX_MATERIAL_NAME = 0x0000002C,    // vertex material name (NULL-terminated string)
                        W3D_CHUNK_VERTEX_MATERIAL_INFO = 0x0000002D,    // W3dVertexMaterialStruct
                        W3D_CHUNK_VERTEX_MAPPER_ARGS0 = 0x0000002E, // Null-terminated string
                        W3D_CHUNK_VERTEX_MAPPER_ARGS1 = 0x0000002F, // Null-terminated string

                W3D_CHUNK_TEXTURES = 0x00000030,    // wraps all of the texture info
                    W3D_CHUNK_TEXTURE = 0x00000031, // wraps a texture definition
                        W3D_CHUNK_TEXTURE_NAME = 0x00000032,    // texture filename (NULL-terminated string)
                        W3D_CHUNK_TEXTURE_INFO = 0x00000033,    // optional W3dTextureInfoStruct

                W3D_CHUNK_MATERIAL_PASS = 0x00000038,   // wraps the information for a single material pass
                    W3D_CHUNK_VERTEX_MATERIAL_IDS = 0x00000039, // single or per-vertex array of uint32 vertex material indices (check chunk size)
                    W3D_CHUNK_SHADER_IDS = 0x0000003A,  // single or per-tri array of uint32 shader indices (check chunk size)
                    W3D_CHUNK_DCG = 0x0000003B, // per-vertex diffuse color values (array of W3dRGBAStruct's)
                    W3D_CHUNK_DIG = 0x0000003C, // per-vertex diffuse illumination values (array of W3dRGBStruct's)
                    W3D_CHUNK_SCG = 0x0000003E, // per-vertex specular color values (array of W3dRGBStruct's)

                    W3D_CHUNK_TEXTURE_STAGE = 0x00000048,   // wrapper around a texture stage.
                        W3D_CHUNK_TEXTURE_IDS = 0x00000049, // single or per-tri array of uint32 texture indices (check chunk size)
                        W3D_CHUNK_STAGE_TEXCOORDS = 0x0000004A, // per-vertex texture coordinates (array of W3dTexCoordStruct's)
                        W3D_CHUNK_PER_FACE_TEXCOORD_IDS = 0x0000004B,   // indices to W3D_CHUNK_STAGE_TEXCOORDS, (array of Vector3i)


            W3D_CHUNK_DEFORM = 0x00000058,  // mesh deform or 'damage' information.
                W3D_CHUNK_DEFORM_SET = 0x00000059,  // set of deform information
                    W3D_CHUNK_DEFORM_KEYFRAME = 0x0000005A, // a keyframe of deform information in the set
                        W3D_CHUNK_DEFORM_DATA = 0x0000005B, // deform information about a single vertex

            W3D_CHUNK_PS2_SHADERS = 0x00000080, // Shader info specific to the Playstation 2.

            W3D_CHUNK_AABTREE = 0x00000090, // Axis-Aligned Box Tree for hierarchical polygon culling
                W3D_CHUNK_AABTREE_HEADER,                                       // catalog of the contents of the AABTree
                W3D_CHUNK_AABTREE_POLYINDICES,                              // array of uint32 polygon indices with count=mesh.PolyCount
                W3D_CHUNK_AABTREE_NODES,                                        // array of W3dMeshAABTreeNode's with count=aabheader.NodeCount

        W3D_CHUNK_HIERARCHY = 0x00000100,   // hierarchy tree definition
            W3D_CHUNK_HIERARCHY_HEADER,
            W3D_CHUNK_PIVOTS,
            W3D_CHUNK_PIVOT_FIXUPS,                                             // only needed by the exporter...

        W3D_CHUNK_ANIMATION = 0x00000200,   // hierarchy animation data
            W3D_CHUNK_ANIMATION_HEADER,
            W3D_CHUNK_ANIMATION_CHANNEL,                                        // channel of vectors
            W3D_CHUNK_BIT_CHANNEL,                                              // channel of boolean values (e.g. visibility)

        W3D_CHUNK_COMPRESSED_ANIMATION = 0x00000280,    // compressed hierarchy animation data
            W3D_CHUNK_COMPRESSED_ANIMATION_HEADER,                          // describes playback rate, number of frames, and type of compression
            W3D_CHUNK_COMPRESSED_ANIMATION_CHANNEL,                     // compressed channel, format dependent on type of compression
            W3D_CHUNK_COMPRESSED_BIT_CHANNEL,                               // compressed bit stream channel, format dependent on type of compression

        W3D_CHUNK_MORPH_ANIMATION = 0x000002C0, // hierarchy morphing animation data (morphs between poses, for facial animation)
            W3D_CHUNK_MORPHANIM_HEADER,                                     // W3dMorphAnimHeaderStruct describes playback rate, number of frames, and type of compression
            W3D_CHUNK_MORPHANIM_CHANNEL,                                        // wrapper for a channel
                W3D_CHUNK_MORPHANIM_POSENAME,                                   // name of the other anim which contains the poses for this morph channel
                W3D_CHUNK_MORPHANIM_KEYDATA,                                    // morph key data for this channel
            W3D_CHUNK_MORPHANIM_PIVOTCHANNELDATA,                           // uin32 per pivot in the htree, indicating which channel controls the pivot

        W3D_CHUNK_HMODEL = 0x00000300,  // blueprint for a hierarchy model
            W3D_CHUNK_HMODEL_HEADER,                                            // Header for the hierarchy model
            W3D_CHUNK_NODE,                                                     // render objects connected to the hierarchy
            W3D_CHUNK_COLLISION_NODE,                                           // collision meshes connected to the hierarchy
            W3D_CHUNK_SKIN_NODE,                                                    // skins connected to the hierarchy
            OBSOLETE_W3D_CHUNK_HMODEL_AUX_DATA,                             // extension of the hierarchy model header
            OBSOLETE_W3D_CHUNK_SHADOW_NODE,                                 // shadow object connected to the hierarchy

        W3D_CHUNK_LODMODEL = 0x00000400,        // blueprint for an LOD model. This is simply a
            W3D_CHUNK_LODMODEL_HEADER,                                          // collection of 'n' render objects, ordered in terms
            W3D_CHUNK_LOD,                                                          // of their expected rendering costs. (highest is first)

        W3D_CHUNK_COLLECTION = 0x00000420,      // collection of render object names
            W3D_CHUNK_COLLECTION_HEADER,                                        // general info regarding the collection
            W3D_CHUNK_COLLECTION_OBJ_NAME,                                  // contains a string which is the name of a render object
            W3D_CHUNK_PLACEHOLDER,                                              // contains information about a 'dummy' object that will be instanced later
            W3D_CHUNK_TRANSFORM_NODE,                                           // contains the filename of another w3d file that should be transformed by this node

        W3D_CHUNK_POINTS = 0x00000440,      // array of W3dVectorStruct's. May appear in meshes, hmodels, lodmodels, or collections.

        W3D_CHUNK_LIGHT = 0x00000460,       // description of a light
            W3D_CHUNK_LIGHT_INFO,                                               // generic light parameters
            W3D_CHUNK_SPOT_LIGHT_INFO,                                          // extra spot light parameters
            W3D_CHUNK_NEAR_ATTENUATION,                                     // optional near attenuation parameters
            W3D_CHUNK_FAR_ATTENUATION,                                          // optional far attenuation parameters

        W3D_CHUNK_EMITTER = 0x00000500,     // description of a particle emitter
            W3D_CHUNK_EMITTER_HEADER,                                           // general information such as name and version
            W3D_CHUNK_EMITTER_USER_DATA,                                        // user-defined data that specific loaders can switch on
            W3D_CHUNK_EMITTER_INFO,                                             // generic particle emitter definition
            W3D_CHUNK_EMITTER_INFOV2,                                           // generic particle emitter definition (version 2.0)
            W3D_CHUNK_EMITTER_PROPS,                                            // Key-frameable properties
            OBSOLETE_W3D_CHUNK_EMITTER_COLOR_KEYFRAME,                  // structure defining a single color keyframe
            OBSOLETE_W3D_CHUNK_EMITTER_OPACITY_KEYFRAME,                    // structure defining a single opacity keyframe
            OBSOLETE_W3D_CHUNK_EMITTER_SIZE_KEYFRAME,                       // structure defining a single size keyframe
            W3D_CHUNK_EMITTER_LINE_PROPERTIES,                              // line properties, used by line rendering mode
            W3D_CHUNK_EMITTER_ROTATION_KEYFRAMES,                           // rotation keys for the particles
            W3D_CHUNK_EMITTER_FRAME_KEYFRAMES,                              // frame keys (u-v based frame animation)
            W3D_CHUNK_EMITTER_BLUR_TIME_KEYFRAMES,                      // length of tail for line groups

        W3D_CHUNK_AGGREGATE = 0x00000600,       // description of an aggregate object
            W3D_CHUNK_AGGREGATE_HEADER,                                     // general information such as name and version
            W3D_CHUNK_AGGREGATE_INFO,                                       // references to 'contained' models
            W3D_CHUNK_TEXTURE_REPLACER_INFO,                                    // information about which meshes need textures replaced
            W3D_CHUNK_AGGREGATE_CLASS_INFO,                                 // information about the original class that created this aggregate

        W3D_CHUNK_HLOD = 0x00000700,        // description of an HLod object (see HLodClass)
            W3D_CHUNK_HLOD_HEADER,                                              // general information such as name and version
            W3D_CHUNK_HLOD_LOD_ARRAY,                                           // wrapper around the array of objects for each level of detail
                W3D_CHUNK_HLOD_SUB_OBJECT_ARRAY_HEADER,                 // info on the objects in this level of detail array
                W3D_CHUNK_HLOD_SUB_OBJECT,                                      // an object in this level of detail array
            W3D_CHUNK_HLOD_AGGREGATE_ARRAY,                                 // array of aggregates, contains W3D_CHUNK_SUB_OBJECT_ARRAY_HEADER and W3D_CHUNK_SUB_OBJECT_ARRAY
            W3D_CHUNK_HLOD_PROXY_ARRAY,                                     // array of proxies, used for application-defined purposes, provides a name and a bone.

        W3D_CHUNK_BOX = 0x00000740,     // defines an collision box render object (W3dBoxStruct)
        W3D_CHUNK_SPHERE,
        W3D_CHUNK_RING,

        W3D_CHUNK_NULL_OBJECT = 0x00000750,     // defines a NULL object (W3dNullObjectStruct)

        W3D_CHUNK_LIGHTSCAPE = 0x00000800,      // wrapper for lights created with Lightscape.	
            W3D_CHUNK_LIGHTSCAPE_LIGHT,                                     // definition of a light created with Lightscape.
                W3D_CHUNK_LIGHT_TRANSFORM,                                      // position and orientation (defined as right-handed 4x3 matrix transform W3dLightTransformStruct).

        W3D_CHUNK_DAZZLE = 0x00000900,      // wrapper for a glare object. Creates halos and flare lines seen around a bright light source
            W3D_CHUNK_DAZZLE_NAME,                                              // null-terminated string, name of the dazzle (typical w3d object naming: "container.object")
            W3D_CHUNK_DAZZLE_TYPENAME,                                          // null-terminated string, type of dazzle (from dazzle.ini)

        W3D_CHUNK_SOUNDROBJ = 0x00000A00,       // description of a sound render object
            W3D_CHUNK_SOUNDROBJ_HEADER,                                     // general information such as name and version
            W3D_CHUNK_SOUNDROBJ_DEFINITION,                                 // chunk containing the definition of the sound that is to play	

        W3D_CHUNK_VERTICES_2 = 0xC00, // Unknown - added in BFME
        W3D_CHUNK_NORMALS_2 = 0xC01, // Unknown - added in BFME
    }
}

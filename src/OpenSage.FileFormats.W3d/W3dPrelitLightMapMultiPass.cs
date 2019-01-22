using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dPrelitLightMapMultiPass : W3dPrelitBase<W3dPrelitLightMapMultiPass>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_PRELIT_LIGHTMAP_MULTI_PASS;
    }
}

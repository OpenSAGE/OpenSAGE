namespace OpenSage.Graphics
{
    // One ModelMeshPart for each unique shader in a W3D_CHUNK_MATERIAL_PASS.
    public sealed class ModelMeshPart
    {
        //public W3dShader Shader { get; }

        public uint StartIndex { get; }
        public uint IndexCount { get; }

        internal ModelMeshPart(uint startIndex, uint indexCount)
        {
            StartIndex = startIndex;
            IndexCount = indexCount;
        }
    }
}

using OpenZH.Data.W3d;
using OpenZH.Graphics;

namespace OpenZH.Game.Graphics
{
    public sealed class MeshMaterial
    {
        public MeshMaterial()
        {

        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            //var materialConstantBuffer = StaticBuffer.Create(
            //    graphicsDevice,
            //    uploadBatch,
            //    new MaterialConstants
            //    {
            //        MaterialAmbient = vertexMaterial.Ambient.ToVector3(),
            //        MaterialDiffuse = vertexMaterial.Diffuse.ToVector3(),
            //        MaterialSpecular = vertexMaterial.Specular.ToVector3(),
            //        MaterialEmissive = vertexMaterial.Emissive.ToVector3(),
            //        MaterialShininess = vertexMaterial.Shininess,
            //        MaterialOpacity = vertexMaterial.Opacity
            //    });
        }
    }

    public sealed class MeshMaterialPass
    {
        public MeshMaterialPass(W3dMaterialPass w3dMaterialPass)
        {
            //w3dMaterialPass.
        }
    }
}

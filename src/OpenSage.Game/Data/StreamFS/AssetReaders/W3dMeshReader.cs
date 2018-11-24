using System;
using System.IO;
using System.Linq;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Data.W3d;
using OpenSage.Data.W3x;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class W3dMeshReader : AssetReader
    {
        public override AssetType AssetType => AssetType.W3dMesh;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            return W3xMesh.Parse(reader, imports, asset.Header);

            //var effectName = shaderName.Replace(".fx", string.Empty);

            //var effect = context.EffectLibrary.GetEffect(
            //    shaderName,
            //    vertexDescriptors);

            //// TODO: Extract state properties from shader material.
            //var rasterizerState = RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise;
            //var depthState = DepthStencilStateDescription.DepthOnlyLessEqual;
            //var blendState = BlendStateDescription.SingleDisabled;

            //var material = new ShaderMaterial(context.EffectLibrary, effect)
            //{
            //    PipelineState = new EffectPipelineState(
            //        rasterizerState,
            //        depthState,
            //        blendState,
            //        RenderPipeline.GameOutputDescription)
            //};
            

            //var materialPasses = new ModelMeshMaterialPass[1];
            //materialPasses[0] = new ModelMeshMaterialPass(
            //    context.GraphicsDevice,
            //    null,
            //    new ModelMeshPart[]
            //    {
            //        new ModelMeshPart(0, (uint) indices.Length, material)
            //    });

            //return new ModelMesh(
            //    context.GraphicsDevice,
            //    asset.Name,
            //    vertexData,
            //    indices,
            //    effect,
            //    materialPasses,
            //    false, // isSkinned,
            //    null, // parentBone,
            //    0, // numBones,
            //    boundingBox,
            //    hidden,
            //    geometryType == MeshGeometryType.CameraOriented);
        }
    }
}

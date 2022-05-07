﻿using System.Numerics;
using OpenSage.Content.Loaders;
using OpenSage.FileFormats.W3d;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class ModelBox : ModelRenderObject
    {
        private readonly AxisAlignedBoundingBox _boundingBox;

        public override ref readonly AxisAlignedBoundingBox BoundingBox => ref _boundingBox;

        public override bool Hidden { get; } = false;

        internal ModelBox(W3dBox w3dBox, AssetLoadContext context)
        {
            _boundingBox = new AxisAlignedBoundingBox(
                w3dBox.Center - w3dBox.Extent,
                w3dBox.Center + w3dBox.Extent);

            // TODO
        }

        internal override void BuildRenderList(
            RenderList renderList,
            Camera camera,
            ModelInstance modelInstance,
            ModelMeshInstance modelMeshInstance,
            ModelBone parentBone,
            in Matrix4x4 modelTransform,
            bool castsShadow,
            MeshShaderResources.RenderItemConstantsPS? renderItemConstantsPS)
        {
            // TODO
        }

        internal override void BuildRenderListWithWorldMatrix(
            RenderList renderList,
            Camera camera,
            ModelInstance modelInstance,
            ModelMeshInstance modelMeshInstance,
            ModelBone parentBone,
            in Matrix4x4 meshWorldMatrix,
            bool castsShadow,
            MeshShaderResources.RenderItemConstantsPS? renderItemConstantsPS)
        {
            // TODO
        }
    }
}

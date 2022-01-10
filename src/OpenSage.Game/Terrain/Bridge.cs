using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Terrain
{
    public sealed class Bridge : DisposableBase
    {
        private readonly GameObject _bridgeObject;
        private readonly Model _model;
        private readonly ModelInstance _modelInstance;
        private readonly List<Tuple<ModelSubObject, Matrix4x4>> _meshes;

        internal Bridge(
            GameContext gameContext,
            MapObject mapObject,
            in Vector3 startPosition,
            in Vector3 endPosition)
        {
            var template = gameContext.AssetLoadContext.AssetStore.BridgeTemplates.GetByName(mapObject.TypeName);
            if (template == null)
            {
                return;
            }

            var model = template.BridgeModelName.Value;
            if (model == null)
            {
                return;
            }

            // TODO: Cache this.
            var genericBridgeDefinition = gameContext.AssetLoadContext.AssetStore.ObjectDefinitions.GetByName("GenericBridge");

            _bridgeObject = gameContext.GameObjects.Add(genericBridgeDefinition, null);
            _bridgeObject.SetMapObjectProperties(mapObject);

            _model = model;

            _modelInstance = AddDisposable(_model.CreateInstance(gameContext.AssetLoadContext));

            _modelInstance.Update(TimeInterval.Zero);

            _meshes = CreateMeshes(
                gameContext,
                template,
                startPosition,
                endPosition,
                _model,
                _modelInstance);
        }

        private List<Tuple<ModelSubObject, Matrix4x4>> CreateMeshes(
            GameContext gameContext,
            BridgeTemplate template,
            in Vector3 startPosition,
            in Vector3 endPosition,
            Model model,
            ModelInstance modelInstance)
        {
            const float heightBias = 1f;

            var bridgeLeft = model.SubObjects.First(x => x.Name.Contains("BRIDGE_LEFT"));
            var bridgeSpan = model.SubObjects.First(x => x.Name.Contains("BRIDGE_SPAN"));
            var bridgeRight = model.SubObjects.First(x => x.Name.Contains("BRIDGE_RIGHT"));

            float GetLength(ModelSubObject subObject)
            {
                var transformedBounds = AxisAlignedBoundingBox.Transform(subObject.RenderObject.BoundingBox, modelInstance.RelativeBoneTransforms[subObject.Bone.Index]);
                return transformedBounds.Max.X - transformedBounds.Min.X;
            }

            // See how many spans we can fit in.
            var lengthLeft = GetLength(bridgeLeft) * template.BridgeScale;
            var lengthSpan = GetLength(bridgeSpan) * template.BridgeScale;
            var lengthRight = GetLength(bridgeRight) * template.BridgeScale;

            var startPositionWithHeight = startPosition;
            startPositionWithHeight.Z = gameContext.Scene3D.Terrain.HeightMap.GetHeight(startPosition.X, startPosition.Y) + heightBias;

            var endPositionWithHeight = endPosition;
            endPositionWithHeight.Z = gameContext.Scene3D.Terrain.HeightMap.GetHeight(endPosition.X, endPosition.Y) + heightBias;

            var horizontalDistance = Vector3.Distance(startPosition, endPosition);

            var distance = Vector3.Distance(startPositionWithHeight, endPositionWithHeight);

            var spanLength = distance - lengthLeft - lengthRight;

            // There are zero or more spans in the bridge, as space permits.
            var numSpans = (int) (spanLength / lengthSpan);

            var totalLength = lengthLeft + (lengthSpan * numSpans) + lengthRight;
            var scaleX = distance / totalLength;

            var rotationAroundZ = QuaternionUtility.CreateRotation(Vector3.UnitX, Vector3.Normalize(endPosition - startPosition));

            var rotationAroundY = Matrix4x4.CreateRotationY(-MathF.Atan((endPositionWithHeight.Z - startPositionWithHeight.Z) / horizontalDistance));

            var worldMatrix =
                Matrix4x4.CreateScale(scaleX, 1, 1) *
                Matrix4x4.CreateScale(template.BridgeScale) *
                rotationAroundY *
                Matrix4x4.CreateFromQuaternion(rotationAroundZ) *
                Matrix4x4.CreateTranslation(startPositionWithHeight);

            var meshes = new List<Tuple<ModelSubObject, Matrix4x4>>();

            var currentOffset = 0f;
            Matrix4x4 GetLocalTranslation(ModelSubObject subObject)
            {
                var transformedBounds = AxisAlignedBoundingBox.Transform(subObject.RenderObject.BoundingBox, modelInstance.RelativeBoneTransforms[subObject.Bone.Index]);
                var w3dOffset = transformedBounds.Min.X;

                var result =
                    modelInstance.RelativeBoneTransforms[subObject.Bone.Index] *
                    Matrix4x4.CreateTranslation(new Vector3(currentOffset, 0, 0)) *
                    Matrix4x4.CreateTranslation(new Vector3(-w3dOffset, 0, 0)) *
                    worldMatrix;

                currentOffset += (transformedBounds.Max.X - transformedBounds.Min.X);

                return result;
            }

            meshes.Add(Tuple.Create(bridgeLeft, GetLocalTranslation(bridgeLeft)));

            for (var i = 0; i < numSpans; i++)
            {
                meshes.Add(Tuple.Create(bridgeSpan, GetLocalTranslation(bridgeSpan)));
            }

            meshes.Add(Tuple.Create(bridgeRight, GetLocalTranslation(bridgeRight)));

            var transformedLeftBounds = AxisAlignedBoundingBox.Transform(bridgeLeft.RenderObject.BoundingBox, modelInstance.RelativeBoneTransforms[bridgeLeft.Bone.Index]);

            new BridgeTowers(
                template,
                _bridgeObject.Owner,
                gameContext,
                worldMatrix,
                0,
                transformedLeftBounds.Min.Y,
                totalLength / template.BridgeScale,
                transformedLeftBounds.Max.Y,
                rotationAroundZ);

            return meshes;
        }

        internal void BuildRenderList(RenderList renderList, Camera camera)
        {
            if (_model == null)
            {
                return;
            }

            foreach (var meshMatrix in _meshes)
            {
                // TODO: Don't do this.
                var index = Array.IndexOf(_model.SubObjects, meshMatrix.Item1);

                meshMatrix.Item1.RenderObject.BuildRenderListWithWorldMatrix(
                    renderList,
                    camera,
                    _modelInstance,
                    _modelInstance.BeforeRenderDelegates[index],
                    _modelInstance.BeforeRenderDelegatesDepth[index],
                    meshMatrix.Item1.Bone,
                    meshMatrix.Item2,
                    true);
            }
        }
    }
}

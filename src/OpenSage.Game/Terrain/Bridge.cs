using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Terrain
{
    public sealed class Bridge : DisposableBase
    {
        private readonly Model _model;
        private readonly ModelInstance _modelInstance;
        private readonly List<Tuple<ModelSubObject, Matrix4x4>> _meshes;
        private readonly List<GameObject> _towers;

        private Bridge(
            ContentManager contentManager,
            HeightMap heightMap,
            BridgeTemplate template,
            in Vector3 startPosition,
            in Vector3 endPosition,
            Model model,
            GameObjectCollection parent)
        {
            _model = model;

            _modelInstance = AddDisposable(_model.CreateInstance(contentManager));

            _modelInstance.Update(TimeInterval.Zero);

            _towers = new List<GameObject>();

            GameObject CreateTower(string objectName)
            {
                var tower = AddDisposable(contentManager.InstantiateObject(objectName, parent));
                _towers.Add(tower);
                return tower;
            }

            var towerFromLeft = CreateTower(template.TowerObjectNameFromLeft);
            var towerFromRight = CreateTower(template.TowerObjectNameFromRight);
            var towerToLeft = CreateTower(template.TowerObjectNameToLeft);
            var towerToRight = CreateTower(template.TowerObjectNameToRight);

            _meshes = CreateMeshes(
                heightMap,
                template,
                startPosition,
                endPosition,
                _model,
                _modelInstance,
                towerFromLeft,
                towerFromRight,
                towerToLeft,
                towerToRight);
        }

        internal static bool TryCreateBridge(
            ContentManager contentManager,
            HeightMap heightMap,
            BridgeTemplate template,
            in Vector3 startPosition,
            in Vector3 endPosition,
            out Bridge bridge,
            GameObjectCollection parent)
        {
            var modelPath = Path.Combine("Art", "W3D", template.BridgeModelName + ".W3D");
            var model = contentManager.Load<Model>(modelPath);

            if (model == null)
            {
                bridge = null;
                return false;
            }

            bridge = new Bridge(
                contentManager,
                heightMap,
                template,
                startPosition,
                endPosition,
                model,
                parent);
            return true;
        }

        private static List<Tuple<ModelSubObject, Matrix4x4>> CreateMeshes(
            HeightMap heightMap,
            BridgeTemplate template,
            in Vector3 startPosition,
            in Vector3 endPosition,
            Model model,
            ModelInstance modelInstance,
            GameObject towerFromLeft,
            GameObject towerFromRight,
            GameObject towerToLeft,
            GameObject towerToRight)
        {
            const float heightBias = 1f;

            var bridgeLeft = model.SubObjects.First(x => x.Name.Contains("BRIDGE_LEFT"));
            var bridgeSpan = model.SubObjects.First(x => x.Name.Contains("BRIDGE_SPAN"));
            var bridgeRight = model.SubObjects.First(x => x.Name.Contains("BRIDGE_RIGHT"));

            float GetLength(ModelSubObject subObject)
            {
                var transformedBounds = BoundingBox.Transform(subObject.RenderObject.BoundingBox, modelInstance.RelativeBoneTransforms[subObject.Bone.Index]);
                return transformedBounds.Max.X - transformedBounds.Min.X;
            }

            // See how many spans we can fit in.
            var lengthLeft = GetLength(bridgeLeft) * template.BridgeScale;
            var lengthSpan = GetLength(bridgeSpan) * template.BridgeScale;
            var lengthRight = GetLength(bridgeRight) * template.BridgeScale;

            var startPositionWithHeight = startPosition;
            startPositionWithHeight.Z = heightMap.GetHeight(startPosition.X, startPosition.Y) + heightBias;

            var endPositionWithHeight = endPosition;
            endPositionWithHeight.Z = heightMap.GetHeight(endPosition.X, endPosition.Y) + heightBias;

            var horizontalDistance = Vector3.Distance(startPosition, endPosition);

            var distance = Vector3.Distance(startPositionWithHeight, endPositionWithHeight);

            var spanLength = distance - lengthLeft - lengthRight;

            // There are zero or more spans in the bridge, as space permits.
            var numSpans = (int) (spanLength / lengthSpan);

            var totalLength = lengthLeft + (lengthSpan * numSpans) + lengthRight;
            var scaleX = distance / totalLength;

            var rotationAroundZ = QuaternionUtility.CreateRotation(Vector3.UnitX, Vector3.Normalize(endPosition - startPosition));

            var rotationAroundY = Matrix4x4.CreateRotationY(-MathUtility.Atan((endPositionWithHeight.Z - startPositionWithHeight.Z) / horizontalDistance));

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
                var transformedBounds = BoundingBox.Transform(subObject.RenderObject.BoundingBox, modelInstance.RelativeBoneTransforms[subObject.Bone.Index]);
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

            var transformedLeftBounds = BoundingBox.Transform(bridgeLeft.RenderObject.BoundingBox, modelInstance.RelativeBoneTransforms[bridgeLeft.Bone.Index]);

            var width = GetWidth(transformedLeftBounds) * template.BridgeScale;

            void SetTowerTransform(GameObject tower, float x, float y)
            {
                tower.Transform.Translation = Vector3.Transform(
                    new Vector3(x, y, 0),
                    worldMatrix);
                tower.Transform.Rotation = rotationAroundZ;

                tower.LocalLogicTick(TimeInterval.Zero, 1.0f, heightMap);
            }

            SetTowerTransform(
                towerFromLeft,
                0,
                transformedLeftBounds.Min.Y);

            SetTowerTransform(
                towerFromRight,
                0,
                transformedLeftBounds.Max.Y);

            SetTowerTransform(
                towerToLeft,
                totalLength / template.BridgeScale,
                transformedLeftBounds.Min.Y);

            SetTowerTransform(
                towerToRight,
                totalLength / template.BridgeScale,
                transformedLeftBounds.Max.Y);

            return meshes;
        }

        private static float GetWidth(in BoundingBox box)
        {
            return box.Max.Y - box.Min.Y;
        }

        internal void BuildRenderList(RenderList renderList, Camera camera)
        {
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

            foreach (var tower in _towers)
            {
                tower.BuildRenderList(
                    renderList,
                    camera);
            }
        }
    }
}

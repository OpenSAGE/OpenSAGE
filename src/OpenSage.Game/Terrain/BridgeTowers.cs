using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Logic.Object;

namespace OpenSage.Terrain
{
    public sealed class BridgeTowers
    {
        internal static BridgeTowers CreateForLandmarkBridge(
            AssetStore assetStore,
            GameObjectCollection gameObjects,
            GameObject gameObject,
            MapObject mapObject)
        {
            var worldMatrix =
                Matrix4x4.CreateFromQuaternion(gameObject.Transform.Rotation)
                * Matrix4x4.CreateTranslation(gameObject.Transform.Translation);

            var landmarkBridgeTemplate = assetStore.BridgeTemplates.GetByKey(mapObject.TypeName);

            var halfLength = gameObject.Definition.Geometry.MinorRadius;
            var halfWidth = gameObject.Definition.Geometry.MajorRadius;

            return new BridgeTowers(
                landmarkBridgeTemplate,
                gameObjects,
                worldMatrix,
                -halfWidth,
                -halfLength,
                halfWidth,
                halfLength,
                gameObject.Transform.Rotation);
        }

        public BridgeTowers(
            BridgeTemplate template,
            GameObjectCollection gameObjects,
            Matrix4x4 worldMatrix,
            float startX,
            float startY,
            float endX,
            float endY,
            Quaternion rotation)
        {
            void CreateTower(ObjectDefinition objectDefinition, float x, float y)
            {
                var tower = gameObjects.Add(objectDefinition);

                tower.Transform.Translation = Vector3.Transform(
                    new Vector3(x, y, 0),
                    worldMatrix);

                tower.Transform.Rotation = rotation;
            }

            CreateTower(template.TowerObjectNameFromLeft.Value, startX, startY);
            CreateTower(template.TowerObjectNameFromRight.Value, endX, startY);
            CreateTower(template.TowerObjectNameToLeft.Value, startX, endY);
            CreateTower(template.TowerObjectNameToRight.Value, endX, endY);
        }
    }
}

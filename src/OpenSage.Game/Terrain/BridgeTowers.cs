using System.Numerics;
using OpenSage.Logic;
using OpenSage.Logic.Object;

namespace OpenSage.Terrain
{
    public sealed class BridgeTowers
    {
        internal static BridgeTowers CreateForLandmarkBridge(
            GameContext gameContext,
            GameObject gameObject)
        {
            var worldMatrix =
                Matrix4x4.CreateFromQuaternion(gameObject.Rotation)
                * Matrix4x4.CreateTranslation(gameObject.Translation);

            var landmarkBridgeTemplate = gameContext.AssetLoadContext.AssetStore.BridgeTemplates.GetByName(gameObject.Definition.Name);

            var halfLength = gameObject.Definition.Geometry.MinorRadius;
            var halfWidth = gameObject.Definition.Geometry.MajorRadius;

            return new BridgeTowers(
                landmarkBridgeTemplate,
                gameObject.Owner,
                gameContext,
                worldMatrix,
                -halfWidth,
                -halfLength,
                halfWidth,
                halfLength,
                gameObject.Rotation);
        }

        internal BridgeTowers(
            BridgeTemplate template,
            Player owner,
            GameContext gameContext,
            Matrix4x4 worldMatrix,
            float startX,
            float startY,
            float endX,
            float endY,
            Quaternion rotation)
        {
            void CreateTower(ObjectDefinition objectDefinition, float x, float y)
            {
                var tower = gameContext.GameObjects.Add(objectDefinition, owner);

                var translation = Vector3.Transform(
                    new Vector3(x, y, 0),
                    worldMatrix);

                tower.UpdateTransform(translation, rotation);
            }

            CreateTower(template.TowerObjectNameFromLeft.Value, startX, startY);
            CreateTower(template.TowerObjectNameFromRight.Value, endX, startY);
            CreateTower(template.TowerObjectNameToLeft.Value, startX, endY);
            CreateTower(template.TowerObjectNameToRight.Value, endX, endY);
        }
    }
}

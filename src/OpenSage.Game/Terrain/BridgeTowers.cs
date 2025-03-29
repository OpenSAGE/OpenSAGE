using System.Numerics;
using OpenSage.Logic;
using OpenSage.Logic.Object;

namespace OpenSage.Terrain;

public sealed class BridgeTowers
{
    internal static BridgeTowers CreateForLandmarkBridge(
        IGameEngine gameEngine,
        GameObject gameObject)
    {
        var worldMatrix =
            Matrix4x4.CreateFromQuaternion(gameObject.Rotation)
            * Matrix4x4.CreateTranslation(gameObject.Translation);

        var landmarkBridgeTemplate = gameEngine.AssetLoadContext.AssetStore.BridgeTemplates.GetByName(gameObject.Definition.Name);

        var geometryShape = gameObject.Definition.Geometry.Shapes[0];

        var halfLength = geometryShape.MinorRadius;
        var halfWidth = geometryShape.MajorRadius;

        return new BridgeTowers(
            landmarkBridgeTemplate,
            gameObject.Owner,
            gameEngine,
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
        IGameEngine gameEngine,
        Matrix4x4 worldMatrix,
        float startX,
        float startY,
        float endX,
        float endY,
        Quaternion rotation)
    {
        void CreateTower(ObjectDefinition objectDefinition, float x, float y)
        {
            var tower = gameEngine.GameLogic.CreateObject(objectDefinition, owner);

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

/// <summary>
/// Bridges have 4 towers around it that the player can attack or use to repair
/// the bridge.
/// </summary>
public enum BridgeTowerType
{
    TowerFromLeft,
    TowerFromRight,
    TowerToLeft,
    TowerToRight,
}

using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Logic.Object;

namespace OpenSage.Terrain
{
    public sealed class BridgeTowers : DisposableBase
    {
        public static BridgeTowers CreateForLandmarkBridge(
            ContentManager contentManager,
            GameObjectCollection gameObjects,
            GameObject gameObject,
            MapObject mapObject)
        {
            var worldMatrix =
                Matrix4x4.CreateFromQuaternion(gameObject.Transform.Rotation)
                * Matrix4x4.CreateTranslation(gameObject.Transform.Translation);

            var landmarkBridgeTemplate = contentManager.IniDataContext.FindBridgeTemplate(mapObject.TypeName);

            var halfLength = gameObject.Definition.Geometry.MinorRadius;
            var halfWidth = gameObject.Definition.Geometry.MajorRadius;

            return new BridgeTowers(
                landmarkBridgeTemplate,
                contentManager,
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
            ContentManager contentManager,
            GameObjectCollection gameObjects,
            Matrix4x4 worldMatrix,
            float startX,
            float startY,
            float endX,
            float endY,
            Quaternion rotation)
        {
            void CreateTower(string objectName, float x, float y)
            {
                var tower = AddDisposable(contentManager.InstantiateObject(objectName, gameObjects));
                gameObjects.Add(tower);

                tower.Transform.Translation = Vector3.Transform(
                    new Vector3(x, y, 0),
                    worldMatrix);

                tower.Transform.Rotation = rotation;
            }

            CreateTower(template.TowerObjectNameFromLeft, startX, startY);
            CreateTower(template.TowerObjectNameFromRight, endX, startY);
            CreateTower(template.TowerObjectNameToLeft, startX, endY);
            CreateTower(template.TowerObjectNameToRight, endX, endY);
        }

        // TODO: Remove game objects when this is disposed.
    }
}

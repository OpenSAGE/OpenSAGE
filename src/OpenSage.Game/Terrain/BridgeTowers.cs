using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;

namespace OpenSage.Terrain
{
    public sealed class BridgeTowers : DisposableBase
    {
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

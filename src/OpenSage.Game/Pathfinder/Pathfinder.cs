using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Logic.Object;

namespace OpenSage.Pathfinder
{
    public class Pathfinder : DisposableBase
    {
        public List<GridPoint> PassabilPoints { get; }
        private readonly MapFile _mapFile;

        public Pathfinder(MapFile mapFile)
        {
            PassabilPoints = new List<GridPoint>();
            _mapFile = mapFile;
            GenerateGridFromMapFile();
        }

        private void GenerateGridFromMapFile()
        {
            for (var x = 0; x < _mapFile.HeightMapData.Width && x < _mapFile.BlendTileData.Impassability.GetLength(0); x++)
            {
                for (var y = 0; y < _mapFile.HeightMapData.Height && y < _mapFile.BlendTileData.Impassability.GetLength(1); y++)
                {
                    if (!_mapFile.BlendTileData.Impassability[x, y])
                    {
                        var position = new Vector3(x * 10 + (_mapFile.HeightMapData.BorderWidth * -10), y * 10 + (_mapFile.HeightMapData.BorderWidth * -10), _mapFile.HeightMapData.Elevations[x, y] * _mapFile.HeightMapData.VerticalScale);
                        PassabilPoints.Add(new GridPoint(position));
                    }
                }
            }
        }

        public void RemoveGridPointsForGameObjects(GameObjectCollection gameObjects)
        {
            foreach (var gameObject in gameObjects.Items)
            {
                if (gameObject.Collider != null)
                {
                    List<GridPoint> points = gameObject.Collider.GetGridPoints();
                    
                    foreach (var gridPoint in points)
                    {
                        if (PassabilPoints.Exists(i =>
                            (int) i.Position.X == ((int) gridPoint.Position.X / 10) * 10 &&
                            (int) i.Position.Y == ((int) gridPoint.Position.Y / 10) * 10))
                        {
                            PassabilPoints.RemoveAll(i =>
                                    (int) i.Position.X == ((int) gridPoint.Position.X / 10) * 10 &&
                                    (int) i.Position.Y == ((int) gridPoint.Position.Y / 10) * 10);
                        }
                    }
                }
            }
        }
    }
}

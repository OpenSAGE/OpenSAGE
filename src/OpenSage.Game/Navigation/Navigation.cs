using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Terrain;

namespace OpenSage.Navigation
{
    public class Navigation
    {
        private readonly Graph _graph;
        private readonly HeightMap _heightMap;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Navigation(BlendTileData tileData, HeightMap heightMap)
        {
            var width = tileData.Impassability.GetLength(0);
            var height = tileData.Impassability.GetLength(1);
            _graph = new Graph(width, height);

            for (var x = 0; x < _graph.Width; x++)
            {
                for (var y = 0; y < _graph.Height; y++)
                {
                    var impassable = tileData.Impassability[x, y];
                    _graph.GetNode(x, y).Passability = impassable ? Passability.Impassable : Passability.Passable;
                }
            }

            _heightMap = heightMap;
        }

        private Vector2 GetNodePosition(Node node)
        {
            var xyz =_heightMap.GetPosition(node.X, node.Y);
            return new Vector2(xyz.X, xyz.Y);
        }

        private Node GetClosestNode(Vector3 pos)
        {
            var coords = _heightMap.GetTilePosition(pos);

            if (coords == null)
            {
                return null;
            }

            var (x, y) = coords.Value;
            return _graph.GetNode(x, y);
        }

        public bool IsPassable(Vector3 position) => GetClosestNode(position).IsPassable;

        private Node GetNextPassablePosition(Vector3 start, Vector3 end)
        {
            var offset = end - start;
            var direction = Vector3.Normalize(offset);
            var currentNode = GetClosestNode(start);

            var maxSteps = (int) offset.Vector2XY().Length() + 1;

            while(!currentNode.IsPassable && maxSteps > 0)
            {
                start += direction;
                currentNode = GetClosestNode(start);
                maxSteps--;
            }
            return maxSteps > 0 ? currentNode : null;
        }

        public IList<Vector3> CalculatePath(Vector3 start, Vector3 end, out bool endIsPassable)
        {
            var result = new List<Vector3>();
            endIsPassable = true;
            var startNode = GetClosestNode(start);
            var endNode = GetClosestNode(end);

            if (!startNode.IsPassable)
            {
                startNode = GetNextPassablePosition(start, end);
            }

            if (!endNode.IsPassable)
            {
                endNode = GetNextPassablePosition(end, start);
                endIsPassable = false;
            }

            if (startNode == null || endNode == null)
            {
                Logger.Info("Aborting pathfinding because start and/or end are null");
                return result;
            }

            var route = _graph.Search(startNode, endNode);

            if (route == null)
            {
                Logger.Warn($"Graph search failed to find a path between {start} and {end}.");
                return result;
            }

            PathOptimizer.RemoveRedundantNodes(route);
            PathOptimizer.SmoothPath(route, _graph);

            foreach (var node in route)
            {
                var pos = GetNodePosition(node);
                result.Add(new Vector3(pos.X, pos.Y, _heightMap.GetHeight(pos.X, pos.Y)));
            }
            return result;
        }

        public void UpdateAreaPassability(GameObject gameObject, bool passable)
        {
            if (gameObject.RoughCollider == null)
            {
                return;
            }

            foreach (var collider in gameObject.Colliders)
            {
                var axisAlignedBoundingArea = collider.AxisAlignedBoundingArea;

                var bottomLeft = new Vector3(axisAlignedBoundingArea.X, axisAlignedBoundingArea.Y, 0);
                var bottomLeftNode = GetClosestNode(bottomLeft);
                var topRight = new Vector3(axisAlignedBoundingArea.X + axisAlignedBoundingArea.Width, axisAlignedBoundingArea.Y + axisAlignedBoundingArea.Height, 0);
                var topRightNode = GetClosestNode(topRight);

                //sometimes map objects are places outside the actual map....
                if (bottomLeftNode == null || topRightNode == null)
                {
                    return;
                }

                var area = gameObject.RoughCollider.BoundingArea;

                for (var x = 0; x < topRightNode.X - bottomLeftNode.X; x++)
                {
                    for (var y = 0; y < topRightNode.Y - bottomLeftNode.Y; y++)
                    {
                        var node = _graph.GetNode(bottomLeftNode.X + x, bottomLeftNode.Y + y);
                        var position = GetNodePosition(node);
                        if (area.Contains(position))
                        {
                            node.Passability = passable ? Passability.Passable : Passability.Impassable;
                        }
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Terrain;

namespace OpenSage.Navigation
{
    public class Navigation
    {
        Graph _graph;
        HeightMap _heightMap;

        public Navigation(BlendTileData tileData, HeightMap heightMap)
        {
            var width = tileData.Impassability.GetLength(0);
            var height = tileData.Impassability.GetLength(1);
            _graph = new Graph(width, height);

            for (int x = 0; x < _graph.Width; x++)
            {
                for (int y = 0; y < _graph.Height; y++)
                {
                    bool passable = tileData.Impassability[x, y];
                    _graph.GetNode(x, y).Passability = passable ? Passability.Passable : Passability.Impassable;
                }
            }

            _heightMap = heightMap;
        }

        private Vector2 GetNodePosition(Node node)
        {
            return new Vector2(node.X * HeightMap.HorizontalScale,
                               node.Y * HeightMap.HorizontalScale);
        }

        private Node GetClosestNode(Vector3 pos)
        {
            int x = (int) MathF.Round(pos.X / HeightMap.HorizontalScale);
            int y = (int) MathF.Round(pos.Y / HeightMap.HorizontalScale);
            return _graph.GetNode(x, y);
        }

        private void RemoveRedundantNodes(List<Node> nodes)
        {
            int prevDirX = 0;
            int prevDirY = 0;

            for (int i = 1; i < nodes.Count; i++)
            {
                int dirX = nodes[i].X - nodes[i-1].X;
                int dirY = nodes[i].Y - nodes[i-1].Y;

                if (dirX == prevDirX && dirY == prevDirY)
                {
                    nodes.RemoveAt(i);
                    i--;
                }
                prevDirX = dirX;
                prevDirY = dirY;
            }
        }

        public List<Vector3> CalculatePath(Vector3 start, Vector3 end)
        {
            var startNode = GetClosestNode(start);
            var endNode = GetClosestNode(end);

            var route = _graph.Search(startNode, endNode);

            var result = new List<Vector3>();

            if (route != null)
            {
                RemoveRedundantNodes(route);
                foreach (var node in route)
                {
                    var pos = GetNodePosition(node);
                    result.Add(new Vector3(pos, _heightMap.GetHeight(pos.X, pos.Y)));
                }
            }

            return result;
        }
    }
}

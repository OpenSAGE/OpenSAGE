using System.Collections.Generic;

namespace OpenSage.Navigation
{
    public static class PathOptimizer
    {
        public static void RemoveRedundantNodes(List<Node> nodes)
        {
            int prevDirX = 0;
            int prevDirY = 0;

            for (int i = nodes.Count - 1; i > 0; i--)
            {
                int dirX = nodes[i].X - nodes[i - 1].X;
                int dirY = nodes[i].Y - nodes[i - 1].Y;

                if (dirX == prevDirX && dirY == prevDirY)
                {
                    nodes.RemoveAt(i);
                }
                prevDirX = dirX;
                prevDirY = dirY;
            }
        }
    }
}

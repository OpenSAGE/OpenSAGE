using System.Collections.Generic;
using System.Numerics;

namespace OpenSage.Utilities
{
    /// <summary>
    /// Tessellates a polygon into a set of non-overlapping triangles.
    /// From http://www.flipcode.com/archives/Efficient_Polygon_Triangulation.shtml
    /// </summary>
    public static class Triangulator
    {
        private const float Epsilon = 0.0000000001f;

        /// <summary>
        /// Computes area of a contour/polygon.
        /// </summary>
        private static float Area(IReadOnlyList<Vector2> contour)
        {
            var n = contour.Count;
            var A = 0.0f;

            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                A += contour[p].X * contour[q].Y - contour[q].X * contour[p].Y;
            }

            return A * 0.5f;
        }

        /// <summary>
        /// Decides if a point P is Inside of the triangle defined by A, B, C.
        /// </summary>
        private static bool InsideTriangle(
            float Ax, float Ay,
            float Bx, float By,
            float Cx, float Cy,
            float Px, float Py)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = Cx - Bx; ay = Cy - By;
            bx = Ax - Cx; by = Ay - Cy;
            cx = Bx - Ax; cy = By - Ay;
            apx = Px - Ax; apy = Py - Ay;
            bpx = Px - Bx; bpy = Py - By;
            cpx = Px - Cx; cpy = Py - Cy;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return (aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f);
        }

        private static bool Snip(IReadOnlyList<Vector2> contour, int u, int v, int w, int n, int[] V)
        {
            var Ax = contour[V[u]].X;
            var Ay = contour[V[u]].Y;

            var Bx = contour[V[v]].X;
            var By = contour[V[v]].Y;

            var Cx = contour[V[w]].X;
            var Cy = contour[V[w]].Y;

            if (Epsilon > (((Bx - Ax) * (Cy - Ay)) - ((By - Ay) * (Cx - Ax))))
            {
                return false;
            }

            for (var p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                {
                    continue;
                }

                var Px = contour[V[p]].X;
                var Py = contour[V[p]].Y;

                if (InsideTriangle(Ax, Ay, Bx, By, Cx, Cy, Px, Py))
                {
                    return false;
                }
            }

            return true;
        }

        public static unsafe bool Process(IReadOnlyList<Vector2> contour, out IReadOnlyList<Vector2> result)
        {
            // allocate and initialize list of Vertices in polygon

            result = null;

            var n = contour.Count;
            if (n < 3)
            {
                return false;
            }

            var V = new int[n];

            // we want a counter-clockwise polygon in V

            if (0.0f < Area(contour))
            {
                for (int v = 0; v < n; v++)
                {
                    V[v] = v;
                }
            }
            else
            {
                for (int v = 0; v < n; v++)
                {
                    V[v] = (n - 1) - v;
                }
            }

            var nv = n;

            //  remove nv-2 Vertices, creating 1 triangle every time
            var count = 2 * nv;   /* error detection */

            var tempResult = new List<Vector2>();

            for (int m = 0, v = nv - 1; nv > 2;)
            {
                // if we loop, it is probably a non-simple polygon
                if (0 >= count--)
                {
                    // Probably bad polygon!
                    return false;
                }

                // three consecutive vertices in current polygon, <u,v,w>
                var u = v;
                if (nv <= u)
                {
                    u = 0; // previous
                }

                v = u + 1;
                if (nv <= v)
                {
                    v = 0; // new v
                }

                var w = v + 1;
                if (nv <= w)
                {
                    w = 0; // next
                }

                if (Snip(contour, u, v, w, nv, V))
                {
                    int a, b, c, s, t;

                    // true names of the vertices
                    a = V[u]; b = V[v]; c = V[w];

                    // output Triangle
                    tempResult.Add(contour[a]);
                    tempResult.Add(contour[b]);
                    tempResult.Add(contour[c]);

                    m++;

                    // remove v from remaining polygon
                    for (s = v, t = v + 1; t < nv; s++, t++)
                    {
                        V[s] = V[t];
                    }

                    nv--;

                    // reset error detection counter
                    count = 2 * nv;
                }
            }

            result = tempResult;
            return true;
        }
    }
}

using System.Numerics;
using OpenSage.Data.Map;

namespace OpenSage.Terrain
{
    public sealed class HeightMap
    {
        private const int HorizontalScale = 10;

        // Gathered from heights in World Builder's status bar compared to heightmap data.
        private const float VerticalScale = 0.625f;

        private readonly HeightMapData _heightMapData;

        public int Width { get; }
        public int Height { get; }

        public float GetHeight(int x, int y) => _heightMapData.Elevations[x, y] * VerticalScale;

        public Vector3 GetPosition(int x, int y) => new Vector3(
            (x - _heightMapData.BorderWidth) * HorizontalScale,
            (y - _heightMapData.BorderWidth) * HorizontalScale,
            GetHeight(x, y));

        public (int X, int Y)? GetTilePosition(Vector3 worldPosition)
        {
            var tilePosition = (worldPosition / HorizontalScale) 
                + new Vector3(_heightMapData.BorderWidth, _heightMapData.BorderWidth, 0);

            var result = (X: (int) tilePosition.X, Y: (int) tilePosition.Y);

            if (result.X < 0 || result.X >= _heightMapData.Width
                || result.Y < 0 || result.Y >= _heightMapData.Height)
            {
                return null;
            }

            return result;
        }

        public Vector3[,] Normals { get; }

        public HeightMap(HeightMapData heightMapData)
        {
            _heightMapData = heightMapData;

            Width = (int) heightMapData.Width;
            Height = (int) heightMapData.Height;

            Normals = new Vector3[Width, Height];
            for (int x = 0; x < Width; ++x)
                for (int y = 0; y < Height; ++y)
                    Normals[x, y] = CalculateNormal(x, y);
        }

        /// <summary>
		/// Function computes the normal for the xy'th quad.
		/// We take the quad normal as the average of the two
		/// triangles that make up the quad.
		/// 
		///       u
		/// h0*-------*h1
		///   |      /|
		///  v|     / |t
		///   |    /  |
		///   |   /   |
		/// h2*-------*h3
		///       s
		/// </summary>
		private Vector3 CalculateQuadNormal(int x, int y)
        {
            float h0 = GetHeight(x, y);
            float h1 = GetHeight(x + 1, y);
            float h2 = GetHeight(x, y + 1);
            float h3 = GetHeight(x + 1, y + 1);

            Vector3 u = new Vector3(HorizontalScale, 0, h1 - h0);
            Vector3 v = new Vector3(0, HorizontalScale, h2 - h0);

            Vector3 s = new Vector3(-HorizontalScale, 0, h2 - h3);
            Vector3 t = new Vector3(0, -HorizontalScale, h1 - h3);

            Vector3 n1 = Vector3.Normalize(Vector3.Cross(u, v));
            Vector3 n2 = Vector3.Normalize(Vector3.Cross(s, t));

            return (n1 + n2) * 0.5f;
        }

        /// <summary>
        /// The vertex normal is found by averaging the normals of the four quads that surround the vertex
        /// </summary>
        private Vector3 CalculateNormal(int x, int y)
        {
            Vector3 avg = Vector3.Zero;
            float num = 0;

            for (int m = x - 1; m <= x; ++m)
            {
                for (int n = y - 1; n <= y; ++n)
                {
                    // vertices on heightmap boundaries do not have
                    // surrounding quads in some directions, so we just
                    // average in a normal vector that is axis aligned
                    // with the y-axis.
                    if (m < 0 || n < 0 || m == Width - 1 || n == Height - 1)
                    {
                        avg += Vector3.UnitZ;
                        num += 1.0f;
                    }
                    else
                    {
                        avg += CalculateQuadNormal(m, n);
                        num += 1.0f;
                    }
                }
            }
            avg /= num;

            return Vector3.Normalize(avg);
        }
    }
}

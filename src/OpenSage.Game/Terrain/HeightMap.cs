using System;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Mathematics;

namespace OpenSage.Terrain
{
    public sealed class HeightMap
    {
        internal const int HorizontalScale = 10;

        private readonly float _verticalScale;

        private readonly HeightMapData _heightMapData;

        public int Width { get; }
        public int Height { get; }

        public float GetHeight(int x, int y) => _heightMapData.Elevations[x, y] * _verticalScale;

        /// <summary>
        /// Gets height at the given world space coordinate.
        /// Returns 0 if the coordinate is out of bounds.
        /// </summary>
        public float GetHeight(float x, float y)
        {
            // convert coordinates to heightmap scale
            x = x / HorizontalScale + _heightMapData.BorderWidth;
            y = y / HorizontalScale + _heightMapData.BorderWidth;

            x = Math.Clamp(x, 0, Width - 1);
            y = Math.Clamp(y, 0, Height - 1);

            // get integer and fractional parts of coordinates
            var nIntX0 = MathUtility.FloorToInt(x);
            var nIntY0 = MathUtility.FloorToInt(y);
            var fFractionalX = x - nIntX0;
            var fFractionalY = y - nIntY0;

            // get coordinates for "other" side of quad
            var nIntX1 = MathUtility.Clamp(nIntX0 + 1, 0, Width - 1);
            var nIntY1 = MathUtility.Clamp(nIntY0 + 1, 0, Height - 1);

            // read 4 map values
            var f0 = GetHeight(nIntX0, nIntY0);
            var f1 = GetHeight(nIntX1, nIntY0);
            var f2 = GetHeight(nIntX0, nIntY1);
            var f3 = GetHeight(nIntX1, nIntY1);

            // calculate averages
            var fAverageLo = (f1 * fFractionalX) + (f0 * (1.0f - fFractionalX));
            var fAverageHi = (f3 * fFractionalX) + (f2 * (1.0f - fFractionalX));

            return (fAverageHi * fFractionalY) + (fAverageLo * (1.0f - fFractionalY));
        }

        public Vector3 GetNormal(float x, float y)
        {
            // convert coordinates to heightmap scale
            x = x / HorizontalScale + _heightMapData.BorderWidth;
            y = y / HorizontalScale + _heightMapData.BorderWidth;

            if (x >= Width || y >= Height || x < 0 || y < 0)
            {
                return new Vector3(0, 0, 1.0f);
            }

            return Normals[(int)x, (int)y];
        }

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

            _verticalScale = heightMapData.VerticalScale;

            Width = (int) heightMapData.Width - 1; //last colum is not rendered (in worldbuilder)
            Height = (int) heightMapData.Height - 1;//last row is not rendered (in worldbuilder)

            Normals = new Vector3[Width, Height];
            for (var x = 0; x < Width; ++x)
            {
                for (var y = 0; y < Height; ++y)
                {
                    Normals[x, y] = CalculateNormal(x, y);
                }
            }
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
            var h0 = GetHeight(x, y);
            var h1 = GetHeight(x + 1, y);
            var h2 = GetHeight(x, y + 1);
            var h3 = GetHeight(x + 1, y + 1);

            var u = new Vector3(HorizontalScale, 0, h1 - h0);
            var v = new Vector3(0, HorizontalScale, h2 - h0);

            var s = new Vector3(-HorizontalScale, 0, h2 - h3);
            var t = new Vector3(0, -HorizontalScale, h1 - h3);

            var n1 = Vector3.Normalize(Vector3.Cross(u, v));
            var n2 = Vector3.Normalize(Vector3.Cross(s, t));

            return (n1 + n2) * 0.5f;
        }

        /// <summary>
        /// The vertex normal is found by averaging the normals of the four quads that surround the vertex
        /// </summary>
        private Vector3 CalculateNormal(int x, int y)
        {
            var avg = Vector3.Zero;
            float num = 0;

            for (var m = x - 1; m <= x; ++m)
            {
                for (var n = y - 1; n <= y; ++n)
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

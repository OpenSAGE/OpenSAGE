using System;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Mathematics;

namespace OpenSage.Terrain
{
    public sealed class HeightMap
    {
        public const int HorizontalScale = 10;

        private readonly float _verticalScale;

        private readonly HeightMapData _heightMapData;

        public int Width { get; }
        public int Height { get; }
        public int MaxXCoordinate => (Width - 2 * (int) _heightMapData.BorderWidth) * HorizontalScale;
        public int MaxYCoordinate => (Height - 2 * (int) _heightMapData.BorderWidth) * HorizontalScale;


        public float GetHeight(int x, int y) => _heightMapData.Elevations[x, y] * _verticalScale;

        public float GetUpperHeight(float x, float y)
        {
            var (nIntX0, nIntX1, _) = ConvertWorldCoordinates(x, Width);
            var (nIntY0, nIntY1, _) = ConvertWorldCoordinates(y, Height);

            var heights = new float[] {
                GetHeight(nIntX0, nIntY0),
                GetHeight(nIntX1, nIntY0),
                GetHeight(nIntX0, nIntY1),
                GetHeight(nIntX1, nIntY1),
            };
            return heights.Max();
        }

        /// <summary>
        /// Gets height at the given world space coordinate.
        /// Returns 0 if the coordinate is out of bounds.
        /// </summary>
        public float GetHeight(float x, float y)
        {
            var (nIntX0, nIntX1, fFractionalX) = ConvertWorldCoordinates(x, Width);
            var (nIntY0, nIntY1, fFractionalY) = ConvertWorldCoordinates(y, Height);

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

        private (int p0, int p1, float fractional) ConvertWorldCoordinates(float p, int maxHeightmapScale)
        {
            // convert coordinates to heightmap scale
            p = p / HorizontalScale + _heightMapData.BorderWidth;
            p = Math.Clamp(p, 0, maxHeightmapScale - 1);

            // get integer and fractional parts of coordinate
            var nIntP0 = MathUtility.FloorToInt(p);
            var fFractionalP = p - nIntP0;

            // get coordinates for "other" side of quad
            var nIntP1 = Math.Clamp(nIntP0 + 1, 0, maxHeightmapScale - 1);

            return (nIntP0, nIntP1, fFractionalP);
        }

        public Vector3 GetNormal(float x, float y)
        {
            // convert coordinates to heightmap scale
            x = x / HorizontalScale + _heightMapData.BorderWidth;
            y = y / HorizontalScale + _heightMapData.BorderWidth;

            if (x >= Width || y >= Height || x < 0 || y < 0)
            {
                return Vector3.UnitZ;
            }

            return Normals[(int)x, (int)y];
        }

        public Vector3 GetPosition(int x, int y) => new Vector3(
            (x - _heightMapData.BorderWidth) * HorizontalScale,
            (y - _heightMapData.BorderWidth) * HorizontalScale,
            GetHeight(x, y));

        public (int X, int Y)? GetTilePosition(in Vector3 worldPosition)
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

        public Vector2 GetHeightMapPosition(in Vector3 worldPosition)
        {
            return
                ((worldPosition / HorizontalScale) + new Vector3(_heightMapData.BorderWidth, _heightMapData.BorderWidth, 0))
                .Vector2XY();
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
                    // with the z-axis.
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

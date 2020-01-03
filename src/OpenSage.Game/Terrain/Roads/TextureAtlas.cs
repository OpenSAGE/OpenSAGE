using System.Collections.Generic;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Terrain.Roads
{
    internal class TextureCoordinates
    {
        public TextureCoordinates(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }

        public Vector2 TopLeft { get; }
        public Vector2 TopRight { get; }
        public Vector2 BottomLeft { get; }
        public Vector2 BottomRight { get; }

        public static TextureCoordinates LeftTopWidthHeight(float left, float top, float width, float height)
        {
            return new TextureCoordinates(
                new Vector2(left, top),
                new Vector2(left + width, top),
                new Vector2(left, top + height),
                new Vector2(left + width, top + height));
        }

        public static TextureCoordinates LeftTopRightBottom(float left, float top, float right, float bottom)
        {
            return new TextureCoordinates(
                new Vector2(left, top),
                new Vector2(right, top),
                new Vector2(left, bottom),
                new Vector2(right, bottom));
        }

        public static TextureCoordinates Curve(float y, float halfRoadWidth, float angle)
        {
            var topLeft = new Vector2(0, y - halfRoadWidth);
            var bottomLeft = new Vector2(0, y + halfRoadWidth);

            var radius = 10f / 3f * halfRoadWidth;
            var center = new Vector2(0, y - radius);
            var topRight = Vector2Utility.RotateAroundPoint(center, topLeft, angle);
            var bottomRight = Vector2Utility.RotateAroundPoint(center, bottomLeft, angle);

            return new TextureCoordinates(
                topLeft,
                topRight,
                bottomLeft,
                bottomRight);
        }
    }

    internal class TextureAtlas
    {
        // The actual texture coordinates depend on the RoadTemplate's RoadWidthInTexture,
        // so we create and cache one instance per width.
        private static readonly IDictionary<float, TextureAtlas> TextureAtlasCache = new Dictionary<float, TextureAtlas>();

        // The default road width in texture space.
        private const float UnscaledRoadWidth = 0.25f;
        private const float StubLength = 0.004f;

        // There are 3x3 tiles in a road texture, so a half tile is 1/6 = 0.1666...,
        // but the calculations are based on the rounded number.
        private const float HalfTileSize = 0.166f;


        private IDictionary<RoadTextureType, TextureCoordinates> _coordinates;

        private TextureAtlas(float roadWidthInTexture)
        {
            var roadWidth = UnscaledRoadWidth * roadWidthInTexture;
            var halfRoadWidth = roadWidth / 2;

            _coordinates = new Dictionary<RoadTextureType, TextureCoordinates>();

            _coordinates.Add(
                RoadTextureType.Straight,
                TextureCoordinates.LeftTopWidthHeight(
                    0f,
                    HalfTileSize - halfRoadWidth,
                    1f,
                    roadWidth));

            _coordinates.Add(
                RoadTextureType.TCrossing,
                TextureCoordinates.LeftTopRightBottom(
                    5 * HalfTileSize - halfRoadWidth,
                    3 * HalfTileSize - UnscaledRoadWidth / 2 - StubLength,
                    5 * HalfTileSize + UnscaledRoadWidth / 2 + StubLength,
                    3 * HalfTileSize + UnscaledRoadWidth / 2 + StubLength));

            _coordinates.Add(
                RoadTextureType.XCrossing,
                TextureCoordinates.LeftTopRightBottom(
                    5 * HalfTileSize - UnscaledRoadWidth / 2 - StubLength,
                    5 * HalfTileSize - UnscaledRoadWidth / 2 - StubLength,
                    5 * HalfTileSize + UnscaledRoadWidth / 2 + StubLength,
                    5 * HalfTileSize + UnscaledRoadWidth / 2 + StubLength));

            _coordinates.Add(
                RoadTextureType.AsymmetricYCrossing,
                TextureCoordinates.LeftTopWidthHeight(
                    0.395f - halfRoadWidth,
                    0.645f,
                    1.2f * UnscaledRoadWidth + halfRoadWidth,
                    0.335f));

            _coordinates.Add(
                RoadTextureType.BroadCurve,
                TextureCoordinates.Curve(
                    HalfTileSize,
                    halfRoadWidth,
                    MathUtility.ToRadians(30)));

            // TODO: TightCurve, SymmetricY, Join
        }

        public TextureCoordinates this[RoadTextureType roadType] => _coordinates[roadType];

        public static TextureAtlas ForRoadWidth(float roadWidthInTexture)
        {
            if (!TextureAtlasCache.TryGetValue(roadWidthInTexture, out var atlas))
            {
                atlas = new TextureAtlas(roadWidthInTexture);
                TextureAtlasCache.Add(roadWidthInTexture, atlas);
            }

            return atlas;
        }
    }
}

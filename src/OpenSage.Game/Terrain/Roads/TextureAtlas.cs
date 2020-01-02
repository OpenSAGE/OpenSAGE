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

        public static TextureCoordinates Rectangle(float left, float top, float width, float height)
        {
            return new TextureCoordinates(
                new Vector2(left, top),
                new Vector2(left + width, top),
                new Vector2(left, top + height),
                new Vector2(left + width, top + height));
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

        // There are 3x3 tiles in a road texture
        private const float TileSize = 1 / 3f;

        // The width of the crossings' small road stubs
        private const float StubLength = 1 / 64f;

        private IDictionary<RoadTextureType, TextureCoordinates> _coordinates;

        private TextureAtlas(float roadWidthInTexture)
        {
            var roadWidth = 0.25f * roadWidthInTexture;
            var halfRoadWidth = roadWidth / 2;

            _coordinates = new Dictionary<RoadTextureType, TextureCoordinates>();

            _coordinates.Add(
                RoadTextureType.Straight,
                TextureCoordinates.Rectangle(
                    0f,
                    GetTileCenter(0) - halfRoadWidth,
                    1f,
                    roadWidth));

            _coordinates.Add(
                RoadTextureType.TCrossing,
                TextureCoordinates.Rectangle(
                    GetTileCenter(2) - halfRoadWidth,
                    GetTileCenter(1) - halfRoadWidth - StubLength,
                    roadWidth + StubLength,
                    roadWidth + 2 * StubLength));

            _coordinates.Add(
                RoadTextureType.XCrossing,
                TextureCoordinates.Rectangle(
                    GetTileCenter(2) - halfRoadWidth - StubLength,
                    GetTileCenter(2) - halfRoadWidth - StubLength,
                    roadWidth + 2 * StubLength,
                    roadWidth + 2 * StubLength));

            _coordinates.Add(
                RoadTextureType.AsymmetricYCrossing,
                TextureCoordinates.Rectangle(
                    0.395f - halfRoadWidth,
                    GetTileCenter(2) - 0.15f,
                    0.415f,
                    0.3f));

            _coordinates.Add(
                RoadTextureType.BroadCurve,
                TextureCoordinates.Curve(
                    GetTileCenter(1),
                    halfRoadWidth,
                    MathUtility.ToRadians(30)));

            // TODO: TightCurve, SymmetricY, Join
        }

        public TextureCoordinates this[RoadTextureType roadType] => _coordinates[roadType];

        private float GetTileCenter(int tileIndex) => (tileIndex + 0.5f) * TileSize;

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

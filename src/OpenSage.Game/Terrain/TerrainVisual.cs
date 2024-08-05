using System.Collections.Generic;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Terrain
{
    public sealed class TerrainVisual : IPersistableObject
    {
        private bool _unknownBool;
        private readonly List<Tree> _trees = new();

        public void Persist(StatePersister reader)
        {
            var version = reader.PersistVersion(3);

            reader.BeginObject("Base");
            reader.PersistVersion(1);
            reader.EndObject();

            reader.PersistBoolean(ref _unknownBool);
            if (_unknownBool)
            {
                reader.BeginObject("UnknownThing");

                reader.PersistVersion(1);

                // Matches VertexWaterXGridCellsN and VertexWaterYGridCellsN in GameData.ini
                var gridCellsX = 0;
                reader.PersistInt32(ref gridCellsX);
                var gridCellsY = 0;
                reader.PersistInt32(ref gridCellsY);

                // Don't know why, but this gives the correct length for this array.
                var dataCount = (gridCellsX + 3) * (gridCellsY + 3) * 10;
                reader.SkipUnknownBytes(dataCount);

                reader.EndObject();
            }

            var game = reader.Game;

            var area = game.Scene3D.MapFile.HeightMapData.Area;
            reader.PersistUInt32(ref area);
            if (area != game.Scene3D.MapFile.HeightMapData.Area)
            {
                throw new InvalidStateException();
            }

            var width = game.Scene3D.MapFile.HeightMapData.Width;
            var height = game.Scene3D.MapFile.HeightMapData.Height;
            var elevations = game.Scene3D.MapFile.HeightMapData.Elevations;

            reader.BeginArray("Elevations");
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var elevation = (byte)elevations[x, y];
                    reader.PersistByteValue(ref elevation);
                    elevations[x, y] = elevation;
                }
            }
            reader.EndArray();

            if (reader.Mode == StatePersistMode.Read)
            {
                // TODO: Not great to create the initial patches,
                // then recreate them here.
                game.Scene3D.Terrain.OnHeightMapChanged();
            }

            if (version >= 3)
            {
                reader.PersistVersion(1);

                reader.PersistVersion(1);

                reader.PersistListWithUInt32Count(
                    _trees,
                    static (StatePersister persister, ref Tree item) =>
                    {
                        item ??= new Tree();
                        persister.PersistObject(item);
                    },
                    "Trees");

                reader.PersistVersion(1);
            }
        }
    }

    public sealed class Tree : IPersistableObject
    {
        public string ModelName;
        public string TextureFileName;
        public Vector3 Position;
        public float Scale;
        public float UnknownFloat1;
        public float UnknownFloat2;
        public uint DrawableID;
        public float UnknownFloat3;
        public Vector3 ToppleDirection;
        public TreeToppleState ToppleState;
        public float UnknownFloat4;
        public Matrix4x3 ToppleTransform;
        public int UnknownInt;

        public void Persist(StatePersister persister)
        {
            persister.PersistAsciiString(ref ModelName);
            persister.PersistAsciiString(ref TextureFileName);
            persister.PersistVector3(ref Position);
            persister.PersistSingle(ref Scale);
            persister.PersistSingle(ref UnknownFloat1);
            persister.PersistSingle(ref UnknownFloat2);
            persister.PersistUInt32(ref DrawableID);

            persister.SkipUnknownBytes(4);

            persister.PersistSingle(ref UnknownFloat3);
            persister.PersistVector3(ref ToppleDirection);
            persister.PersistEnum(ref ToppleState);
            persister.PersistSingle(ref UnknownFloat4);

            persister.SkipUnknownBytes(4);

            persister.PersistMatrix4x3(ref ToppleTransform);

            persister.PersistInt32(ref UnknownInt);

            if ((ToppleState == TreeToppleState.NotToppled && UnknownInt != 0) ||
                (ToppleState == TreeToppleState.Toppled && UnknownInt != -1))
            {
                throw new InvalidStateException();
            }
        }
    }

    public enum TreeToppleState
    {
        NotToppled = 0,
        Toppled = 4,
    }
}

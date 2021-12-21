namespace OpenSage.Terrain
{
    public sealed class TerrainVisual
    {
        internal void Load(SaveFileReader reader, Game game)
        {
            reader.ReadVersion(2);
            reader.ReadVersion(1);

            var unknownBool1 = reader.ReadBoolean();
            if (unknownBool1)
            {
                reader.ReadVersion(1);

                // Matches VertexWaterXGridCellsN and VertexWaterYGridCellsN in GameData.ini
                var gridCellsX = reader.ReadInt32();
                var gridCellsY = reader.ReadInt32();

                // Don't know why, but this gives the correct length for this array.
                var dataCount = (gridCellsX + 3) * (gridCellsY + 3) * 10;
                reader.SkipUnknownBytes(dataCount);
            }

            var area = reader.ReadUInt32();
            if (area != game.Scene3D.MapFile.HeightMapData.Area)
            {
                throw new InvalidStateException();
            }

            var width = game.Scene3D.MapFile.HeightMapData.Width;
            var height = game.Scene3D.MapFile.HeightMapData.Height;
            var elevations = game.Scene3D.MapFile.HeightMapData.Elevations;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    elevations[x, y] = reader.ReadByte();
                }
            }

            // TODO: Not great to create the initial patches,
            // then recreate them here.
            game.Scene3D.Terrain.OnHeightMapChanged();
        }
    }
}

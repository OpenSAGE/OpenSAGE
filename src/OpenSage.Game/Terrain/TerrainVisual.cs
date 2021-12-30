namespace OpenSage.Terrain
{
    public sealed class TerrainVisual
    {
        private bool _unknownBool;

        internal void Load(StatePersister reader, Game game)
        {
            reader.PersistVersion(2);
            reader.PersistVersion(1);

            reader.PersistBoolean(ref _unknownBool);
            if (_unknownBool)
            {
                reader.PersistVersion(1);

                // Matches VertexWaterXGridCellsN and VertexWaterYGridCellsN in GameData.ini
                var gridCellsX = 0;
                reader.PersistInt32(ref gridCellsX);
                var gridCellsY = 0;
                reader.PersistInt32(ref gridCellsY);

                // Don't know why, but this gives the correct length for this array.
                var dataCount = (gridCellsX + 3) * (gridCellsY + 3) * 10;
                reader.SkipUnknownBytes(dataCount);
            }

            var area = game.Scene3D.MapFile.HeightMapData.Area;
            reader.PersistUInt32(ref area);
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
                    var elevation = (byte)elevations[x, y];
                    reader.PersistByte(ref elevation);
                    elevations[x, y] = elevation;
                }
            }

            // TODO: Not great to create the initial patches,
            // then recreate them here.
            game.Scene3D.Terrain.OnHeightMapChanged();
        }
    }
}

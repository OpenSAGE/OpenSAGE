namespace OpenSage.Terrain
{
    public sealed class TerrainVisual : IPersistableObject
    {
        private bool _unknownBool;

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.BeginObject("Base");
            reader.PersistVersion(1);
            reader.EndObject();

            reader.PersistBoolean("UnknownBool", ref _unknownBool);
            if (_unknownBool)
            {
                reader.BeginObject("UnknownThing");

                reader.PersistVersion(1);

                // Matches VertexWaterXGridCellsN and VertexWaterYGridCellsN in GameData.ini
                var gridCellsX = 0;
                reader.PersistInt32("GridCellsX", ref gridCellsX);
                var gridCellsY = 0;
                reader.PersistInt32("GridCellsY", ref gridCellsY);

                // Don't know why, but this gives the correct length for this array.
                var dataCount = (gridCellsX + 3) * (gridCellsY + 3) * 10;
                reader.SkipUnknownBytes(dataCount);

                reader.EndObject();
            }

            var game = reader.Game;

            var area = game.Scene3D.MapFile.HeightMapData.Area;
            reader.PersistUInt32("Area", ref area);
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
        }
    }
}

using System;
using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic
{
    public sealed class PartitionCellManager
    {
        private readonly Game _game;
        private readonly float _partitionCellSize;

        private int _numCellsX;
        private int _numCellsY;
        private PartitionCell[] _cells;

        internal PartitionCellManager(Game game)
        {
            _game = game;
            _partitionCellSize = game.AssetStore.GameData.Current.PartitionCellSize;
        }

        internal void OnNewGame()
        {
            var border = _game.Scene3D.MapFile.HeightMapData.Borders[0];

            var boundaryWidth = border.Corner2X - border.Corner1X;
            var boundaryHeight = border.Corner2Y - border.Corner1Y;

            _numCellsX = (int) MathF.Ceiling((boundaryWidth * 10.0f) / _partitionCellSize);
            _numCellsY = (int) MathF.Ceiling((boundaryHeight * 10.0f) / _partitionCellSize);

            var numCells = _numCellsX * _numCellsY;

            _cells = new PartitionCell[numCells];
            for (var i = 0; i < _cells.Length; i++)
            {
                _cells[i] = new PartitionCell();
            }
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            var partitionCellSize = reader.ReadSingle();
            if (partitionCellSize != _partitionCellSize)
            {
                throw new InvalidDataException();
            }

            var partitionCellCount = reader.ReadUInt32();
            if (partitionCellCount != _cells.Length)
            {
                throw new InvalidDataException();
            }

            for (var i = 0; i < partitionCellCount; i++)
            {
                _cells[i].Load(reader);
            }

#if DEBUG
            var builder = new System.Text.StringBuilder();
            for (var y = 0; y < _numCellsY; y++)
            {
                for (var x = 0; x < _numCellsX; x++)
                {
                    var cell = _cells[((_numCellsY - 1 - y) * _numCellsX) + x];
                    var value = cell.Values[2];

                    char c;
                    if (value.State < 0)
                    {
                        c = (char)((-value.State) + '0');
                    }
                    else if (value.State == 0)
                    {
                        c = '-';
                    }
                    else if (value.State == 1)
                    {
                        c = '*';
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }

                    builder.Append(c);
                }
                builder.AppendLine();
            }
            File.WriteAllText($"Partition{Path.GetFileNameWithoutExtension(((FileStream) reader.Inner.BaseStream).Name)}.txt", builder.ToString());
#endif

            var someOtherCount = reader.ReadUInt32();
            for (var i = 0; i < someOtherCount; i++)
            {
                reader.ReadVersion(1);
                var position = reader.ReadVector3();
                var visionRange = reader.ReadSingle();
                reader.ReadUInt16();
                var frameSomething = reader.ReadUInt32();
            }
        }

        // TODO: We think the algorithm is:
        // onMove(prevPosition, newPosition){
        //   foreach(visibleCellsOn(prevPosition) as cell){
        //     setTimeout(() => cell--, 5000);
        //     }
        //   
        //   foreach(visibleCellsOn(newPosition) as cell){
        //     cell++
        //   }
        // }
    }

    public sealed class PartitionCell
    {
        public readonly PartitionCellValue[] Values;

        internal PartitionCell()
        {
            Values = new PartitionCellValue[16];
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var valuesSpan = Values.AsSpan();
            reader.ReadSpan(valuesSpan);

            for (var i = 0; i < Values.Length; i++)
            {
                if (Values[i].Unknown != 0)
                {
                    throw new InvalidDataException();
                }
            }
        }
    }

    public struct PartitionCellValue
    {
        public short State;
        public short Unknown;
    }
}

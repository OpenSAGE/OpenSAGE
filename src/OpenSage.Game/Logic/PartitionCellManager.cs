using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ImGuiNET;

namespace OpenSage.Logic
{
    public sealed class PartitionCellManager
    {
        private readonly Game _game;
        private readonly float _partitionCellSize;
        private readonly List<ShroudReveal> _shroudReveals = new();

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
                throw new InvalidStateException();
            }

            var partitionCellCount = reader.ReadUInt32();
            if (partitionCellCount != _cells.Length)
            {
                throw new InvalidStateException();
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

            var shroudRevealCount = reader.ReadUInt32();
            for (var i = 0; i < shroudRevealCount; i++)
            {
                var shroudReveal = new ShroudReveal();
                shroudReveal.Load(reader);
                _shroudReveals.Add(shroudReveal);
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

        internal void DrawDiagnostic()
        {
            var p = ImGui.GetCursorScreenPos();

            var availableSize = ImGui.GetContentRegionAvail();
            var cellSize = availableSize / new Vector2(_numCellsX, _numCellsY);

            for (var y = 0; y < _numCellsY; y++)
            {
                for (var x = 0; x < _numCellsX; x++)
                {
                    var cell = _cells[((_numCellsY - 1 - y) * _numCellsX) + x];
                    var value = cell.Values[2];

                    uint cellColor;
                    if (value.State < 0)
                    {
                        //c = (char) ((-value.State) + '0');
                        cellColor = 0xFFFF00FF;
                    }
                    else if (value.State == 0)
                    {
                        cellColor = 0xFF0000FF;
                    }
                    else if (value.State == 1)
                    {
                        cellColor = 0xFF00FFFF;
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }

                    ImGui.GetWindowDrawList().AddRectFilled(
                        p + new Vector2(cellSize.X * x, cellSize.Y * y),
                        p + new Vector2(cellSize.X * x, cellSize.Y * y) + cellSize,
                        cellColor);
                }
            }
        }
    }

    public sealed class PartitionCell
    {
        public readonly PartitionCellValue[] Values;

        internal PartitionCell()
        {
            Values = new PartitionCellValue[Player.MaxPlayers];
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            for (var i = 0; i < Values.Length; i++)
            {
                ref var value = ref Values[i];

                reader.ReadInt16(ref value.State);
                reader.ReadInt16(ref value.Unknown);

                if (value.Unknown != 0)
                {
                    throw new InvalidStateException();
                }
            }
        }
    }

    public struct PartitionCellValue
    {
        public short State;
        public short Unknown;
    }

    public sealed class ShroudReveal
    {
        public Vector3 Position;
        public float VisionRange;
        public ushort Unknown;
        public uint FrameSomething;

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            reader.ReadVector3(ref Position);
            VisionRange = reader.ReadSingle();
            reader.ReadUInt16(ref Unknown);
            reader.ReadFrame(ref FrameSomething);
        }
    }
}

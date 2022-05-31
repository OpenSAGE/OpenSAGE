using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Logic
{
    public sealed class PartitionCellManager : IPersistableObject
    {
        private readonly Game _game;
        private readonly float _partitionCellSize;
        private readonly List<ShroudReveal> _shroudReveals = new();
        private readonly List<PartitionObject> _objects = new();
        private readonly HashSet<PartitionObject> _dirtyObjects = new();

        private Rectangle _terrainBoundary;
        private int _numCellsX;
        private int _numCellsY;
        private PartitionCell[] _cells;

        public float PartitionCellSize => _partitionCellSize;

        public int NumCellsX => _numCellsX;

        public int NumCellsY => _numCellsY;

        public PartitionCell this[int x, int y] => _cells[(y * _numCellsX) + x];

        internal PartitionCellManager(Game game)
        {
            _game = game;
            _partitionCellSize = game.AssetStore.GameData.Current.PartitionCellSize;
        }

        internal void OnNewGame()
        {
            var border = _game.Scene3D.MapFile.HeightMapData.Borders[0];

            _terrainBoundary = Rectangle.FromCorners(
                new Point2D((int)border.Corner1X, (int)border.Corner1Y),
                new Point2D((int)border.Corner2X, (int)border.Corner2Y));

            _numCellsX = (int) MathF.Ceiling((_terrainBoundary.Width * 10.0f) / _partitionCellSize);
            _numCellsY = (int) MathF.Ceiling((_terrainBoundary.Height * 10.0f) / _partitionCellSize);

            _cells = new PartitionCell[_numCellsX * _numCellsY];

            for (var y = 0; y < _numCellsY; y++)
            {
                for (var x = 0; x < _numCellsX; x++)
                {
                    var cellTopLeft = new Vector2(x, y);
                    var worldSpaceMin = (cellTopLeft * _partitionCellSize) + _terrainBoundary.TopLeft.ToVector2();

                    var worldSpaceBounds = new RectangleF(worldSpaceMin, _partitionCellSize, _partitionCellSize);

                    var cell = new PartitionCell(worldSpaceBounds);

                    _cells[(y * _numCellsX) + x] = cell;
                }
            }
        }

        public void OnObjectAdded(GameObject gameObject)
        {
            var partitionObject = new PartitionObject(this, gameObject);

            _objects.Add(partitionObject);
            _dirtyObjects.Add(partitionObject);

            gameObject.PartitionObject = partitionObject;
        }

        internal void RemovePartitionObject(PartitionObject partitionObject)
        {
            _objects.Remove(partitionObject);
            _dirtyObjects.Remove(partitionObject);
        }

        internal void SetDirty(PartitionObject partitionObject)
        {
            _dirtyObjects.Add(partitionObject);
        }

        public void Update()
        {
            // First update overlapping cells of dirty objects.
            foreach (var dirtyObject in _dirtyObjects)
            {
                dirtyObject.UpdateOverlappingCells();
            }

            // Then check for possible collision pairs using coarse check.
            // TODO: Don't allocate this every time.
            var possibleCollisionPairs = new List<(PartitionObject, PartitionObject)>();
            foreach (var dirtyObject in _dirtyObjects)
            {
                if (dirtyObject.GameObject.IsKindOf(ObjectKinds.Immobile))
                {
                    continue;
                }

                foreach (var cell in dirtyObject.OverlappingCells)
                {
                    foreach (var overlappingObject in cell.Objects)
                    {
                        if (overlappingObject != dirtyObject)
                        {
                            possibleCollisionPairs.Add((dirtyObject, overlappingObject));
                        }
                    }
                }
            }

            // Finally, check for actual collisions using exact checks.
            foreach (var possibleCollisionPair in possibleCollisionPairs)
            {
                if (possibleCollisionPair.Item1.CollidesWith(possibleCollisionPair.Item2))
                {
                    possibleCollisionPair.Item1.GameObject.OnCollide(possibleCollisionPair.Item2.GameObject);
                    possibleCollisionPair.Item2.GameObject.OnCollide(possibleCollisionPair.Item1.GameObject);
                }
            }

            _dirtyObjects.Clear();
        }

        public IEnumerable<PartitionCell> GetCells(Vector2 worldPosition, float worldRadius)
        {
            // Compute which cells are overlapped by this object.
            var partitionSpaceBounds = GetPartitionSpaceBounds(worldPosition, worldRadius);

            for (var y = partitionSpaceBounds.TopLeft.Y; y < partitionSpaceBounds.BottomRight.Y; y++)
            {
                if (y < 0 || y >= NumCellsY)
                {
                    continue;
                }

                for (var x = partitionSpaceBounds.TopLeft.X; x < partitionSpaceBounds.BottomRight.X; x++)
                {
                    if (x < 0 || x >= NumCellsX)
                    {
                        continue;
                    }

                    var cell = this[x, y];

                    if (cell.WorldSpaceBounds.Intersects(worldPosition, worldRadius))
                    {
                        yield return cell;
                    }
                }
            }
        }

        private Rectangle GetPartitionSpaceBounds(in Vector2 worldPosition, float worldRadius)
        {
            // Compute which cells are overlapped by this object.
            var minX = worldPosition.X - worldRadius;
            var maxX = worldPosition.X + worldRadius;
            var minY = worldPosition.Y - worldRadius;
            var maxY = worldPosition.Y + worldRadius;

            return WorldSpaceToPartitionSpace(
                new Vector2(minX, minY),
                new Vector2(maxX, maxY));
        }

        private Rectangle WorldSpaceToPartitionSpace(Vector2 worldSpaceMin, Vector2 worldSpaceMax)
        {
            var partitionSpaceMin = (worldSpaceMin - _terrainBoundary.TopLeft.ToVector2()) / _partitionCellSize;
            var partitionSpaceMax = (worldSpaceMax - _terrainBoundary.TopLeft.ToVector2()) / _partitionCellSize;

            return Rectangle.FromCorners(
                new Point2D((int)partitionSpaceMin.X, (int)partitionSpaceMin.Y),
                new Point2D((int)Math.Ceiling(partitionSpaceMax.X), (int)Math.Ceiling(partitionSpaceMax.Y)));
        }

        public IEnumerable<GameObject> QueryObjects<T>(
            GameObject searchObject,
            Vector3 searchPosition,
            float searchRadius,
            T query)
            where T : struct, IPartitionQuery
        {
            foreach (var cell in GetCells(searchPosition.Vector2XY(), searchRadius))
            {
                foreach (var partitionObject in cell.Objects)
                {
                    if (searchObject != partitionObject.GameObject &&
                        query.Evaluate(partitionObject.GameObject))
                    {
                        yield return partitionObject.GameObject;
                    }
                }
            }
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(2);

            var partitionCellSize = _partitionCellSize;
            reader.PersistSingle(ref partitionCellSize);
            if (partitionCellSize != _partitionCellSize)
            {
                throw new InvalidStateException();
            }

            reader.PersistArrayWithUInt32Length(
                _cells, static (StatePersister persister, ref PartitionCell item) =>
                {
                    persister.PersistObjectValue(item);
                },
                "PartitionCells");

#if DEBUG
            //var builder = new System.Text.StringBuilder();
            //for (var y = 0; y < _numCellsY; y++)
            //{
            //    for (var x = 0; x < _numCellsX; x++)
            //    {
            //        var cell = _cells[((_numCellsY - 1 - y) * _numCellsX) + x];
            //        var value = cell.Values[2];

            //        char c;
            //        if (value.State < 0)
            //        {
            //            c = (char)((-value.State) + '0');
            //        }
            //        else if (value.State == 0)
            //        {
            //            c = '-';
            //        }
            //        else if (value.State == 1)
            //        {
            //            c = '*';
            //        }
            //        else
            //        {
            //            throw new InvalidOperationException();
            //        }

            //        builder.Append(c);
            //    }
            //    builder.AppendLine();
            //}
            //File.WriteAllText($"Partition{Path.GetFileNameWithoutExtension(((FileStream) reader.Inner.BaseStream).Name)}.txt", builder.ToString());
#endif

            reader.PersistListWithUInt32Count(
                _shroudReveals,
                static (StatePersister persister, ref ShroudReveal item) =>
                {
                    item ??= new ShroudReveal();
                    persister.PersistObjectValue(item);
                });
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

    public sealed class PartitionCell : IPersistableObject
    {
        public readonly RectangleF WorldSpaceBounds;

        public readonly PartitionCellValue[] Values;

        /// <summary>
        /// Stores all the objects overlapping (or potentially overlapping) this cell.
        /// </summary>
        public readonly List<PartitionObject> Objects = new();

        internal PartitionCell(in RectangleF worldSpaceBounds)
        {
            WorldSpaceBounds = worldSpaceBounds;

            Values = new PartitionCellValue[Player.MaxPlayers];
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistArray(
                Values,
                static (StatePersister persister, ref PartitionCellValue item) =>
                {
                    persister.PersistInt16Value(ref item.State);

                    persister.SkipUnknownBytes(2);
                });
        }
    }

    public struct PartitionCellValue
    {
        public short State;
    }

    public sealed class ShroudReveal : IPersistableObject
    {
        public Vector3 Position;
        public float VisionRange;
        public ushort Unknown;
        public uint FrameSomething;

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistVector3(ref Position);
            reader.PersistSingle(ref VisionRange);
            reader.PersistUInt16(ref Unknown);
            reader.PersistFrame(ref FrameSomething);
        }
    }

    public sealed class PartitionObject
    {
        private readonly PartitionCellManager _manager;

        public readonly GameObject GameObject;

        /// <summary>
        /// Stores all the cells that this object overlaps (or potentially overlaps).
        /// </summary>
        public readonly HashSet<PartitionCell> OverlappingCells = new();

        public PartitionObject(PartitionCellManager manager, GameObject gameObject)
        {
            _manager = manager;
            GameObject = gameObject;
        }

        public void Remove()
        {
            _manager.RemovePartitionObject(this);
        }

        public void SetDirty()
        {
            _manager.SetDirty(this);
        }

        internal void UpdateOverlappingCells()
        {
            // Clear previous overlapping cells, and remove this object from those cells.
            foreach (var cell in OverlappingCells)
            {
                cell.Objects.Remove(this);
            }
            OverlappingCells.Clear();

            // Compute which cells are overlapped by this object.
            foreach (var overlappingCell in _manager.GetCells(GameObject.Translation.Vector2XY(), GameObject.Geometry.BoundingCircleRadius))
            {
                OverlappingCells.Add(overlappingCell);
                overlappingCell.Objects.Add(this);
            }
        }

        internal bool CollidesWith(PartitionObject otherObject)
        {
            if (GameObject.IsKindOf(ObjectKinds.NoCollide) || otherObject.GameObject.IsKindOf(ObjectKinds.NoCollide))
            {
                return false;
            }

            return GeometryCollisionDetectionUtility.Intersects(
                GeometryCollisionDetectionUtility.CreateCollideInfo(GameObject),
                GeometryCollisionDetectionUtility.CreateCollideInfo(otherObject.GameObject));
        }
    }

    public interface IPartitionQuery
    {
        bool Evaluate(GameObject queryObject);
    }

    public static class PartitionQueries
    {
        public readonly record struct TrueQuery
            : IPartitionQuery
        {
            public bool Evaluate(GameObject queryObject) => true;
        }

        public readonly record struct KindOfQuery(ObjectKinds kind)
            : IPartitionQuery
        {
            public bool Evaluate(GameObject queryObject) => queryObject.IsKindOf(kind);
        }

        public readonly record struct CollidesWithObjectQuery(GameObject GameObject)
            : IPartitionQuery
        {
            public bool Evaluate(GameObject queryObject)
            {
                return GeometryCollisionDetectionUtility.Intersects(
                    GeometryCollisionDetectionUtility.CreateCollideInfo(GameObject),
                    GeometryCollisionDetectionUtility.CreateCollideInfo(queryObject));
            }
        }
    }
}

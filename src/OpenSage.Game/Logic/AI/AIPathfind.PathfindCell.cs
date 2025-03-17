using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using SharpDX.X3DAudio;
using Vortice.DXGI;

#nullable enable

namespace OpenSage.Logic.AI;

using ZoneStorageType = ushort;

public partial class AIPathfind
{
    /// <summary>
    /// This represents one cell in the pathfinding grid.
    /// These cells categorize the world into idealized cellular states,
    /// and are also used for efficient A* pathfinding.
    /// TODO Optimize memory usage of pathfind grid.
    /// </summary>
    class PathfindCell : IDisposable
    {
        public enum CellType
        {
            /// <summary>
            ///  clear, unobstructed ground
            /// </summary>
            Clear = 0x00,

            /// <summary>
            /// water area
            /// </summary>
            Water = 0x01,

            /// <summary>
            /// steep altitude change
            /// </summary>
            Cliff = 0x02,

            /// <summary>
            /// Cell is occupied by rubble
            /// </summary>
            Rubble = 0x03,

            /// <summary>
            /// Occupied by a structure
            /// </summary>
            Obstacle = 0x04,

            /// <summary>
            /// Piece of a bridge that is impassable
            /// </summary>
            BridgeImpassable = 0x05,

            /// <summary>
            /// Just plain impassable except for aircraft
            /// </summary>
            Impassable = 0x06
        }

        public enum CellFlags
        {
            /// <summary>
            /// No units in this cell.
            /// </summary>
            NoUnits = 0x00,

            /// <summary>
            /// A unit is heading to this cell.
            /// </summary>
            UnitGoal = 0x01,

            /// <summary>
            /// A unit is moving through this cell.
            /// </summary>
            UnitPresentMoving = 0x02,

            /// <summary>
            /// A unit is stationary in this cell.
            /// </summary>
            UnitPresentFixed = 0x03,

            /// <summary>
            /// A unit is moving through this cell, and another unit has this as it's goal.
            /// </summary>
            UnitGoalOtherMoving = 0x05
        };

        private static readonly BitVector32.Section ZoneSection = BitVector32.CreateSection(16384);
        private static readonly BitVector32.Section AircraftGoalSection = BitVector32.CreateSection(1, ZoneSection);
        private static readonly BitVector32.Section PinchedSection = BitVector32.CreateSection(1, AircraftGoalSection);
        private static readonly BitVector32.Section TypeSection = BitVector32.CreateSection(4, PinchedSection);
        private static readonly BitVector32.Section FlagsSection = BitVector32.CreateSection(4, TypeSection);
        private static readonly BitVector32.Section ConnectsToLayerSection = BitVector32.CreateSection(4, FlagsSection);
        private static readonly BitVector32.Section LayerSection = BitVector32.CreateSection(4, ConnectsToLayerSection);

        private PathfindCellInfo? _info;
        private BitVector32 _data = new BitVector32();

        /// <summary>
        /// Zone. Each zone is a set of adjacent terrain type.  If from & to in the same zone, you can successfully pathfind.
        /// If not, you still may be able to if you can cross multiple terrain types.
        /// </summary>
        public ZoneStorageType Zone
        {
            get => (ZoneStorageType)_data[ZoneSection];
            set => _data[ZoneSection] = value;
        }

        /// <summary>
        /// This is an aircraft goal cell.
        /// </summary>
        public bool IsAircraftGoal
        {
            get => _data[AircraftGoalSection] != 0;
            private set => _data[AircraftGoalSection] = value ? 1 : 0;
        }

        /// <summary>
        /// This cell is surrounded by obstacle cells.
        /// </summary>
        public bool Pinched
        {
            get => _data[PinchedSection] != 0;
            set => _data[PinchedSection] = value ? 1 : 0;
        }

        /// <summary>
        /// what type of cell terrain this is.
        /// </summary>
        public CellType Type
        {
            get => (CellType)_data[TypeSection];
            set => _data[TypeSection] = (short)value;
        }

        /// <summary>
        /// what type of units are in or moving through this cell.
        /// </summary>
        public CellFlags Flags
        {
            get => (CellFlags)_data[FlagsSection];
            set => _data[FlagsSection] = (short)value;
        }

        /// <summary>
        /// This cell can pathfind onto this layer, if > LAYER_TOP.
        /// </summary>
        public PathfindLayerType ConnectsToLayer
        {
            get => (PathfindLayerType)_data[ConnectsToLayerSection];
            set => _data[ConnectsToLayerSection] = (short)value;
        }

        /// <summary>
        /// Layer of this cell.
        /// </summary>
        public PathfindLayerType Layer
        {
            get => (PathfindLayerType)_data[LayerSection];
            set => _data[LayerSection] = (short)value;
        }

        /// <summary>
        /// return true if the given object ID is registered as an obstacle in this cell
        /// </summary>
        bool IsObstaclePresent(uint objectId)
        {

        }

        /// <summary>
        /// return true if the obstacle in the cell is KINDOF_CAN_SEE_THROUGHT_STRUCTURE
        /// </summary>
        bool IsObstacleTransparent => _info?.ObstacleIsTransparent ?? false;

        /// <summary>
        /// return true if the given obstacle in the cell is a fence
        /// </summary>
        bool IsObstacleFence => _info?.ObstacleIsFence ?? false;

        public PathfindCell()
        {
            Reset();
        }

        public void Dispose()
        {
            if (_info is not null)
            {
                PathfindCellInfo.ReleaseACellInfo(_info);
            }

            _info = null;
        }

        public void Reset()
        {
            Type = CellType.Clear;
            Flags = CellFlags.NoUnits;
            Zone = 0;
            IsAircraftGoal = false;
            Pinched = false;
            if (_info is not null)
            {
                _info._obstacleId = PathfindCellInfo.InvalidId;
                PathfindCellInfo.ReleaseACellInfo(_info);
                _info = null;
            }

            ConnectsToLayer = PathfindLayerType.Invalid;
            Layer = PathfindLayerType.Ground;
        }

        /// <summary>
        /// flag this cell as an obstacle, from the given one
        /// </summary>
        public bool SetTypeAsObstacle(GameObject obstacle, bool isFence, Vector2 position)
        {
        }

        /// <summary>
        /// unflag this cell as an obstacle, from the given one
        /// </summary>
        public bool RemoveObstacle(GameObject obstacle)
        {
        }

        /// <summary>
        /// Return estimated cost from given cell to reach goal cell
        /// </summary>
        public uint CostToGoal(PathfindCell goal)
        {}

        public uint GetCostToHierGoal(PathfindCell goal);

        public uint GetCostSoFar(PathfindCell parent);

        /// <summary>
        /// put self on "open" list in ascending cost order, return new list
        /// </summary>
        PathfindCell PutOnSortedOpenList(PathfindCell list);

        /// <summary>
        /// remove self from "open" list
        /// </summary>
        PathfindCell RemoveFromOpenList(PathfindCell list);

        /// <summary>
        /// put self on "closed" list, return new list
        /// </summary>
        PathfindCell PutOnClosedList(PathfindCell list);

        /// <summary>
        /// Remove self from "closed" list
        /// </summary>
        PathfindCell RemoveFromClosedList(PathfindCell list);

        /// <summary>
        /// remove all cells from closed list.
        /// </summary>
        static int ReleaseClosedList(PathfindCell list);

        /// <summary>
        /// remove all cells from closed list.
        /// </summary>
        static int ReleaseOpenList(PathfindCell list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PathfindCell? GetNextOpen() =>
            _info?._nextOpen?._cell;

        public ushort XIndex =>
            (ushort)_info!._position.X;

        public ushort YIndex =>
            (ushort)_info!._position.Y;

        public bool IsBlockedByAlly
        {
            get => _info!.BlockedByAlly;
            set => _info!.BlockedByAlly = value;
        }

        public bool Open =>
            _info!.Open;

        public bool Closed =>
            _info!.Closed;

        public uint CostSoFar
        {
            get => _info!._costSoFar;
            set => _info!._costSoFar = (ushort)value;
        }

        public uint TotalCost
        {
            get => _info!._totalCost;
            set => _info!._totalCost = (ushort)value;
        }

        public PathfindCell? ParentCell
        {
            get => _info?._pathParent?._cell;
            set
            {
                Debug.Assert(_info is not null, "Has to have info.");
                _info._pathParent = value?._info;
                int dx = (int)(_info._position.X - value!._info!._position.X);
                int dy = (int)(_info._position.Y - value!._info!._position.Y);

                if (dx < -1 || dx > 1 || dy < -1 || dy > 1)
                {
                    Debug.Fail("Invalid parent index.");
                }
            }
        }

        /// <summary>
        /// Reset the parent cell.
        /// </summary>
        public void ClearParentCell()
        {
            Debug.Assert(_info is not null, "Has to have info.");
            _info._pathParent = null;
        }

        /// <summary>
        /// Set the parent pointer.
        /// </summary>
        public void SetParentCellHierarchical(PathfindCell parent)
        {
            Debug.Assert(_info is not null, "Has to have info.");
            _info._pathParent = parent._info;
        }

        /// <summary>
        /// Reset the pathfinding values in the cell.
        /// </summary>
	    public bool StartPathfind(PathfindCell goalCell)
        {
            Debug.Assert(_info is not null, "Has to have info.");
            _info._nextOpen = null;
            _info._prevOpen = null;
            _info._pathParent = null;
            _info._costSoFar = 0; // start node, no cost to get here
            _info._totalCost = 0;
            if (goalCell is not null)
            {
                _info._totalCost = (ushort)CostToGoal(goalCell);
            }

            _info.Open = true;
            _info.Closed = false;

            return true;
        }

        public bool AllocateInfo(Vector2 position);
        public void ReleaseInfo();

        bool HasInfo => _info is not null;

        public void SetGoalUnit(uint unit, Vector2 position);
        public void SetGoalAircraft(uint unit, Vector2 position);
        public void SetPosUnit(uint unit, Vector2 position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetGoalUnit() =>
            _info?._goalUnitId ?? PathfindCellInfo.InvalidId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetGoalAircraft() =>
            _info?._goalAircraftId ?? PathfindCellInfo.InvalidId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetPosUnit() =>
            _info?._posUnitId ?? PathfindCellInfo.InvalidId;
    }
}

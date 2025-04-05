#define NO_REAL_DIST

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Terrain;
using SharpDX.X3DAudio;
using Veldrid.MetalBindings;
using Vortice.Direct3D11;
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
    public class PathfindCell : IDisposable
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

        private const int CostOrthogonal = 10;
        private const int CostDiagonal = 14;
        private const float CostToDistanceFactor = 1.0f / 10.0f;
        private const float CostToDistanceFactorSqr = CostToDistanceFactor * CostToDistanceFactor;

        private const uint ZoneMask             = 0b00000000_00000000_00111111_11111111; // 14 bits
        private const uint AircraftGoalMask     = 0b00000000_00000000_01000000_00000000; //  1 bit
        private const uint PinchedMask          = 0b00000000_00000000_10000000_00000000; //  1 bit
        private const uint TypeMask             = 0b00000000_00001111_00000000_00000000; //  4 bits
        private const uint FlagsMask            = 0b00000000_11110000_00000000_00000000; //  4 bits
        private const uint ConnectsToLayerMask  = 0b00001111_00000000_00000000_00000000; //  4 bits
        private const uint LayerMask            = 0b11110000_00000000_00000000_00000000; //  4 bits

        private PathfindCellInfo? _info;
        private uint _data = 0;

        /// <summary>
        /// Zone. Each zone is a set of adjacent terrain type.  If from & to in the same zone, you can successfully pathfind.
        /// If not, you still may be able to if you can cross multiple terrain types.
        /// </summary>
        public ZoneStorageType Zone
        {
            get => (ZoneStorageType)(_data & ZoneMask);
            set => _data = (_data & ~ZoneMask) | (value & ZoneMask);
        }

        /// <summary>
        /// This is an aircraft goal cell.
        /// </summary>
        public bool IsAircraftGoal
        {
            get => (_data & AircraftGoalMask) == AircraftGoalMask;
            private set => _data = value ? (_data | AircraftGoalMask) : (_data & ~AircraftGoalMask);
        }

        /// <summary>
        /// This cell is surrounded by obstacle cells.
        /// </summary>
        public bool Pinched
        {
            get => (_data & PinchedMask) == PinchedMask;
            set => _data = value ? (_data | PinchedMask) : (_data & ~PinchedMask);
        }

        /// <summary>
        /// what type of cell terrain this is.
        /// </summary>
        public CellType Type
        {
            get => (CellType)((_data & TypeMask) >>> 16);
            set
            {
                if (_info is not null && _info._obstacleId != ObjectId.Invalid)
                {
                    DebugUtility.AssertCrash(value == CellType.Obstacle, "Wrong type.");
                }

                _data = (_data & ~TypeMask) | (((uint)value << 16) & TypeMask);
            }
        }

        /// <summary>
        /// what type of units are in or moving through this cell.
        /// </summary>
        public CellFlags Flags
        {
            get => (CellFlags)((_data & FlagsMask) >>> 20);
            set => _data = (_data & ~FlagsMask) | (((uint)value << 20) & FlagsMask);
        }

        /// <summary>
        /// This cell can pathfind onto this layer, if > LAYER_TOP.
        /// </summary>
        public PathfindLayerType ConnectsToLayer
        {
            get => (PathfindLayerType)((_data & ConnectsToLayerMask) >>> 24);
            set => _data = (_data & ~ConnectsToLayerMask) | (((uint)value << 24) & ConnectsToLayerMask);
        }

        /// <summary>
        /// Layer of this cell.
        /// </summary>
        public PathfindLayerType Layer
        {
            get => (PathfindLayerType)((_data & LayerMask) >>> 28);
            set => _data = (_data & ~LayerMask) | (((uint)value << 28) & LayerMask);
        }

        /// <summary>
        /// return true if the given object ID is registered as an obstacle in this cell
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsObstaclePresent(ObjectId objectId)
        {
            if (objectId != ObjectId.Invalid && Type == CellType.Obstacle)
            {
                DebugUtility.AssertCrash(_info is not null, "Should have info to be obstacle.");
                return _info is not null && _info._obstacleId == objectId;
            }

            return false;
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
                _info._obstacleId = ObjectId.Invalid;
                PathfindCellInfo.ReleaseACellInfo(_info);
                _info = null;
            }

            ConnectsToLayer = PathfindLayerType.Invalid;
            Layer = PathfindLayerType.Ground;
        }

        /// <summary>
        /// Flag this cell as an obstacle, from the given one.
        /// Return true if cell was flagged.
        /// </summary>
        public bool SetTypeAsObstacle(GameObject obstacle, bool isFence, Point2D position)
        {
            if (Type != CellType.Clear && Type != CellType.Impassable)
            {
                return false;
            }

            bool isRubble = false;
            if (obstacle.BodyModule?.DamageState == BodyDamageType.Rubble)
            {
                isRubble = true;
            }

            if (isRubble)
            {
                Type = CellType.Rubble;
                if (_info is not null)
                {
                    _info._obstacleId = ObjectId.Invalid;
                    ReleaseInfo();
                }

                return true;
            }

            Type = CellType.Obstacle;
            if (_info is null)
            {
                _info = PathfindCellInfo.GetACellInfo(this, position);
                if (_info is null)
                {
                    DebugUtility.Crash("Not enough PathFindCellInfos in pool.");
                    return false;
                }
            }

            _info._obstacleId = obstacle.Id;
            _info.ObstacleIsFence = isFence;
            _info.ObstacleIsTransparent = obstacle.IsKindOf(ObjectKinds.CanSeeThroughStructure);

            return true;
        }

        /// <summary>
        /// Unflag this cell as an obstacle, from the given one.
        /// Return true if this cell was previously flagged as an obstacle by this object.
        /// </summary>
        public bool RemoveObstacle(GameObject obstacle)
        {
            if (Type == CellType.Rubble)
            {
                Type = CellType.Clear;
            }

            if (_info is null)
            {
                return false;
            }

            if (_info._obstacleId != obstacle.Id)
            {
                return false;
            }

            Type = CellType.Clear;
            _info._obstacleId = ObjectId.Invalid;
            ReleaseInfo();

            return true;
        }

        /// <summary>
        /// Return estimated cost from given cell to reach goal cell
        /// </summary>
        public uint CostToGoal(PathfindCell goal)
        {
            DebugUtility.AssertCrash(_info is not null, "Has to have info.");
            int dx = _info._position.X - goal.XIndex;
            int dy = _info._position.Y - goal.YIndex;

#if REAL_DIST
            int cost = (int)(CostOrthogonal * MathF.Sqrt(dx * dx + dy * dy));
#else
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            int cost;
            if (dx > dy)
            {
                cost = CostOrthogonal * dx + (CostOrthogonal * dy) / 2;
            }
            else
            {
                cost = CostOrthogonal * dy + (CostOrthogonal * dx) / 2;
            }

#endif
            return (uint)cost;
        }

        public uint GetCostToHierGoal(PathfindCell goal)
        {
            if (_info is null)
            {
                DebugUtility.Crash("Has to have info.");
                return 100000; //...patch hack 1.01
            }

            int dx = (int)_info._position.X - goal.XIndex;
            int dy = (int)_info._position.Y - goal.YIndex;
            int cost = (int)(CostOrthogonal * MathF.Sqrt(dx * dx + dy * dy) + 0.5f);
            return (uint)cost;
        }

        /// <summary>
        /// PORT: The original C++ code contained a property (methods "getCostSoFar"/"setCostSoFar")
        /// and this method called "costSoFar". Since in C# the property is called "CostSoFar",
        /// we need to rename this method to avoid a naming conflict.        
        /// </summary>
        public uint GetCostSoFar(PathfindCell parent)
        {
            DebugUtility.AssertCrash(_info is not null, "Has to have info.");
            
            // very first node in path - no turns, no cost
            if (parent is null)
            {
                return 0;
            }

            // add in number of turns in path so far
            Point2D prevDir = new(
                parent.XIndex - _info._position.X,
                parent.YIndex - _info._position.Y);

            uint cost;

            // diagonal moves cost a bit more than orthogonal ones
            if (prevDir.X == 0f || prevDir.Y == 0f)
            {
                cost = parent.CostSoFar + CostOrthogonal;
            }
            else
            {
                cost = parent.CostSoFar + CostDiagonal;
            }

            if (Pinched)
            {
                cost += 1 * CostDiagonal;
            }

	        // Increase cost of turns.
	        uint numTurns = 0;
	        PathfindCell? prevCell = parent.ParentCell;
	        if (prevCell is not null)
            {
                Point2D dir = new(
                    prevCell.XIndex - parent.XIndex,
                    prevCell.YIndex - parent.YIndex);
		
		        // count number of direction changes
		        if (dir.X != prevDir.X || dir.Y != prevDir.Y)
		        {
                    int dot = dir.X * prevDir.X + dir.Y * prevDir.Y;
			        if (dot > 0)
                    {
				        numTurns = 4; // 45 degree turn
                    }
			        else if (dot == 0)
                    {
				        numTurns = 8; // 90 degree turn
                    }
			        else
                    {
				        numTurns = 16; // 135 degree turn
                    }
		        }
	        }

	        return cost + numTurns;
        }

        /// <summary>
        /// put self on "open" list in ascending cost order, return new list
        /// </summary>
        public PathfindCell PutOnSortedOpenList(PathfindCell list)
        {
            DebugUtility.AssertCrash(_info is not null, "Has to have info.");
            DebugUtility.AssertCrash(!_info.Closed && !_info.Open, "Serious error - Invalid flags. jba");
            if (list is null)
            {
                list = this;
                _info._prevOpen = null;
                _info._nextOpen = null;
            }
            else
            {
                // insertion sort
                PathfindCell? c, lastCell = null;
                for (c = list; c is not null; c = c.GetNextOpen())
                {
                    if (c._info._totalCost > _info._totalCost)
                    {
                        break;
                    }

                    lastCell = c;
                }

                if (c is not null)
                {
                    // insert just before "c"
                    if (c._info._prevOpen is not null)
                    {
                        c._info._prevOpen._nextOpen = this._info;
                    }
                    else
                    {
                        list = this;
                    }

                    _info._prevOpen = c._info._prevOpen;
                    c._info._prevOpen = this._info;

                    _info._nextOpen = c._info;

                }
                else
                {
                    // append after "lastCell" - end of list
                    lastCell!._info!._nextOpen = this._info;
                    _info._prevOpen = lastCell._info;
                    _info._nextOpen = null;
                }
            }

            // mark newCell as being on open list
            _info.Open = true;
            _info.Closed = false;

            return list;
        }

        /// <summary>
        /// remove self from "open" list
        /// </summary>
        public PathfindCell RemoveFromOpenList(PathfindCell list)
        {
            DebugUtility.AssertCrash(_info is not null, "Has to have info.");
            DebugUtility.AssertCrash(!_info.Closed && _info.Open, "Serious error - Invalid flags. jba");

            if (_info._nextOpen is not null)
            {
                _info._nextOpen._prevOpen = _info._prevOpen;
            }

            if (_info._prevOpen is not null)
            {
                _info._prevOpen._nextOpen = _info._nextOpen;
            }
            else
            {
                list = GetNextOpen()!;
            }

            _info.Open = false;
            _info._nextOpen = null;
            _info._prevOpen = null;

            return list;
        }

        /// <summary>
        /// put self on "closed" list, return new list
        /// </summary>
        public PathfindCell PutOnClosedList(PathfindCell list)
        {
            DebugUtility.AssertCrash(_info is not null, "Has to have info.");
            DebugUtility.AssertCrash(!_info.Closed && !_info.Open, "Serious error - Invalid flags. jba");

            // only put on list if not already on it
            if (_info.Closed == false)
            {
                _info.Closed = false;
                _info.Closed = true;

                _info._prevOpen = null;
                _info._nextOpen = list?._info;
                if (list is not null)
                {
                    list._info!._prevOpen = this._info;
                }

                list = this;
            }

            return list;
        }

        /// <summary>
        /// Remove self from "closed" list
        /// </summary>
        public PathfindCell RemoveFromClosedList(PathfindCell list)
        {
            DebugUtility.AssertCrash(_info is not null, "Has to have info.");
            DebugUtility.AssertCrash(_info.Closed && !_info.Open, "Serious error - Invalid flags. jba");

            if (_info._nextOpen is not null)
            {
                _info._nextOpen._prevOpen = _info._prevOpen;
            }

            if (_info._prevOpen is not null)
            {
                _info._prevOpen._nextOpen = _info._nextOpen;
            }
            else
            {
                list = GetNextOpen()!;
            }

            _info.Closed = false;
            _info._nextOpen = null;
            _info._prevOpen = null;

            return list;
        }

        /// <summary>
        /// remove all cells from "closed" list.
        /// </summary>
        public static int ReleaseClosedList(PathfindCell? list)
        {
            int count = 0;
            while (list is not null)
            {
                count++;

                DebugUtility.AssertCrash(list._info is not null, "Has to have info.");
                DebugUtility.AssertCrash(list._info.Closed && !list._info.Open, "Serious error - Invalid flags. jba");

                PathfindCell? cur = list;
                PathfindCellInfo curInfo = list._info;
                if (curInfo._nextOpen is not null)
                {
                    list = curInfo._nextOpen._cell;
                }
                else
                {
                    list = null;
                }

                DebugUtility.AssertCrash(cur == curInfo._cell, "Bad backpointer in PathfindCellInfo");
                curInfo._nextOpen = null;
                curInfo._prevOpen = null;
                curInfo.Closed = false;
                cur.ReleaseInfo();
            }
            return count;
        }

        /// <summary>
        /// remove all cells from "open" list.
        /// </summary>
        public static int ReleaseOpenList(PathfindCell? list)
        {
            int count = 0;
            while (list is not null)
            {
                count++;

                DebugUtility.AssertCrash(list._info is not null, "Has to have info.");
                DebugUtility.AssertCrash(!list._info.Closed && list._info.Open, "Serious error - Invalid flags. jba");

                PathfindCell cur = list;
                PathfindCellInfo curInfo = list._info;
                if (curInfo._nextOpen is not null)
                {
                    list = curInfo._nextOpen._cell;
                }
                else
                {
                    list = null;
                }

                DebugUtility.AssertCrash(cur == curInfo._cell, "Bad backpointer in PathfindCellInfo");

                curInfo._nextOpen = null;
                curInfo._prevOpen = null;
                curInfo.Open = false;
                cur.ReleaseInfo();
            }

            return count;
        }

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
                DebugUtility.AssertCrash(_info is not null, "Has to have info.");
                _info._pathParent = value?._info;
                int dx = (int)(_info._position.X - value!._info!._position.X);
                int dy = (int)(_info._position.Y - value!._info!._position.Y);

                if (dx < -1 || dx > 1 || dy < -1 || dy > 1)
                {
                    DebugUtility.Crash("Invalid parent index.");
                }
            }
        }

        /// <summary>
        /// Reset the parent cell.
        /// </summary>
        public void ClearParentCell()
        {
            DebugUtility.AssertCrash(_info is not null, "Has to have info.");
            _info._pathParent = null;
        }

        /// <summary>
        /// Set the parent pointer.
        /// </summary>
        public void SetParentCellHierarchical(PathfindCell parent)
        {
            DebugUtility.AssertCrash(_info is not null, "Has to have info.");
            _info._pathParent = parent._info;
        }

        /// <summary>
        /// Reset the pathfinding values in the cell.
        /// </summary>
	    public bool StartPathfind(PathfindCell goalCell)
        {
            DebugUtility.AssertCrash(_info is not null, "Has to have info.");
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

        /// <summary>
        /// Allocates an info record for a cell.
        /// </summary>
        public bool AllocateInfo(Point2D position)
        {
            if (_info is null)
            {
                _info = PathfindCellInfo.GetACellInfo(this, position);
                return _info is not null;
            }

            return true;
        }

        /// <summary>
        /// Releases an info record for a cell.
        /// </summary>
        public void ReleaseInfo()
        {
            if (Type == CellType.Obstacle)
            {
                return;
            }

            if (Flags != CellFlags.NoUnits)
            {
                return;
            }

            if (IsAircraftGoal)
            {
                return;
            }

            if (_info is not null)
            {
                DebugUtility.AssertCrash(_info._prevOpen is null && _info._nextOpen is null, "Shouldn't be linked.");
                DebugUtility.AssertCrash(_info.Open && _info.Closed, "Shouldn't be linked.");
                DebugUtility.AssertCrash(_info._goalUnitId == ObjectId.Invalid && _info._posUnitId == ObjectId.Invalid, "Shouldn't be occupied.");
                DebugUtility.AssertCrash(_info._goalAircraftId == ObjectId.Invalid, "Shouldn't be occupied by aircraft.");
                if (_info._prevOpen is not null || _info._nextOpen is not null || _info.Open || _info.Closed)
                {
                    // Bad release. Skip for now, better leak than crash. jba.
                    return;
                }

                PathfindCellInfo.ReleaseACellInfo(_info);
                _info = null;
            }
        }

        bool HasInfo => _info is not null;

        /// <summary>
        /// Sets the goal unit into the info record for a cell.
        /// </summary>
        public void SetGoalUnit(ObjectId unitId, Point2D position)
        {
            if (unitId == ObjectId.Invalid)
            {
                // removing goal.
                if (_info is not null)
                {
                    _info._goalUnitId = ObjectId.Invalid;
                    if (_info._posUnitId == ObjectId.Invalid)
                    {
                        // No units here.
                        DebugUtility.AssertCrash(Flags == CellFlags.UnitGoal, "Bad flags.");
                        Flags = CellFlags.NoUnits;
                        ReleaseInfo();
                    }
                    else
                    {
                        Flags = CellFlags.UnitPresentMoving;
                    }
                }
                else
                {
                    DebugUtility.AssertCrash(Flags == CellFlags.NoUnits, "Bad flags.");
                }
            }
            else
            {
                // adding goal.
                if (_info is null)
                {
                    DebugUtility.AssertCrash(Flags == CellFlags.NoUnits, "Bad flags.");
                    AllocateInfo(position);
                }

                if (_info is null)
                {
                    DebugUtility.Crash("Ran out of pathfind cells - fatal error!!!!! jba.");
                    return;
                }

                _info._goalUnitId = unitId;

                if (unitId == _info._posUnitId)
                {
                    Flags = CellFlags.UnitPresentFixed;
                }
                else if (_info._posUnitId == ObjectId.Invalid)
                {
                    Flags = CellFlags.UnitGoal;
                }
                else
                {
                    Flags = CellFlags.UnitGoalOtherMoving;
                }
            }
        }

        /// <summary>
        /// Sets the goal aircraft into the info record for a cell.
        /// </summary>
        public void SetGoalAircraft(ObjectId unitId, Point2D position)
        {
            if (unitId == ObjectId.Invalid)
            {
                // removing goal.
                if (_info is not null)
                {
                    _info._goalAircraftId = ObjectId.Invalid;
                    IsAircraftGoal = false;
                    ReleaseInfo();
                }
                else
                {
                    DebugUtility.AssertCrash(!IsAircraftGoal, "Bad flags.");
                }
            }
            else
            {
                // adding goal.
                if (_info is null)
                {
                    DebugUtility.AssertCrash(!IsAircraftGoal, "Bad flags.");
                    AllocateInfo(position);
                }

                if (_info is null)
                {
                    DebugUtility.Crash("Ran out of pathfind cells - fatal error!!!!! jba.");
                    return;
                }

                _info._goalAircraftId = unitId;
                IsAircraftGoal = true;
            }
        }

        /// <summary>
        /// Sets the position unit into the info record for a cell.
        /// </summary>
        public void SetPosUnit(ObjectId unitId, Point2D position)
        {
            if (unitId == ObjectId.Invalid)
            {
                // removing position.
                if (_info is not null)
                {
                    _info._posUnitId = ObjectId.Invalid;
                    if (_info._goalUnitId == ObjectId.Invalid)
                    {
                        // No units here.
                        DebugUtility.AssertCrash(Flags == CellFlags.UnitPresentMoving, "Bad flags.");
                        Flags = CellFlags.NoUnits;
                        ReleaseInfo();
                    }
                    else
                    {
                        Flags = CellFlags.UnitGoal;
                    }
                }
                else
                {
                    DebugUtility.AssertCrash(Flags == CellFlags.NoUnits, "Bad flags.");
                }
            }
            else
            {
                // adding goal.
                if (_info is null)
                {
                    DebugUtility.AssertCrash(Flags == CellFlags.NoUnits, "Bad flags.");
                    AllocateInfo(position);
                }

                if (_info is null)
                {
                    DebugUtility.Crash("Ran out of pathfind cells - fatal error!!!!! jba.");
                    return;
                }

                if (_info._goalUnitId != ObjectId.Invalid && (_info._goalUnitId == _info._posUnitId))
                {
                    // A unit is already occupying this cell.
                    return;
                }

                _info._posUnitId = unitId;
                if (unitId == _info._goalUnitId)
                {
                    Flags = CellFlags.UnitPresentFixed;
                }
                else if (_info._goalUnitId == ObjectId.Invalid)
                {
                    Flags = CellFlags.UnitPresentMoving;
                }
                else
                {
                    Flags = CellFlags.UnitGoalOtherMoving;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectId GetGoalUnit() =>
            _info?._goalUnitId ?? ObjectId.Invalid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectId GetGoalAircraft() =>
            _info?._goalAircraftId ?? ObjectId.Invalid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectId GetPosUnit() =>
            _info?._posUnitId ?? ObjectId.Invalid;
    }
}

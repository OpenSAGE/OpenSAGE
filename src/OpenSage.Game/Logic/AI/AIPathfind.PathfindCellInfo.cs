using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

#nullable enable

namespace OpenSage.Logic.AI;

public partial class AIPathfind
{
    class PathfindCellInfo
    {
        /// <summary>
        /// Number of cells we will search pathfinding per frame.
        /// </summary>
        public const int PathfindCellsPerFrame = 5000;

        public const int CellInfosToAllocate = 30000;

        protected static PathfindCellInfo[]? InfoArray;
        protected static PathfindCellInfo? FirstFree;

        /// <summary>
        /// Allocates a pool of pathfind cell infos.
        /// </summary>
        public static void AllocateCellInfos()
        {
            ReleaseCellInfos();
            InfoArray = new PathfindCellInfo[CellInfosToAllocate];
            InfoArray[CellInfosToAllocate - 1]._pathParent = null;
            InfoArray[CellInfosToAllocate - 1].IsFree = true;
            FirstFree = InfoArray[0];

            for (var i = 0; i < CellInfosToAllocate - 1; i++)
            {
                InfoArray[i]._pathParent = InfoArray[i + 1];
                InfoArray[i].IsFree = true;
            }
        }

        /// <summary>
        /// Releases a pool of pathfind cell infos.
        /// </summary>
        public static void ReleaseCellInfos()
        {
            if (InfoArray is null)
            {
                return; // haven't allocated any yet.
            }

            int count = 0;

            while (FirstFree is not null)
            {
                count++;
                DebugUtility.AssertCrash(FirstFree.IsFree, "Should be free");
                FirstFree = FirstFree._pathParent;
                count++;
            }

            DebugUtility.AssertCrash(count == CellInfosToAllocate, "Error - Allocated cellinfos");

            InfoArray = null;
            FirstFree = null;
        }

        public static PathfindCellInfo? GetACellInfo(PathfindCell cell, Point2D position)
        {
            PathfindCellInfo? info = FirstFree;
            if (FirstFree is not null)
            {
                DebugUtility.AssertCrash(FirstFree.IsFree, "Should be free");
                FirstFree = FirstFree._pathParent;
                info!.IsFree = false; // Just allocated it.
                info._cell = cell;
                info._position = position;

                info._nextOpen = null;
                info._prevOpen = null;
                info._pathParent = null;
                info._costSoFar = 0;
                info._totalCost = 0;
                info.Open = false;
                info.Closed = false;
                info._obstacleId = ObjectId.Invalid;
                info._goalUnitId = ObjectId.Invalid;
                info._posUnitId = ObjectId.Invalid;
                info._goalAircraftId = ObjectId.Invalid;
                info.ObstacleIsFence = false;
                info.ObstacleIsTransparent = false;
                info.BlockedByAlly = false;
            }

            return info;
        }

        /// <summary>
        /// Returns a pathfindcellinfo.
        /// </summary>        
        public static void ReleaseACellInfo(PathfindCellInfo info)
        {
            DebugUtility.AssertCrash(!info.IsFree, "Shouldn't be free");

            //TODO -fix this assert on usa04. jba.
            //DebugUtility.AssertCrash(info._obstacleId == 0, "Shouldn't be obstacle");

            info._pathParent = FirstFree;
            FirstFree = info;
            info.IsFree = true;
        }

        /// <summary>
        /// for A* "open" list, shared by closed list
        /// </summary>
        internal PathfindCellInfo? _nextOpen;

        /// <summary>
        /// for A* "open" list, shared by closed list
        /// </summary>
        internal PathfindCellInfo? _prevOpen;

        /// <summary>
        /// "parent" cell from pathfinder
        /// </summary>
        internal PathfindCellInfo? _pathParent;

        /// <summary>
        /// Cell this info belongs to currently.
        /// </summary>
        internal PathfindCell? _cell;

        /// <summary>
        /// cost estimates for A* search
        /// </summary>
        internal ushort _totalCost;

        /// <summary>
        /// cost estimates for A* search
        /// </summary>
        internal ushort _costSoFar;

        /// <summary>
        /// have to include cell's coordinates, since cells are often accessed via pointer only
        /// </summary>
        internal Point2D _position;

        /// <summary>
        /// The objectID of the ground unit whose goal this is.
        /// </summary>
        internal ObjectId _goalUnitId;

        /// <summary>
        /// The objectID of the ground unit that is occupying this cell.
        /// </summary>
        internal ObjectId _posUnitId;

        /// <summary>
        /// The objectID of the aircraft whose goal this is.
        /// </summary>
        internal ObjectId _goalAircraftId;

        /// <summary>
        /// the object ID who overlaps this cell
        /// </summary>
        internal ObjectId _obstacleId;

        protected BitArray _flags = new BitArray(sizeof(uint) * 8);

        internal bool IsFree
        {
            get => _flags.Get(0);
            set => _flags.Set(0, value);
        }

        /// <summary>
        /// True if this cell is blocked by an allied unit.
        /// </summary
        internal bool BlockedByAlly
        {
            get => _flags.Get(1);
            set => _flags.Set(1, value);
        }

        /// <summary>
        /// True if occupied by a fence.
        /// </summary>
        internal bool ObstacleIsFence
        {
            get => _flags.Get(2);
            set => _flags.Set(2, value);
        }

        /// <summary>
        /// True if obstacle is transparent (undefined if obstacleid is invalid)
        /// </summary>
        internal bool ObstacleIsTransparent
        {
            get => _flags.Get(3);
            set => _flags.Set(3, value);
        }

        // TODO Do we need both mark values in this cell?  Can't store a single value and compare it?
        internal bool Open
        {
            get => _flags.Get(4);
            set => _flags.Set(4, value);
        }

        internal bool Closed
        {
            get => _flags.Get(5);
            set => _flags.Set(5, value);
        }
    }
}

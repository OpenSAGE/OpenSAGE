using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Map;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Terrain;
using Veldrid.MetalBindings;

#nullable enable

namespace OpenSage.Logic.AI;

public partial class AIPathfind
{

    class PathfindLayer
    {
        /// <summary>
        /// Pathfinding map - contains iconic representation of the map
        /// </summary>
        private PathfindCell[]?    _blockOfMapCells;

        /// <summary>
        /// Pathfinding map indexes - contains matrix indexing into the map.
        /// </summary>
        private PathfindCell[][]? _layerCells;

        /// <summary>
        /// Number of cells in x
        /// </summary>
        private int _width;

        /// <summary>
        /// Number of cells in y
        /// </summary>
        private int _height;

        /// <summary>
        /// Index of first cell in x
        /// </summary>
        private int _xOrigin;

        /// <summary>
        /// Index of first cell in y
        /// </summary>
        private int _yOrigin;

        /// <summary>
        /// pathfind cell indexes for center cell on the from side.
        /// </summary>
        private Vector2 _startCell = new Vector2(-1, -1);

        /// <summary>
        /// pathfind cell indexes for center cell on the to side.
        /// </summary>
        private Vector2 _endCell = new Vector2(-1, -1);

        private PathfindLayerType _layer;

        /// <summary>
        /// Whole bridge is in same zone.
        /// </summary>
        public int Zone { get; set; }
        
        /// <summary>
        /// Corresponding bridge in TerrainLogic.
        /// </summary>
        private Bridge? _bridge;
        private bool _destroyed;

        /// <summary>
        /// Returns true if the layer is avaialble for use.
        /// </summary>
        public void Reset()
        {
            _bridge = null;

            if (_layerCells is not null)
            {
                int i, j;
                for (i = 0; i < _width; i++)
                {
                    for (j = 0; j < _height; j++)
                    {
                        PathfindCell cell = _layerCells[i][j];
                        cell.Reset();
                    }
                }

                _layerCells = null;
            }

            if (_blockOfMapCells is not null)
            {
                _blockOfMapCells = null;
            }

            _width = 0;
            _height = 0;
            _xOrigin = 0;
            _yOrigin = 0;
            _startCell = new Vector2(-1, -1);
            _endCell = new Vector2(-1, -1);
            _layer = PathfindLayerType.Ground;
        }

        /// <summary>
        /// Sets the bridge & layer number for a layer.
        /// </summary>
        public bool Init(Bridge bridge, PathfindLayerType layer)
        {
            if (_bridge is not null)
            {
                return false;
            }

            _bridge = bridge;
            _layer = layer;
            _destroyed = false;

            return true;
        }

        /// <summary>
        /// Allocates the pathfind cells for the bridge layer.
        /// </summary>
        public void AllocateCells(IRegion2D* extent)
        {
            if (_bridge is null)
            {
                return;
            }

            Region2D bridgeBounds = _bridge.GetBounds();
            int maxX, maxY;
            _xOrigin = (int)((bridgeBounds.lo.x - PathfindCellSize / 100) / PathfindCellSize);
            _yOrigin = (int)((bridgeBounds.lo.y - PathfindCellSize / 100) / PathfindCellSize);
            _width = 0;
            _height = 0;
            maxX = (int)MathF.Ceiling((bridgeBounds.hi.x + PathfindCellSize / 100) / PathfindCellSize);
            maxY = (int)MathF.Ceiling((bridgeBounds.hi.y + PathfindCellSize / 100) / PathfindCellSize);
            // Pad with 1 extra;
            _xOrigin--;
            _yOrigin--;
            maxX++;
            maxY++;

            if (_xOrigin < extent->lo.x) _xOrigin = extent->lo.x;
            if (_yOrigin < extent->lo.y) _yOrigin = extent->lo.y;
            if (maxX > extent->hi.x) maxX = extent->hi.x;
            if (maxY > extent->hi.y) maxY = extent->hi.y;
            if (maxX <= _xOrigin) return;
            if (maxY <= _yOrigin) return;
            _width = maxX - _xOrigin;
            _height = maxY - _yOrigin;

            // Allocate cells.
            // pool[]ify
            _blockOfMapCells = new PathfindCell[m_width * m_height];
            _layerCells = new PathfindCell[_width][];
            int i;
            for (i = 0; i < _width; i++)
            {
                _layerCells[i] = _blockOfMapCells[i * _height];
            }
        }

        public void AllocateCellsForWallLayer(IRegion2D* extent, ObjectId *wallPieces, int numPieces);
        public void ClassifyCells();
        public void ClassifyWallCells(ObjectId* wallPieces, int numPieces);
        public bool SetDestroyed(bool destroyed);

        /// <summary>
        /// Returns true if the layer is avaialble for use (if it doesn't contain a bridge).
        /// </summary>
        public bool IsUnused
        {
            get
            {
                // Special case - wall layer is built from not a bridge
                if (_layer == PathfindLayerType.Wall && _width > 0)
                {
                    return false;
                }

                if (_bridge is null)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// True if it has been destroyed.
        /// </summary>
        public bool IsDestroyed => _destroyed;

        public PathfindCell GetCell(int x, int y);

        public void ApplyZone(void); // Propagates m_zone to all cells.
        public void GetStartCellIndex(ref Vector2 start)
        {
            start = _startCell;
        }

        public void getEndCellIndex(ref Vector2 end)
        {
            end = _endCell;
        }

        public ObjectId GetBridgeId();
        public bool ConnectsZones(PathfindZoneManager zm, LocomotorSet locomotorSet, int zone1, int zone2);
        public bool IsPointOnWall(ObjectId* wallPieces, int numPieces, Vector3 pt);

        protected void ClassifyLayerMapCell(int i, int j, PathfindCell cell, Bridge bridge);
        protected void ClassifyWallMapCell(int i, int j, PathfindCell cell, ObjectId* wallPieces, int numPieces);


#if DEBUG
        // TODO(Port)
        void DoDebugIcons();
#endif
    }
}

// this doesn't actually seem to be a particularly useful win, 
// performance-wise, so I didn't enable it. (srj)
#define NO_CPOP_STARTS_FROM_PREV_SEG

using System;
using System.Diagnostics;
using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Terrain;

#nullable enable

namespace OpenSage.Logic.AI;

public partial class AIPathfind
{
    /// <summary>
    /// This class encapsulates a "path" returned by the Pathfinder.
    /// </summary>
    class Path(AI ai) // TODO(Port) : Snapshot
    {
        /// <summary>
        /// Max times we will return the cached cpop.
        /// </summary>
        protected const int MaxCpop = 20;

        protected PathNode? _path;
        protected PathNode? _pathTail;

        // caching info for computePointOnPath.
        protected bool _cpopValid;
        protected int _cpopCountdown = MaxCpop;
        protected Vector3 _cpopIn;
        protected ClosestPointOnPathInfo _cpopOut = new ClosestPointOnPathInfo()
        {
            DistanceAlongPath = 0,
            Layer = PathfindLayerType.Ground,
            PositionOnPath = Vector3.Zero
        };

        protected PathNode? _cpopRecentStart;

        public PathNode? FirstNode => _path;
        public PathNode? LastNode => _pathTail;

        protected bool IsOptimized { get; set; }
        public bool BlockedByAlly { get; set; }

        public void MarkOptimized()
        {
            IsOptimized = true;
        }

        /// <summary>
        /// Given a location, return nearest location on path, and along-path dist to end as function result
        /// </summary>
        public void PeekCachedPointOnPath(out Vector3 position)
        {
            position = _cpopOut.PositionOnPath;
        }

        /// <summary>
        /// Create a new node at the head of the path
        /// </summary>
        public void PrependNode(Vector3 position, PathfindLayerType layer)
        {
            var node = new PathNode()
            {
                Position = position,
                Layer = layer
            };

            _path = node.PrependToList(_path);
            _pathTail ??= node;

            IsOptimized = false;

#if CPOP_STARTS_FROM_PREV_SEG
            _cpopRecentStart = null;
#endif
        }

        /// <summary>
        /// Create a new node at the tail of the path
        /// </summary>
        public void AppendNode(Vector3 position, PathfindLayerType layer)
        {
            if (IsOptimized && _pathTail is not null)
            {
                // Check for duplicates.
                if (position == _pathTail.Position)
                {
                    Debug.WriteLine("Warning - Path Seg length == 0, ignoring. john a.");
                    return;
                }
            }

            var node = new PathNode()
            {
                Position = position,
                Layer = layer
            };

            _path = node.AppendToList(_path);

            if (IsOptimized && _pathTail is not null)
            {
                _pathTail.NextOptimized = node;
            }

            _pathTail = node;


#if CPOP_STARTS_FROM_PREV_SEG
            _cpopRecentStart = null;
#endif
        }

        /// <summary>
        /// Create a new node at the tail of the path
        /// </summary>
        /// <param name="position"></param>
        public void UpdateLastNode(Vector3 position, TerrainLogic terrainLogic)
        {
            var layer = terrainLogic.GetLayerForDestination(position);

            if (_pathTail is not null)
            {
                _pathTail.Position = position;
                _pathTail.Layer = layer;
            }

            if (IsOptimized && _pathTail is not null)
            {
                var node = _path;
                while (node is not null && node.NextOptimized != _pathTail)
                {
                    node = node.NextOptimized;
                }

                if (node is not null && node.NextOptimized == _pathTail)
                {
                    node.NextOptimized = _pathTail;
                }
            }
        }

        /// <summary>
        /// Optimize the path by checking line of sight
        /// </summary>
        public void Optimize(GameObject obj, Surfaces acceptableSurfaces, bool blocked)
        {
            PathNode? node, anchor;

            // start with first node in the path
            anchor = FirstNode;

            var firstNode = true;
            var firstLayer = anchor.Layer;

            // backwards.

            // For each node in the path, check LOS from last node in path, working forward.
            // When a clear LOS is found, keep the resulting straight line segment.
            while (anchor != LastNode)
            {
                // find the farthest node in the path that has a clear line-of-sight to this anchor
                var optimizedSegment = false;
                var layer = anchor.Layer;
                var curLayer = anchor.Layer;
                var count = 0;
                const int allowedSteps = 3; // we can optimize 3 steps to or from a bridge.
                                            // Otherwise, we need to insert a point. jba.
                for (node = anchor.Next; node?.Next is not null; node = node.Next)
                {
                    count++;
                    if (curLayer == PathfindLayerType.Ground)
                    {
                        if (node.Layer != curLayer)
                        {
                            layer = node.Layer;
                            curLayer = layer;
                            if (count > allowedSteps)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (node.Next.Layer != curLayer)
                        {
                            if (count > allowedSteps)
                            {
                                break;
                            }
                        }
                    }

                    curLayer = node.Layer;
                    if (!node.CanOptimize)
                    {
                        break;
                    }
                }

                if (firstNode)
                {
                    layer = firstLayer;
                    firstNode = false;
                }

                //PathfindLayerEnum curLayer = LAYER_GROUND;
                for (; node != anchor; node = node?.Previous)
                {
                    var isPassable = false;
                    //CRCDEBUG_LOG(("Path::optimize() calling isLinePassable()\n"));

                    if (ai.Pathfinder.IsLinePassable(
                        obj,
                        acceptableSurfaces,
                        layer,
                        anchor.Position,
                        node.Position,
                        blocked,
                        false))
                    {
                        isPassable = true;
                    }

                    PathfindCell cell = ai.Pathfinder.GetCell(layer, node.Position);
                    if (cell is not null && cell.Type == PathfindCell.CellType.Cliff && !cell.Pinched)
                    {
                        isPassable = true;
                    }

                    // Horizontal, diagonal, and vertical steps are passable.
                    if (!isPassable)
                    {
                        var dx = (int)(node.Position.X - anchor.Position.X);
                        var dy = (int)(node.Position.Y - anchor.Position.Y);
                        var mightBePassable = false;
                        if (Math.Abs(dx) == PathfindCellSize && Math.Abs(dy) == PathfindCellSize)
                        {
                            isPassable = true;
                        }

                        PathNode? tmpNode;
                        if (dx == 0)
                        {
                            mightBePassable = true;
                            for (tmpNode = node.Previous; tmpNode is not null && tmpNode != anchor; tmpNode = tmpNode.Previous)
                            {
                                dx = (int)(tmpNode.Next.Position.X - tmpNode.Position.X);
                                if (dx != 0)
                                {
                                    mightBePassable = false;
                                }
                            }
                        }

                        if (dy == 0)
                        {
                            mightBePassable = true;
                            for (tmpNode = node.Previous; tmpNode is not null && tmpNode != anchor; tmpNode = tmpNode.Previous)
                            {
                                dy = (int)(tmpNode.Next.Position.Y - tmpNode.Position.Y);
                                if (dy != 0)
                                {
                                    mightBePassable = false;
                                }
                            }
                        }

                        if (dx == dy)
                        {
                            mightBePassable = true;
                            for (tmpNode = node.Previous; tmpNode is not null && tmpNode != anchor; tmpNode = tmpNode.Previous)
                            {
                                dx = (int)(tmpNode.Next.Position.X - tmpNode.Position.X);
                                dy = (int)(tmpNode.Next.Position.Y - tmpNode.Position.Y);
                                if (dy != dx)
                                {
                                    mightBePassable = false;
                                }
                            }
                        }

                        if (dx == -dy)
                        {
                            mightBePassable = true;
                            for (tmpNode = node.Previous; tmpNode is not null && tmpNode != anchor; tmpNode = tmpNode.Previous)
                            {
                                dx = (int)(tmpNode.Next.Position.X - tmpNode.Position.X);
                                dy = (int)(tmpNode.Next.Position.Y - tmpNode.Position.Y);
                                if (dy != -dx)
                                {
                                    mightBePassable = false;
                                }
                            }
                        }

                        if (mightBePassable)
                        {
                            isPassable = true;
                        }
                    }

                    if (isPassable)
                    {
                        // anchor can directly see this node, make it next in the optimized path
                        anchor.NextOptimized = node;
                        anchor = node;
                        optimizedSegment = true;
                        break;
                    }
                }

                if (optimizedSegment == false)
                {
                    // for some reason, there is no clear LOS between the anchor node and the very next node
                    anchor.NextOptimized = anchor.Next;
                    anchor = anchor.Next;
                }
            }

            // the path has been optimized
            IsOptimized = true;
        }

        /// <summary>
        /// Optimize the path by checking line of sight
        /// </summary>
        public void OptimizeGroundPath(AI ai, bool crusher, int pathDiameter)
        {
            PathNode? node, anchor;

            // start with first node in the path
            anchor = FirstNode;

            // For each node in the path, check LOS from last node in path, working forward.
            // When a clear LOS is found, keep the resulting straight line segment.
            while (anchor != LastNode)
            {
                // find the farthest node in the path that has a clear line-of-sight to this anchor
                var optimizedSegment = false;
                var layer = anchor.Layer;
                var curLayer = anchor.Layer;
                var count = 0;
                const int allowedSteps = 3; // we can optimize 3 steps to or from a bridge.
                                            // Otherwise, we need to insert a point. jba.
                for (node = anchor.Next; node?.Next is not null; node = node.Next)
                {
                    count++;
                    if (curLayer == PathfindLayerType.Ground)
                    {
                        if (node.Layer != curLayer)
                        {
                            layer = node.Layer;
                            curLayer = layer;
                            if (count > allowedSteps)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (node.Next.Layer != curLayer)
                        {
                            if (count > allowedSteps)
                            {
                                break;
                            }
                        }
                    }

                    curLayer = node.Layer;
                }

                // find the farthest node in the path that has a clear line-of-sight to this anchor
                for (; node != anchor; node = node?.Previous)
                {
                    var isPassable = false;
                    //CRCDEBUG_LOG(("Path::optimize() calling isLinePassable()\n"));
                    if (ai.Pathfinder.IsGroundPathPassable(
                        crusher,
                        anchor.Position,
                        layer,
                        node.Position,
                        pathDiameter))
                    {
                        isPassable = true;
                    }

                    // Horizontal, diagonal, and vertical steps are passable.
                    if (!isPassable)
                    {
                        var dx = (int)(node.Position.X - anchor.Position.X);
                        var dy = (int)(node.Position.Y - anchor.Position.Y);
                        var mightBePassable = false;

                        PathNode? tmpNode;
                        if (dx == 0)
                        {
                            mightBePassable = true;
                            for (tmpNode = node.Previous; tmpNode is not null && tmpNode != anchor; tmpNode = tmpNode.Previous)
                            {
                                dx = (int)(tmpNode.Next.Position.X - tmpNode.Position.X);
                                if (dx != 0)
                                {
                                    mightBePassable = false;
                                }
                            }
                        }

                        if (dy == 0)
                        {
                            mightBePassable = true;
                            for (tmpNode = node.Previous; tmpNode is not null && tmpNode != anchor; tmpNode = tmpNode.Previous)
                            {
                                dy = (int)(tmpNode.Next.Position.Y - tmpNode.Position.Y);
                                if (dy != 0)
                                {
                                    mightBePassable = false;
                                }
                            }
                        }

                        if (dx == dy)
                        {
                            mightBePassable = true;
                            for (tmpNode = node.Previous; tmpNode is not null && tmpNode != anchor; tmpNode = tmpNode.Previous)
                            {
                                dx = (int)(tmpNode.Next.Position.X - tmpNode.Position.X);
                                dy = (int)(tmpNode.Next.Position.Y - tmpNode.Position.Y);
                                if (dy != dx)
                                {
                                    mightBePassable = false;
                                }
                            }
                        }

                        if (dx == -dy)
                        {
                            mightBePassable = true;
                            for (tmpNode = node.Previous; tmpNode is not null && tmpNode != anchor; tmpNode = tmpNode.Previous)
                            {
                                dx = (int)(tmpNode.Next.Position.X - tmpNode.Position.X);
                                dy = (int)(tmpNode.Next.Position.Y - tmpNode.Position.Y);
                                if (dy != -dx)
                                {
                                    mightBePassable = false;
                                }
                            }
                        }

                        if (mightBePassable)
                        {
                            isPassable = true;
                        }
                    }

                    if (isPassable)
                    {
                        // anchor can directly see this node, make it next in the optimized path
                        anchor.NextOptimized = node;
                        anchor = node;
                        optimizedSegment = true;
                        break;
                    }
                }

                if (optimizedSegment == false)
                {
                    // for some reason, there is no clear LOS between the anchor node and the very next node
                    anchor.NextOptimized = anchor.Next;
                    anchor = anchor.Next;
                }
            }

            // Remove jig/jogs :) jba.
            for (anchor = FirstNode; anchor is not null; anchor = anchor.NextOptimized)
            {
                node = anchor.NextOptimized;
                if (node?.NextOptimized is not null)
                {
                    var dx = node.Position.X - anchor.Position.X;
                    var dy = node.Position.Y - anchor.Position.Y;

                    // If the x & y offsets are less than 2 pathfind cells, kill it.
                    if (dx * dx + dy * dy < MathUtility.Square(PathfindCellSizeF) * 3.9f)
                    {
                        anchor.NextOptimized = node.NextOptimized;
                    }
                }
            }

            // the path has been optimized
            IsOptimized = true;
        }

        /// <summary>
        /// Given a location, return the closest position on the path.
        /// If 'allowBacktrack' is true, the entire path is considered.
        /// If it is false, the point computed cannot be prior to previously returned non-backtracking points on this path.
        /// Because the path "knows" the direction of travel, it will "lead" the given position a bit
        /// to ensure the path is followed in the intended direction.
        /// 
        /// Note: The path cleanup does not take into account rolling terrain, so we can end up with
        /// these situations:
        /// 
        ///           B
        ///        ######
        ///      ##########
        /// A-##----------##---C
        /// #######################
        /// 
        /// 
        /// When an agent gets to B, he seems far off of the path, but is really not.
        /// There are similar problems with valleys.
        /// 
        /// Since agents track the closest path, if a high hill gets close to the underside of
        /// a bridge, an agent may 'jump' to the higher path. This must be avoided in maps.
        /// 
        /// return along-path distance to the end will be returned as function result
        /// </summary>
        public void ComputePointOnPath(GameObject obj, LocomotorSet locomotorSet, Vector3 position, ref ClosestPointOnPathInfo info)
        {
            // CRCDEBUG_LOG(("Path::computePointOnPath() fzor %s\n", DescribeObject(obj).str()));

            info.Layer = PathfindLayerType.Ground;
            info.PositionOnPath = Vector3.Zero;
            info.DistanceAlongPath = 0;

            if (_path is null)
            {
                _cpopValid = false;
                return;
            }

            info.Layer = _path.Layer;

            if (_cpopValid && _cpopCountdown > 0 && IsReallyClose(position, _cpopIn))
            {
                info = _cpopOut;
                _cpopCountdown--;
                //CRCDEBUG_LOG(("Path::computePointOnPath() end because we're really close\n"));
                return;
            }

            _cpopCountdown = MaxCpop;

            // default PositionOnPath to end of the path
            info.PositionOnPath = LastNode!.Position;

            PathNode? closeNode = null;
            Vector2 toPos;
            var closeDistSqr = 99999999.9f;
            var totalPathLength = 0.0f;
            var lengthAlongPathToPos = 0.0f;

            // Find the closest segment of the path
#if CPOP_STARTS_FROM_PREV_SEG
            PathNode? prevNode = _cpopRecentStart;
	        if (prevNode is null)
            {
		        prevNode = _path;
            }
#else
            var prevNode = _path;
#endif
            Vector2 segmentDirNorm;
            float segmentLength;

            // note that the seg dir and len returned by this is the dist & vec from 'prevNode' to 'node'
            for (var node = prevNode.GetNextOptimized(out segmentDirNorm, out segmentLength);
                 node is not null;
                 node = node.GetNextOptimized(out segmentDirNorm, out segmentLength))
            {
                var prevNodePos = prevNode.Position;
                var nodePos = node.Position;

                // compute vector from start of segment to pos
                toPos.X = position.X - prevNodePos.X;
                toPos.Y = position.Y - prevNodePos.Y;

                // compute distance projection of 'toPos' onto segment
                var alongPathDist = segmentDirNorm.X * toPos.X + segmentDirNorm.Y * toPos.Y;

                Vector3 pointOnPath;
                if (alongPathDist < 0.0f)
                {
                    // projected point is before start of segment, use starting point
                    alongPathDist = 0.0f;
                    pointOnPath = prevNodePos;
                }
                else if (alongPathDist > segmentLength)
                {
                    // projected point is beyond end of segment, use end point
                    if (node.NextOptimized is null)
                    {
                        alongPathDist = segmentLength;
                        pointOnPath = nodePos;
                    }
                    else
                    {
                        // beyond the end of this segment, skip this segment
                        // if bend is sharp, start of next segment will grab this point
                        // if bend is gradual, the point will project into the next segment
                        totalPathLength += segmentLength;
                        prevNode = node;
                        continue;
                    }
                }
                else
                {
                    // projected point is on this segment, compute it
                    pointOnPath.X = prevNodePos.X + alongPathDist * segmentDirNorm.X;
                    pointOnPath.Y = prevNodePos.Y + alongPathDist * segmentDirNorm.Y;
                    pointOnPath.Z = 0;
                }

                // compute distance to point on path, and track the closest we've found so far
                Vector2 offset;
                offset.X = position.X - pointOnPath.X;
                offset.Y = position.Y - pointOnPath.Y;

                var offsetDistSqr = offset.X * offset.X + offset.Y * offset.Y;
                if (offsetDistSqr < closeDistSqr)
                {
                    closeDistSqr = offsetDistSqr;
                    closeNode = prevNode;
                    info.PositionOnPath = pointOnPath;

                    lengthAlongPathToPos = totalPathLength + alongPathDist;
                }

                // add this segment's length to find total path length
                // TODO Precompute this and store in path
                totalPathLength += segmentLength;
                prevNode = node;
                //DUMPCOORD3D(&pointOnPath);
            }

#if CPOP_STARTS_FROM_PREV_SEG
	        _cpopRecentStart = closeNode;
#endif

            // Compute the goal movement position for this agent
            if (closeNode?.NextOptimized is not null)
            {
                // note that the seg dir and len returned by this is the dist & vec from 'closeNode' to 'closeNext'
                var closeNext = closeNode.GetNextOptimized(out segmentDirNorm, out segmentLength)!;
                var nextNodePos = closeNext.Position;
                var closeNodePos = closeNode.Position;

                var closePrev = closeNode.Previous;
                if (closePrev?.Layer > PathfindLayerType.Ground)
                {
                    // TODO (Port):
                    // This looks like it should be closePrev.Layer,
                    // but the original C++ code uses closeNode.Layer:
                    // https://github.com/electronicarts/CnC_Generals_Zero_Hour/blob/0a05454d8574207440a5fb15241b98ad0b435590/Generals/Code/GameEngine/Source/GameLogic/AI/AIPathfind.cpp#L877
                    info.Layer = closeNode.Layer;
                }

                if (closeNode.Layer > PathfindLayerType.Ground)
                {
                    info.Layer = closeNode.Layer;
                }

                if (closeNext.Layer > PathfindLayerType.Ground)
                {
                    info.Layer = closeNext.Layer;
                }

                // compute vector from start of segment to pos
                toPos.X = position.X - closeNodePos.X;
                toPos.Y = position.Y - closeNodePos.Y;

                // compute distance projection of 'toPos' onto segment
                var alongPathDist = segmentDirNorm.X * toPos.X + segmentDirNorm.Y * toPos.Y;

                // we know this is the closest segment, so don't allow farther back than the start node
                if (alongPathDist < 0.0f)
                {
                    alongPathDist = 0.0f;
                }

                // compute distance of point from this path segment
                var toDistSqr = MathUtility.Square(toPos.Y) + MathUtility.Square(toPos.Y);
                var offsetDistSq = toDistSqr - MathUtility.Square(alongPathDist);
                var offsetDist = offsetDistSq <= 0.0f ? 0.0f : MathF.Sqrt(offsetDistSq);

                // If we are basically on the path, return the next path node as the movement goal.
                // However, the farther off the path we get, the movement goal becomes closer to our
                // projected position on the path. If we are very far off the path, we will move 
                // directly towards the nearest point on the path, and not the next path node.
                const float maxPathError = 3.0f * PathfindCellSizeF;
                const float maxPathErrorInv = 1.0f / maxPathError;
                var k = offsetDist * maxPathErrorInv;
                if (k > 1.0f)
                {
                    k = 1.0f;
                }

                var gotPos = false;
                // CRCDEBUG_LOG(("Path::computePointOnPath() calling isLinePassable() 1\n"));
                if (ai.Pathfinder.IsLinePassable(obj, locomotorSet.Surfaces, info.Layer, position, /*ref*/ nextNodePos, false, true))
                {
                    info.PositionOnPath = nextNodePos;
                    gotPos = true;

                    var tryAhead = alongPathDist > segmentLength * 0.5;
                    if (!closeNext.CanOptimize)
                    {
                        tryAhead = false; // don't go past no-opt nodes.
                    }

                    if (closeNode.Layer != closeNext.Layer)
                    {
                        tryAhead = false; // don't go past layers.
                    }

                    if (obj.Layer != PathfindLayerType.Ground)
                    {
                        tryAhead = false;
                    }

                    var veryClose = false;
                    if (segmentLength - alongPathDist < 1.0f)
                    {
                        tryAhead = true;
                        veryClose = true;
                    }

                    if (tryAhead)
                    {
                        // try next segment middle.	
                        var next = closeNext.NextOptimized;
                        if (next is not null)
                        {
                            Vector3 tryPos;
                            tryPos.X = (nextNodePos.X + next.Position.X) * 0.5f;
                            tryPos.Y = (nextNodePos.Y + next.Position.Y) * 0.5f;
                            tryPos.Z = nextNodePos.Z;
                            //CRCDEBUG_LOG(("Path::computePointOnPath() calling isLinePassable() 2\n"));
                            if (veryClose || ai.Pathfinder.IsLinePassable(obj, locomotorSet.Surfaces, closeNext.Layer, position, /*ref*/ tryPos, false, true))
                            {
                                gotPos = true;
                                info.PositionOnPath = tryPos;
                            }
                        }
                    }
                }
                else if (k > 0.5f)
                {
                    var tryDist = alongPathDist + 0.5f * (segmentLength - alongPathDist);

                    // projected point is on this segment, compute it
                    info.PositionOnPath = new Vector3(
                        closeNodePos.X + tryDist * segmentDirNorm.X,
                        closeNodePos.Y + tryDist * segmentDirNorm.Y,
                        closeNodePos.Z);

                    //CRCDEBUG_LOG(("Path::computePointOnPath() calling isLinePassable() 3\n"));
                    var positionOnPath = info.PositionOnPath;
                    if (ai.Pathfinder.IsLinePassable(obj, locomotorSet.Surfaces, info.Layer, position, /*ref*/ positionOnPath, false, true))
                    {
                        info.PositionOnPath = positionOnPath;
                        k = 0.5f;
                        gotPos = true;
                    }
                }

                // if we are on the path (k == 0), then alongPathDist == segmentLength
                // if we are way off the path (k == 1), then alongPathDist is unchanged, and it projection of actual pos
                alongPathDist += (1.0f - k) * (segmentLength - alongPathDist);

                if (!gotPos)
                {
                    if (alongPathDist > segmentLength)
                    {
                        alongPathDist = segmentLength;
                        info.PositionOnPath = nextNodePos;
                    }
                    else
                    {
                        // projected point is on this segment, compute it
                        info.PositionOnPath = new Vector3(
                            closeNodePos.X + alongPathDist * segmentDirNorm.X,
                            closeNodePos.Y + alongPathDist * segmentDirNorm.Y,
                            closeNodePos.Z);

                        var dx = MathF.Abs(position.X - info.PositionOnPath.X);
                        var dy = MathF.Abs(position.Y - info.PositionOnPath.Y);
                        if (dx < 1 && dy < 1 && closeNode.NextOptimized?.NextOptimized is { } nextButOne)
                        {
                            info.PositionOnPath = nextButOne.Position;
                        }
                    }
                }
            }

            ai.Pathfinder.SetDebugPathPosition(info.PositionOnPath);

            info.DistanceAlongPath = totalPathLength - lengthAlongPathToPos;

            Vector3 delta;
            delta.X = info.PositionOnPath.X - position.X;
            delta.Y = info.PositionOnPath.Y - position.Y;
            delta.Z = 0;
            var lenDelta = delta.Length();
            if (lenDelta > info.DistanceAlongPath && info.DistanceAlongPath > PathfindCloseEnough) 
	        {
		        info.DistanceAlongPath = lenDelta;
            }

            _cpopIn = position;
            _cpopOut = info;
            _cpopValid = true;
            //CRCDEBUG_LOG(("Path::computePointOnPath() end\n"));
        }

        /// <summary>
        /// Given a position, computes the distance to the goal. Returns 0 if we are past the goal.
        /// Returns the goal position in goalPosition.This is intended for use with flying paths, that go
        /// directly to the goal and don't consider obstacles. jba.
        public float ComputeFlightDistanceToGoal(Vector3 position, ref Vector3 goalPosition)
        {
            if (_path is null)
            {

                goalPosition.X = 0.0f;
                goalPosition.Y = 0.0f;
                goalPosition.Z = 0.0f;

                return 0.0f;
            }

            var curNode = FirstNode!;
            if (_cpopRecentStart is not null)
            {
                curNode = _cpopRecentStart;
            }
            else
            {
                _cpopRecentStart = curNode;
            }

            var nextNode = curNode.NextOptimized;
            goalPosition = curNode.Position;
            float distance = 0;
            var useNext = true;
            while (nextNode is not null)
            {
                if (useNext)
                {
                    goalPosition = nextNode.Position;
                }

                var startPos = curNode.Position;
                var endPos = nextNode.Position;

                Vector2 posToGoalVector;
                // posToGoalVector is pos to goalPos vector.
                posToGoalVector.X = endPos.X - position.X;
                posToGoalVector.Y = endPos.Y - position.Y;

                // pathVector is the startPos to goal pos vector.
                Vector2 pathVector;
                pathVector.X = endPos.X - startPos.X;
                pathVector.Y = endPos.Y - startPos.Y;

                // Normalize pathVector
                pathVector = Vector2.Normalize(pathVector);

                // Dot product is the posToGoal vector projected onto the path vector.
                var dotProduct = posToGoalVector.X * pathVector.X + posToGoalVector.Y * pathVector.Y;
                if (dotProduct >= 0f)
                {
                    distance += dotProduct;
                    useNext = false;
                }
                else if (useNext)
                {
                    _cpopRecentStart = nextNode;
                }

                curNode = nextNode;
                nextNode = curNode.NextOptimized;
            }

            return distance;
        }
    }
}

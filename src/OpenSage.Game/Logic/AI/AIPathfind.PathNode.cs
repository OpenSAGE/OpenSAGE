using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI;

public partial class AIPathfind
{
    class PathNode
    {
        /// <summary>
        ///  Used in Path::xfer() to save & recreate the path list.
        /// </summary>
        public int _id = -1;

        private PathNode? _nextOptimized;

        /// <summary>
        /// if <see cref="NextOptimized"/> is nonnull, the dist to it.
        /// </summary>
        private float _nextOptimizedDistance2D;

        /// <summary>
        /// if <see cref="NextOptimized"/> is nonnull, normalized dir vec towards it.
        /// </summary>
        private Vector2 _nextOptimizedDirectionNormalized2D;

        /// <summary>
        /// position of node in space
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// next node in the path
        /// </summary>
        public PathNode? Next { get; set; }

        /// <summary>
        ///  previous node in the path
        /// </summary>
        public PathNode? Previous { get; set; }

        /// <summary>
        /// next node in the optimized path
        /// </summary>
        public PathNode? NextOptimized
        {
            get => _nextOptimized;
            set
            {
                _nextOptimized = value;
                if (value is not null)
                {
                    _nextOptimizedDirectionNormalized2D = new Vector2(value.Position.X - Position.X, value.Position.Y - Position.Y);
                    _nextOptimizedDistance2D = _nextOptimizedDirectionNormalized2D.Length();
                    if (_nextOptimizedDistance2D == 0.0f)
                    {
                        _nextOptimizedDistance2D = 0.01f;
                    }

                    _nextOptimizedDirectionNormalized2D /= _nextOptimizedDistance2D;
                }
                else
                {
                    _nextOptimizedDistance2D = 0.0f;
                }
            }
        }

        public PathNode? GetNextOptimized(out Vector2 direction, out float distance)
        {
            direction = _nextOptimizedDirectionNormalized2D;
            distance = _nextOptimizedDistance2D;

            return _nextOptimized;
        }

        /// <summary>
        /// Layer for this section.
        /// </summary>
        public PathfindLayerType Layer { get; set; }

        /// <summary>
        /// True if this cell can be optimized out.
        /// </summary>
        public bool CanOptimize { get; set; }


        /// <summary>
        ///  given a list, prepend this node, return new list
        /// </summary>
        public PathNode PrependToList(PathNode? list)
        {
            Next = list;
            if (list is not null)
            {
                list.Previous = this;
            }

            Previous = null;
            return this;
        }

        /// <summary>
        /// given a list, append this node, return new list. slow implementation.
        /// TODO optimize this
        /// </summary>
        public PathNode AppendToList(PathNode? list)
        {
            if (list is null)
            {
                Next = null;
                Previous = null;
                return this;
            }

            PathNode tail;
            for (tail = list; tail.Next is not null; tail = tail.Next)
                ;

            tail.Next = this;
            Previous = tail;
            Next = null;

            return list;
        }

        /// <summary>
        /// given a node, append new node to this.
        /// </summary>
        public void Append(PathNode newNode)
        {
            newNode.Next = Next;
            newNode.Previous = this;
            if (newNode.Next is not null)
            {
                newNode.Next.Previous = newNode;
            }

            Next = newNode;
        }

        /// <summary>
        /// Compute direction vector to next node
        /// </summary>
        public Vector3 ComputeDirectionVector()
        {
            if (Next is null)
            {
                if (Previous is null)
                {
                    // only one node on whole path - no direction
                    return Vector3.Zero;
                }
                else
                {
                    // tail node - continue prior direction
                    return Previous.ComputeDirectionVector();
                }
            }
            else
            {
                return Next.Position - Position;
            }
        }
    }
}

using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI;

public partial class AIPathfind
{
    struct ClosestPointOnPathInfo
    {
        public float DistanceAlongPath { get; set; }
        public Vector3 PositionOnPath { get; set; }
        public PathfindLayerType Layer { get; set; }
    };
}

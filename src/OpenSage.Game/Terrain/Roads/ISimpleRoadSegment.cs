namespace OpenSage.Terrain.Roads
{
    internal interface ISimpleRoadSegment : IRoadSegment
    {
        RoadSegmentEndPoint Start { get; }
        RoadSegmentEndPoint End { get; }
    }
}

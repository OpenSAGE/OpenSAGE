namespace OpenSage.Data.W3d
{
    public abstract class W3dMotionChannelData
    {
        public ushort[] TimeCodes { get; protected set; }
        public W3dAnimationChannelDatum[] Values { get; protected set; }
    }
}

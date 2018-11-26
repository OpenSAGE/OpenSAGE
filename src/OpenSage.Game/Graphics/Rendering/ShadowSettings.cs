namespace OpenSage.Graphics.Rendering
{
    public sealed class ShadowSettings
    {
        public FixedFilterSize FixedFilterSize = FixedFilterSize.Filter7x7;

        public bool VisualizeCascades = false;
        public bool FilterAcrossCascades = true;

        public float SplitDistance0 = 0.05f;
        public float SplitDistance1 = 0.15f;
        public float SplitDistance2 = 0.50f;
        public float SplitDistance3 = 1.00f;

        public float Bias = 0.002f;
        public float OffsetScale = 0.0f;
    }

    public enum FixedFilterSize
    {
        Filter2x2 = 2,
        Filter3x3 = 3,
        Filter5x5 = 5,
        Filter7x7 = 7
    }
}

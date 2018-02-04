using Veldrid;

namespace OpenSage.Graphics
{
    internal static class RasterizerStateDescriptionUtility
    {
        public static RasterizerStateDescription CullNoneSolid;
        public static RasterizerStateDescription DefaultFrontIsCounterClockwise;

        static RasterizerStateDescriptionUtility()
        {
            CullNoneSolid = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                true,
                false);

            DefaultFrontIsCounterClockwise = new RasterizerStateDescription(
                FaceCullMode.Back,
                PolygonFillMode.Solid,
                FrontFace.CounterClockwise,
                true,
                false);
        }
    }
}

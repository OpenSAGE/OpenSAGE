using Veldrid;

namespace OpenSage.Graphics
{
    // TODO: See if these can go into Veldrid.

    internal static class BlendStateDescriptionUtility
    {
        public static BlendStateDescription SingleBlendOpaque;
        public static BlendStateDescription SingleAdditive;

        static BlendStateDescriptionUtility()
        {
            SingleBlendOpaque = new BlendStateDescription(
                RgbaFloat.White,
                new BlendAttachmentDescription { BlendEnabled = false });

            SingleAdditive = new BlendStateDescription(
                RgbaFloat.White,
                BlendAttachmentDescription.AdditiveBlend);
        }
    }

    internal static class RasterizerStateDescriptionUtility
    {
        public static RasterizerStateDescription CullNoneSolid;

        static RasterizerStateDescriptionUtility()
        {
            CullNoneSolid = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                true,
                false);
        }
    }

    internal static class DepthStencilStateDescriptionUtility
    {
        public static DepthStencilStateDescription DepthRead;

        static DepthStencilStateDescriptionUtility()
        {
            DepthRead = new DepthStencilStateDescription(true, false, ComparisonKind.LessEqual);
        }
    }
}

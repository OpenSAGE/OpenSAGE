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
                FrontFace.CounterClockwise,
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

    internal static class BlendStateDescriptionUtility
    {
        public static readonly BlendStateDescription SingleAdditiveBlendNoAlpha = new BlendStateDescription
        {
            AttachmentStates = new[]
            {
                new BlendAttachmentDescription
                {
                    BlendEnabled = true,
                    SourceColorFactor = BlendFactor.One,
                    DestinationColorFactor = BlendFactor.One,
                    ColorFunction = BlendFunction.Add,
                    SourceAlphaFactor = BlendFactor.One,
                    DestinationAlphaFactor = BlendFactor.One,
                    AlphaFunction = BlendFunction.Add,
                }
            }
        };
    }
}

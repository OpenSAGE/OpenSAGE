using System;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Rendering.Shadows
{
    public sealed class ShadowSettings
    {
        private uint _shadowMapSize = 1024;

        /// <summary>
        /// Gets or sets the number of shadow map cascades.
        /// </summary>
        public ShadowMapCascades ShadowMapCascades { get; set; } = ShadowMapCascades.OneCascade;

        /// <summary>
        /// Gets or sets the type of shadows that are rendered.
        /// </summary>
        public ShadowsType ShadowsType { get; set; } = ShadowsType.Soft;

        /// <summary>
        /// Gets or sets the size of the shadow map (in pixels).
        /// </summary>
        public uint ShadowMapSize
        {
            get { return _shadowMapSize; }
            set
            {
                if (value <= 0u)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Shadow map size must be greater than 0.");
                }
                _shadowMapSize = MathUtility.NextPowerOfTwo(value);
            }
        }

        /// <summary>
        /// Gets or sets the split depth used when <see cref="ShadowMapCascades"/> is <see cref="ShadowMapCascades.TwoCascades"/>.
        /// </summary>
        public float ShadowCascades2SplitDepth { get; set; } = 0.333f;

        /// <summary>
        /// Gets or sets the split depths used when <see cref="ShadowMapCascades"/> is <see cref="ShadowMapCascades.FourCascades"/>.
        /// </summary>
        public Vector3 ShadowCascades4SplitDepths { get; set; } = new Vector3(0.06f, 0.20f, 0.50f);

        /// <summary>
        /// Gets or sets the furthest distance from the camera that shadows will be rendered.
        /// </summary>
        public float ShadowDistance { get; set; } = 1200.0f;

        /// <summary>
        /// Gets or sets whether to stabilize the shadow map cascades.
        /// If set to false, shadows will use more of the available shadow map resolution,
        /// but will appear to wobble as the camera moves.
        /// </summary>
        public bool StabilizeShadowCascades { get; set; } = true;

        public bool VisualizeCascades { get; set; } = false;

        public bool UpdateShadowMatrices { get; set; } = true;

        public bool VisualizeShadowFrustums { get; set; } = false;

        /// <summary>
        /// Gets or sets the depth bias to use when rendering shadows.
        /// </summary>
        public float Bias { get; set; } = 0.002f;

        /// <summary>
        /// Gets or sets the normal offset to use when rendering shadows.
        /// </summary>
        /// <remarks>
        /// For more information about the normal offset technique, visit this page:
        /// <see href="http://www.dissidentlogic.com/old/#Normal Offset Shadows"/>
        /// </remarks>
        public float NormalOffset { get; set; } = 0.4f;
    }

    /// <summary>
    /// Defines how many shadow cascades to render in the shadow map.
    /// </summary>
    public enum ShadowMapCascades
    {
        /// <summary>
        /// Render shadow maps using one cascade. This is the lowest quality and fastest option.
        /// </summary>
        OneCascade = 1,

        /// <summary>
        /// Render shadow maps using two cascades.
        /// </summary>
        TwoCascades = 2,

        /// <summary>
        /// Render shadow maps using four cascades. This is the best quality and slowest option.
        /// </summary>
        FourCascades = 4
    }

    /// <summary>
    /// Specifies the type of shadows cast by lights.
    /// </summary>
    public enum ShadowsType
    {
        /// <summary>
        /// No shadows.
        /// </summary>
        None = 0,

        /// <summary>
        /// Hard-edged shadows.
        /// </summary>
        Hard = 1,

        /// <summary>
        /// Soft-edged shadows.
        /// </summary>
        Soft = 2
    }
}

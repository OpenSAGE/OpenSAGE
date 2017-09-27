using System;
using System.Collections.Generic;
using OpenSage.Graphics.Cameras;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Lights
{
    /// <summary>
    /// Base class for light components.
    /// </summary>
    public abstract class LightComponent : EntityComponent
    {
        private int _shadowMapSize;

        /// <summary>
        /// Gets or sets the color of this light.
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets the intensity of this light.
        /// </summary>
        public float Intensity { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the type of shadows that are rendered for this light.
        /// </summary>
        public ShadowsType Shadows { get; set; } = ShadowsType.None;

        /// <summary>
        /// Gets or sets the size of the shadow map (in pixels).
        /// </summary>
        public int ShadowMapSize
        {
            get { return _shadowMapSize; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Shadow map size must be greater than 0.");
                _shadowMapSize = MathUtility.NextPowerOfTwo(value);
            }
        }

        /// <summary>
        /// Gets or sets the depth bias to use when rendering shadows for this light.
        /// </summary>
        public float ShadowBias { get; set; } = 0.002f;

        /// <summary>
        /// Gets or sets the normal offset to use when rendering shadows for this light.
        /// </summary>
        /// <remarks>
        /// For more information about the normal offset technique, visit this page:
        /// <see href="http://www.dissidentlogic.com/old/#Normal Offset Shadows"/>
        /// </remarks>
        public float ShadowNormalOffset { get; set; } = 0.4f;

        /// <summary>
        /// Gets or sets the furthest distance from the camera that shadows will be rendered for this light.
        /// </summary>
        public float ShadowDistance { get; set; } = 100.0f;

        /// <summary>
        /// Gets or sets the culling mask for this light.
        /// </summary>
        /// <remarks>
        /// The culling mask is a bit mask that is compared against <see cref="Entity.Layer"/>
        /// to determine whether a given <see cref="Entity"/> should be affected by this light.
        /// </remarks>
        public int CullingMask { get; set; } = int.MaxValue;

        private Dictionary<CameraComponent, IDisposable> ShadowData { get; } = new Dictionary<CameraComponent, IDisposable>();

        internal Color GetColor()
        {
            return Color * Intensity;
        }

        internal abstract Rectangle? GetScissorRectangle(CameraComponent camera);
        //internal abstract string EffectKeyword { get; }

        internal abstract bool IsVisibleInCameraFrustum(BoundingFrustum frustum);

        //internal virtual void SetParameters(RenderEffectPassCollection effectPasses, CameraComponent camera)
        //{
        //    effectPasses.SetParameter("ShadowBias", ShadowBias);
        //    effectPasses.SetParameter("ShadowOffsetScale", ShadowNormalOffset);
        //    effectPasses.SetParameter("ShadowDistance", ShadowDistance);
        //}

        internal TShadowData GetShadowData<TShadowData>(CameraComponent camera)
            where TShadowData : IDisposable
        {
            IDisposable result;
            ShadowData.TryGetValue(camera, out result);
            return (TShadowData) result;
        }

        internal void SetShadowData<TShadowData>(CameraComponent camera, TShadowData shadowData)
            where TShadowData : IDisposable
        {
            ShadowData[camera] = shadowData;
        }

        internal void RemoveShadowData(CameraComponent camera)
        {
            ShadowData.Remove(camera);
        }

        /// <inheritdoc />
        protected override void Destroy()
        {
            foreach (var shadowData in ShadowData.Values)
                shadowData.Dispose();
            ShadowData.Clear();
        }
    }
}

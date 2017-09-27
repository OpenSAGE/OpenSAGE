using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Graphics.Cameras;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Lights
{
    /// <summary>
    /// A directional light component. Directional lights emit light in a specific direction, without attenuation.
    /// </summary>
    public sealed class DirectionalLightComponent : LightComponent
    {
        /// <summary>
        /// Creates a new <see cref="DirectionalLightComponent"/>.
        /// </summary>
        public DirectionalLightComponent()
        {
            ShadowMapSize = 2048;
            ShadowDistance = 150.0f;
            ShadowCascades2SplitDepth = 0.333f;
            ShadowCascades4SplitDepths = new Vector3(0.06f, 0.20f, 0.50f);
        }

        internal Vector3 Direction => Entity.Transform.Forward;

        /// <summary>
        /// Gets or sets the number of shadow map cascades for this light.
        /// </summary>
        public ShadowMapCascades ShadowMapCascades { get; set; } = ShadowMapCascades.FourCascades;

        /// <summary>
        /// Gets or sets whether to stabilize the shadow map cascades.
        /// If set to false, shadows will use more of the available shadow map resolution,
        /// but will appear to wobble as the camera moves.
        /// </summary>
        public bool StabilizeShadowCascades { get; set; } = true;

        /// <summary>
        /// Gets or sets the split depth used when <see cref="ShadowMapCascades"/> is <see cref="Lights.ShadowMapCascades.TwoCascades"/>.
        /// </summary>
        public float ShadowCascades2SplitDepth { get; set; }

        /// <summary>
        /// Gets or sets the split depths used when <see cref="ShadowMapCascades"/> is <see cref="Lights.ShadowMapCascades.FourCascades"/>.
        /// </summary>
        public Vector3 ShadowCascades4SplitDepths { get; set; }

        internal override Rectangle? GetScissorRectangle(CameraComponent camera)
        {
            return null;
        }

        //internal override string EffectKeyword => RenderEffectTags.DirectionalLightTag;

        //internal override void SetParameters(RenderEffectPassCollection effectPasses, CameraComponent camera)
        //{
        //    base.SetParameters(effectPasses, camera);

        //    effectPasses.SetParameter("DirectionalLightDirection", Direction);
        //    effectPasses.SetParameter("DirectionalLightColor", GetColor().ToVector3());

        //    var shadowData = GetShadowData<DirectionalLightShadowData>(camera);
        //    if (shadowData != null)
        //    {
        //        effectPasses.SetParameter("ShadowMatrix", shadowData.ShadowMatrix);

        //        var cascadeSplits = new Vector4();
        //        cascadeSplits.X = shadowData.CascadeSplits[0];
        //        cascadeSplits.Y = shadowData.CascadeSplits[1];
        //        cascadeSplits.Z = shadowData.CascadeSplits[2];
        //        cascadeSplits.W = shadowData.CascadeSplits[3];
        //        effectPasses.SetParameter("CascadeSplits", cascadeSplits);

        //        effectPasses.SetParameter("CascadeOffsets", shadowData.CascadeOffsets);
        //        effectPasses.SetParameter("CascadeScales", shadowData.CascadeScales);

        //        effectPasses.SetParameter("ShadowMap", shadowData.ShadowMap);

        //        GraphicsDevice.SamplerStates[3] = SamplerStateUtility.ShadowMap;
        //    }
        //    else
        //    {
        //        effectPasses.SetParameter("ShadowMap", (Texture) null);
        //    }
        //}

        internal override bool IsVisibleInCameraFrustum(BoundingFrustum frustum)
        {
            return true;
        }

        //// Instead of another shader permutation for "no light", we just use the
        //// directional light permutation, with colour set to 0.
        //// This shouldn't often be used.
        //internal static void SetParametersNoLight(RenderEffectPassCollection effectPasses)
        //{
        //    effectPasses.SetParameter("DirectionalLightColor", Vector3.Zero);
        //    effectPasses.SetParameter("DirectionalLightDirection", Vector3.Down);
        //    effectPasses.SetParameter("ShadowMap", (Texture) null);
        //}
    }
}

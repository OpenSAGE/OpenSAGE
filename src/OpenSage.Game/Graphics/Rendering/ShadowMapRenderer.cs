using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class ShadowMapRenderer
    {
        private readonly float[] _cascadeSplits;
        private readonly Vector3[] _frustumCorners;

        /// <summary>
        /// Makes the "global" shadow matrix used as the reference point for the cascades.
        /// </summary>
        private Matrix4x4 MakeGlobalShadowMatrix(in Matrix4x4 cameraViewProjection, in Vector3 lightDirection)
        {
            // Get the 8 points of the view frustum in world space
            ResetViewFrustumCorners();

            var invViewProj = Matrix4x4Utility.Invert(cameraViewProjection);
            var frustumCenter = Vector3.Zero;
            for (var i = 0; i < 8; i++)
            {
                _frustumCorners[i] = Vector4.Transform(_frustumCorners[i], invViewProj).ToVector3();
                frustumCenter += _frustumCorners[i];
            }

            frustumCenter /= 8.0f;

            // Get position of the shadow camera
            var shadowCameraPos = frustumCenter + lightDirection * -0.5f;

            // Come up with a new orthographic camera for the shadow caster
            var shadowCamera = new OrthographicCamera(-0.5f, -0.5f, 0.5f, 0.5f, 0.0f, 1.0f);
            shadowCamera.SetLookAt(shadowCameraPos, frustumCenter, Vector3.UnitZ);

            var texScaleBias = Matrix4x4.CreateScale(0.5f, -0.5f, 1.0f);
            texScaleBias.Translation = new Vector3(0.5f, 0.5f, 0.0f);
            return shadowCamera.ViewProjection * texScaleBias;
        }

        private void ResetViewFrustumCorners()
        {
            _frustumCorners[0] = new Vector3(-1.0f, 1.0f, 0.0f);
            _frustumCorners[1] = new Vector3(1.0f, 1.0f, 0.0f);
            _frustumCorners[2] = new Vector3(1.0f, -1.0f, 0.0f);
            _frustumCorners[3] = new Vector3(-1.0f, -1.0f, 0.0f);
            _frustumCorners[4] = new Vector3(-1.0f, 1.0f, 1.0f);
            _frustumCorners[5] = new Vector3(1.0f, 1.0f, 1.0f);
            _frustumCorners[6] = new Vector3(1.0f, -1.0f, 1.0f);
            _frustumCorners[7] = new Vector3(-1.0f, -1.0f, 1.0f);
        }
    }
}

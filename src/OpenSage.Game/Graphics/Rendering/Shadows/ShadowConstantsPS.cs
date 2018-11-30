using System.Numerics;

namespace OpenSage.Graphics.Rendering.Shadows
{
    internal struct ShadowConstantsPS
    {
        public Matrix4x4 ShadowMatrix;
        public float CascadeSplit0;
        public float CascadeSplit1;
        public float CascadeSplit2;
        public float CascadeSplit3;
        public Vector4 CascadeOffset0;
        public Vector4 CascadeOffset1;
        public Vector4 CascadeOffset2;
        public Vector4 CascadeOffset3;
        public Vector4 CascadeScale0;
        public Vector4 CascadeScale1;
        public Vector4 CascadeScale2;
        public Vector4 CascadeScale3;
        public float Bias;
        public float OffsetScale;
        public uint VisualizeCascades;
        public uint FilterAcrossCascades;
        public float ShadowDistance;
        public ShadowsType ShadowsType;
        public uint NumSplits;
#pragma warning disable IDE1006
        private readonly float _Padding;
#pragma warning restore IDE1006

        public void Set(uint numCascades, ShadowSettings settings, ShadowData data)
        {
            ShadowMatrix = data.ShadowMatrix;

            SetArrayData(
                numCascades,
                data.CascadeSplits,
                out CascadeSplit0,
                out CascadeSplit1,
                out CascadeSplit2,
                out CascadeSplit3);

            SetArrayData(
                numCascades,
                data.CascadeOffsets,
                out CascadeOffset0,
                out CascadeOffset1,
                out CascadeOffset2,
                out CascadeOffset3);

            SetArrayData(
                numCascades,
                data.CascadeScales,
                out CascadeScale0,
                out CascadeScale1,
                out CascadeScale2,
                out CascadeScale3);

            Bias = settings.Bias;
            OffsetScale = settings.NormalOffset;
            VisualizeCascades = settings.VisualizeCascades ? 1u : 0u;
            FilterAcrossCascades = 1u;
            ShadowDistance = settings.ShadowDistance;
            ShadowsType = settings.ShadowsType;
            NumSplits = numCascades;
        }

        private static void SetArrayData<T>(
            uint numValues,
            in T[] values,
            out T value1,
            out T value2,
            out T value3,
            out T value4)
            where T : struct
        {
            value1 = values[0];
            value2 = (numValues > 1) ? values[1] : default;
            value3 = (numValues > 2) ? values[2] : default;
            value4 = (numValues > 3) ? values[3] : default;
        }
    }
}

using System;
using System.Numerics;
using LLGfx;
using OpenSage.Data.W3d;

namespace OpenSage.Graphics.Util
{
    public static class ConversionExtensions
    {
        public static Vector2 ToVector2(this W3dTexCoord value)
        {
            return new Vector2(value.U, value.V);
        }

        public static Vector3 ToVector3(this W3dVector value)
        {
            // Switch y and z to account for z being up in .w3d (thanks Stephan)
            return new Vector3(value.X, value.Z, -value.Y);
        }

        public static Vector3 ToVector3(this W3dRgb value)
        {
            return new Vector3(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f);
        }

        public static Quaternion ToQuaternion(this W3dQuaternion value)
        {
            // Switch y and z to account for z being up in .w3d (thanks Stephan)
            return new Quaternion(value.X, value.Z, -value.Y, value.W);
        }

        public static Blend ToBlend(this W3dShaderSrcBlendFunc value)
        {
            switch (value)
            {
                case W3dShaderSrcBlendFunc.Zero:
                    return Blend.Zero;

                case W3dShaderSrcBlendFunc.One:
                    return Blend.One;

                case W3dShaderSrcBlendFunc.SrcAlpha:
                    return Blend.SrcAlpha;

                case W3dShaderSrcBlendFunc.OneMinusSrcAlpha:
                    return Blend.OneMinusSrcAlpha;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Blend ToBlend(this W3dShaderDestBlendFunc value)
        {
            switch (value)
            {
                case W3dShaderDestBlendFunc.Zero:
                    return Blend.Zero;

                case W3dShaderDestBlendFunc.One:
                    return Blend.One;

                case W3dShaderDestBlendFunc.SrcAlpha:
                    return Blend.SrcAlpha;

                case W3dShaderDestBlendFunc.OneMinusSrcAlpha:
                    return Blend.OneMinusSrcAlpha;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

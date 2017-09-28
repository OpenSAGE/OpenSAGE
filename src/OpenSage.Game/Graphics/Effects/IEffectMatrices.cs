using System.Numerics;

namespace OpenSage.Graphics.Effects
{
    public interface IEffectMatrices
    {
        void SetWorld(Matrix4x4 matrix);
        void SetView(Matrix4x4 matrix);
        void SetProjection(Matrix4x4 matrix);
    }
}

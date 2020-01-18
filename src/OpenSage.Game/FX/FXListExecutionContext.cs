using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.FX
{
    internal sealed class FXListExecutionContext
    {
        public readonly GameObject GameObject;
        public readonly Matrix4x4 WorldMatrix;
        public readonly GameContext GameContext;

        public FXListExecutionContext(
            GameObject gameObject,
            in Matrix4x4 worldMatrix,
            GameContext gameContext)
        {
            GameObject = gameObject;
            WorldMatrix = worldMatrix;
            GameContext = gameContext;
        }
    }
}

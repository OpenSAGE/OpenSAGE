using System.Numerics;

namespace OpenSage.FX
{
    internal sealed class FXListExecutionContext
    {
        public readonly Quaternion Rotation;
        public readonly Vector3 Position;
        public readonly GameContext GameContext;

        public FXListExecutionContext(
            in Quaternion rotation,
            in Vector3 position,
            GameContext gameContext)
        {
            Rotation = rotation;
            Position = position;
            GameContext = gameContext;
        }
    }
}

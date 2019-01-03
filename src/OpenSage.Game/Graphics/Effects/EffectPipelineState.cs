using System;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public readonly struct EffectPipelineState : IEquatable<EffectPipelineState>
    {
        public readonly RasterizerStateDescription RasterizerState;

        public readonly DepthStencilStateDescription DepthStencilState;

        public readonly BlendStateDescription BlendState;

        public readonly OutputDescription OutputDescription;

        public readonly EffectPipelineStateHandle Handle;

        public EffectPipelineState(
            in RasterizerStateDescription rasterizerState,
            in DepthStencilStateDescription depthStencilState,
            in BlendStateDescription blendState,
            in OutputDescription outputDescription)
        {
            RasterizerState = rasterizerState;
            DepthStencilState = depthStencilState;
            BlendState = blendState;

            OutputDescription = outputDescription;

            Handle = null;
            Handle = EffectPipelineStateFactory.GetHandle(this);
        }

        public override bool Equals(object obj)
        {
            return obj is EffectPipelineState && Equals((EffectPipelineState) obj);
        }

        public bool Equals(EffectPipelineState other)
        {
            return RasterizerState.Equals(other.RasterizerState) &&
                   DepthStencilState.Equals(other.DepthStencilState) &&
                   BlendState.Equals(other.BlendState) &&
                   OutputDescription.Equals(other.OutputDescription);
        }

        public override int GetHashCode()
        {
            // Not using HashCode.Combine here, to avoid copying large structs.
            var hashCode = 414621651;
            hashCode = hashCode * -1521134295 + RasterizerState.GetHashCode();
            hashCode = hashCode * -1521134295 + DepthStencilState.GetHashCode();
            hashCode = hashCode * -1521134295 + BlendState.GetHashCode();
            hashCode = hashCode * -1521134295 + OutputDescription.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(in EffectPipelineState state1, in EffectPipelineState state2)
        {
            return state1.Equals(state2);
        }

        public static bool operator !=(in EffectPipelineState state1, in EffectPipelineState state2)
        {
            return !(state1 == state2);
        }
    }
}

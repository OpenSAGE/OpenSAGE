using System;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Rendering.Water
{
    public sealed class WaterSettings
    {
        private uint _reflectionMapSize = 256u;
        private uint _refractionMapSize = 256u;

        /// <summary>
        /// Gets or sets the size of the reflection map (in pixels).
        /// </summary>
        public uint ReflectionMapSize
        {
            get { return _reflectionMapSize; }
            set
            {
                if (value <= 0u)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Reflection map size must be greater than 0.");
                }
                _reflectionMapSize = MathUtility.NextPowerOfTwo(value);
            }
        }
        /// <summary>
        /// Gets or sets the size of the refraction map (in pixels).
        /// </summary>
        public uint RefractionMapSize
        {
            get { return _refractionMapSize; }
            set
            {
                if (value <= 0u)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Refraction map size must be greater than 0.");
                }
                _refractionMapSize = MathUtility.NextPowerOfTwo(value);
            }
        }
        /// <summary>
        /// Gets or sets the furthest distance from the camera that reflections will be rendered.
        /// </summary>
        public float ReflectionRenderDistance { get; set; } = 600.0f;
        /// <summary>
        /// Gets or sets the furthest distance from the camera that refractions will be rendered.
        /// </summary>
        public float RefractionRenderDistance { get; set; } = 2000.0f;

        /// <summary>
        /// Gets or sets the clipping plane height offset.
        /// </summary>
        public float ClippingOffset { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets whether we use reflections.
        /// </summary>
        public bool IsRenderReflection { get; set; } = true;

        /// <summary>
        /// Gets or sets whether we use refractions.
        /// </summary>
        public bool IsRenderRefraction { get; set; } = true;

        /// <summary>
        /// Gets or sets whether we use soft edges.
        /// </summary>
        public bool IsRenderSoftEdge { get; set; } = true;

        /// <summary>
        /// Gets or sets whether we use caustics.
        /// </summary>
        public bool IsRenderCaustics { get; set; } = true;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Input
{
    /// <summary>
    /// Configures an input axis. An input axis contains a set of axis mappings that abstract
    /// keyboard, mouse, and gamepad input into a convenient higher-level API. Axes are for
    /// continuous input, such as mouse movement in an FPS. Input for discrete events should
    /// be configured using <see cref="InputAction"/>.
    /// </summary>
    public sealed class InputAxis
    {
        /// <summary>
        /// Gets or sets the axis name. This is the name that should be passed to <see cref="IInputSystem.GetAxis(string)"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the mappings for this axis.
        /// </summary>
        public List<InputAxisMapping> Mappings { get; set; } = new List<InputAxisMapping>();

        internal float GetValue(InputState state)
        {
            // Loop through each axis, finding the current value.
            var value = 0.0f;
            foreach (var axisValue in Mappings.Select(x => x.GetValue(state)))
            {
                if (Math.Abs(axisValue) > Math.Abs(value))
                    value = axisValue;
            }
            return value;
        }
    }
}
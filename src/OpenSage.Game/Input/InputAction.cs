using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Input
{
    /// <summary>
    /// Configures an input action. An input action contains a set of action mappings that abstract
    /// keyboard, mouse, and gamepad input into a convenient higher-level API. Actions are for discrete
    /// events, such as a pistol fire button. Continuous input, such as mouse movement in an FPS, should
    /// be configured using <see cref="InputAxis"/>.
    /// </summary>
    public sealed class InputAction
    {
        /// <summary>
        /// Gets or sets the action name. This is the name that should be passed to <see cref="IInputSystem.GetAction"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the mappings for this action.
        /// </summary>
        public List<InputActionMapping> Mappings { get; set; } = new List<InputActionMapping>();

        internal bool GetPressed(InputState state)
        {
            return Mappings.Any(x => x.GetPressed(state));
        }

        internal bool GetReleased(InputState state)
        {
            return Mappings.Any(x => x.GetReleased(state));
        }
    }
}
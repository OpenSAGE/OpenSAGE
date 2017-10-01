using System.Collections.Generic;

namespace OpenSage.Input
{
    /// <summary>
    /// Stores input-related settings.
    /// </summary>
    public class InputSettings
    {
        /// <summary>
        /// Stores axis mappings.
        /// </summary>
        public List<InputAxis> Axes { get; set; } = new List<InputAxis>();

        /// <summary>
        /// Stores action mappings.
        /// </summary>
        public List<InputAction> Actions { get; set; } = new List<InputAction>();
    }
}
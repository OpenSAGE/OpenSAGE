using System.Net;

namespace OpenSage
{
    // TODO: Should this be immutable?
    // TODO: Should there be a way of merging Configuration instances?
    /// <summary>
    /// Contains configuration for a game instance, typically gathered from
    /// command line parameters and configuration files.
    /// </summary>
    public sealed class Configuration
    {
        public bool LoadShellMap { get; set; } = true;
        public bool UseRenderDoc { get; set; } = false;
        public bool UseFullscreen { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to use a unique port for each client in a multiplayer game.
        /// Normally, <see cref="Network.Ports.SkirmishGame"/> is used, but when we want to run multiple game
        /// instances on the same machine (for debugging purposes), each client needs a different port.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use unique ports]; otherwise, <c>false</c>.
        /// </value>
        public bool UseUniquePorts { get; set; } = false;
    }
}

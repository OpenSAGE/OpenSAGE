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

        public IPAddress LanIpAddress { get; set; } = IPAddress.Any;
    }
}

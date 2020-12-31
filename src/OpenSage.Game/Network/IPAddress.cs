namespace OpenSage.Network
{
    public static class IPAddress
    {
        /// <summary>
        /// Gets or sets the local address.
        /// </summary>
        /// <value>
        /// The local address.
        /// </value>
        public static System.Net.IPAddress Local { get; set; }

        /// <summary>
        /// Gets or sets the NAT external address (if using UPnP).
        /// </summary>
        /// <value>
        /// The NAT external address.
        /// </value>
        public static System.Net.IPAddress NatExternal { get; set; }

        /// <summary>
        /// Gets the externally visible IP address (the external NAT address if using UPnP, otherwise the local address).
        /// </summary>
        /// <value>
        /// The external.
        /// </value>
        public static System.Net.IPAddress External => UPnP.Status switch
        {
            UPnPStatus.Enabled => NatExternal,
            UPnPStatus.PortsForwarded => NatExternal,
            _ => Local
        };
    }
}

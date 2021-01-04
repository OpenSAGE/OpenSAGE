using System;
using System.Threading;
using System.Threading.Tasks;
using Open.Nat;

namespace OpenSage.Network
{
    public enum UPnPStatus
    {
        Uninitialized,
        Disabled,
        Enabled,
        PortsForwarded
    }

    public static class UPnP
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static NatDevice NatDevice;
        private static Mapping SkirmishHostMapping;
        private static Mapping SkirmishGameMapping;

        public static UPnPStatus Status { get; private set; } = UPnPStatus.Uninitialized; 

        public static async Task InitializeAsync(TimeSpan timeout)
        {
            if (Status != UPnPStatus.Uninitialized)
            {
                throw new InvalidOperationException($"{nameof(InitializeAsync)} can only be called in status {UPnPStatus.Uninitialized}.");
            }

            var natDiscoverer = new NatDiscoverer();
            var token = new CancellationTokenSource(timeout);

            try
            {
                NatDevice = await natDiscoverer.DiscoverDeviceAsync(PortMapper.Upnp, token);
                Status = UPnPStatus.Enabled;
            }
            catch (NatDeviceNotFoundException)
            {
                Logger.Info("Failed to find UPnP device.");
                Status = UPnPStatus.Disabled;
                return;
            }

            IPAddress.NatExternal = await NatDevice.GetExternalIPAsync();
        }

        public static async Task<bool> ForwardPortsAsync()
        {
            if (Status != UPnPStatus.Enabled)
            {
                throw new InvalidOperationException($"{nameof(ForwardPortsAsync)} can only be called in status {UPnPStatus.Enabled}.");
            }

            try
            {
                SkirmishHostMapping = new Mapping(Protocol.Udp, IPAddress.Local, Ports.SkirmishHost, Ports.SkirmishHost, 0, "OpenSAGE Skirmish Host");
                await NatDevice.CreatePortMapAsync(SkirmishHostMapping);

                SkirmishGameMapping = new Mapping(Protocol.Udp, IPAddress.Local, Ports.SkirmishGame, Ports.SkirmishGame, 0, "OpenSAGE Skirmish Game");
                await NatDevice.CreatePortMapAsync(SkirmishGameMapping);

                Status = UPnPStatus.PortsForwarded;
                Logger.Info("Created port forwarding.");
                return true;
            }
            catch (Exception e)
            {
                if (SkirmishHostMapping != null)
                {
                    await NatDevice.DeletePortMapAsync(SkirmishHostMapping);
                }

                if (SkirmishGameMapping != null)
                {
                    await NatDevice.DeletePortMapAsync(SkirmishGameMapping);
                }

                Logger.Error(e, "Failed to forward port.");
                return false;
            }
        }

        public static async Task RemovePortForwardingAsync()
        {
            if (Status != UPnPStatus.PortsForwarded)
            {
                throw new InvalidOperationException($"{nameof(RemovePortForwardingAsync)} can only be called in status {UPnPStatus.PortsForwarded}.");
            }

            await NatDevice.DeletePortMapAsync(SkirmishHostMapping);
            SkirmishHostMapping = null;
            await NatDevice.DeletePortMapAsync(SkirmishGameMapping);
            SkirmishGameMapping = null;

            Status = UPnPStatus.Enabled;
            Logger.Info("Removed port forwarding.");
        }
    }
}

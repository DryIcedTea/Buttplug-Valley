using Buttplug.Client;
using Buttplug.Client.Connectors.WebsocketConnector;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace ButtplugValley
{
    internal class BPManager
    {
        private ButtplugClient client = new ButtplugClient("ButtplugValley");
        private ModEntry _modEntry;
        private IMonitor monitor;

        public async Task ScanForDevices()
        {
            // If we're not connected, don't even run
            if (!client.Connected)
            {
                return;
            }
            await client.StartScanningAsync();
            await Task.Delay(30000);
            await client.StopScanningAsync();
        }
        public async Task ConnectButtplug(IMonitor meMonitor)
        {
            monitor = meMonitor;
            // Don't stomp our client if it's already connected.
            if (client.Connected)
            {
                return;
            }
            client.Dispose();
            client = new ButtplugClient("ButtplugValley");
            await client.ConnectAsync(new ButtplugWebsocketConnector(new Uri("ws://localhost:12345")));
            client.DeviceAdded += HandleDeviceAdded;
            client.DeviceRemoved += HandleDeviceRemoved;
            client.ServerDisconnect += (object o, EventArgs e) => monitor.Log("Intiface Server disconnected.", LogLevel.Warn);
            monitor.Log("Buttplug Client Connected", LogLevel.Info);
            // Add other event handlers as needed
        }

        public async Task DisconnectButtplug()
        {
            // Doesn't *really* matter but saves an extra exception from being thrown.
            if (!client.Connected)
            {
                return;
            }
            // Disconnect from the buttplug.io server
            await client.DisconnectAsync();
        }

        private void HandleDeviceAdded(object sender, DeviceAddedEventArgs e)
        {
            monitor.Log($"Buttplug Device {e.Device.Name} ({e.Device.DisplayName} : {e.Device.Index}) Added", LogLevel.Info);
        }

        private void HandleDeviceRemoved(object sender, DeviceRemovedEventArgs e)
        {
            monitor.Log($"Buttplug Device {e.Device.Name} ({e.Device.DisplayName} : {e.Device.Index}) Removed", LogLevel.Info);
        }

        private bool HasVibrators()
        {
            if (!client.Connected)
            {
                // Noop, this ends up being way too spammy if someone is playing with the mod installed but not connected.
                return false;
            }
            else if (client.Devices.Count() == 0)
            {
                monitor.Log("Either buttplug is not connected or no devices are available");
                return false;
            }
            else if (!client.Devices.Any(device => device.VibrateAttributes.Count > 0))
            {
                monitor.Log("No connected devices have vibrators available.", LogLevel.Warn);
                return false;
            }
            return true;
        }

        public async Task VibrateDevice(float level)
        {
            // This implicited works as a Connected check, as Buttplug clears the device list on disconnect.
            if (!HasVibrators())
            {
                return;
            }
            float intensity = MathHelper.Clamp(level, 0f, 100f) / 100f;
            foreach (var device in client.Devices) 
            {
                if (device.VibrateAttributes.Count > 0)
                {
                    this.monitor.Log($"Vibrating at {intensity}", LogLevel.Debug);
                    await device.VibrateAsync(intensity);
                }
            }
        }

        //Short Vibration pulse. Intensity from 1-100
        public async Task VibrateDevicePulse(float level)
        {
            await VibrateDevicePulse(level, 380);
        }

        //Vibration with customizable duration. Intensity from 1-100
        public async Task VibrateDevicePulse(float level, int duration)
        {
            if (!HasVibrators())
            {
                return;
            }
            float intensity = MathHelper.Clamp(level, 0f, 100f) / 100f;
            await VibrateDevice(intensity);
            await Task.Delay(duration);
            await VibrateDevice(0);
        }

        public async Task StopDevices()
        {
            if (!client.Connected)
            {
                return;
            }
            // Once Buttplug C# v3.0.1 is out, just use this line.
            // 
            // await client.StopAllDevicesAsync();

            await VibrateDevice(0);
        }

        // Add other methods and event handlers as needed
    }
}
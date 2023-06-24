using Buttplug.Client;
using Buttplug.Client.Connectors.WebsocketConnector;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ButtplugValley
{
    internal class BPManager
    {
        private ButtplugClient client = new ButtplugClient("ButtplugValley");
        private ModEntry _modEntry;

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
        public async Task ConnectButtplug()
        {
            // Don't stomp our client if it's already connected.
            if (client.Connected)
            {
                return;
            }
            client.Dispose();
            client = new ButtplugClient("ButtplugValley");
            await client.ConnectAsync(new ButtplugWebsocketConnector(new Uri("ws://localhost:12345")));
            client.DeviceAdded += HandleDeviceAdded;
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
            Console.WriteLine($"Device connected: {e.Device.Name}");
        }
        
        private bool HasVibrators()
        {
            return client.Devices.Any(device => device.VibrateAttributes.Count > 0);
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
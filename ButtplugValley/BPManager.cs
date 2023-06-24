using Buttplug.Client;
using Buttplug.Client.Connectors.WebsocketConnector;
using Buttplug.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ButtplugValley
{
    internal class BPManager
    {
        private ButtplugClient client;
        private ModEntry _modEntry;

        public async Task ConnectButtplug()
        {
            client = new ButtplugClient("ButtplugValley");
            await client.ConnectAsync(new ButtplugWebsocketConnector(new Uri("ws://localhost:12345")));
            client.DeviceAdded += HandleDeviceAdded;
            // Add other event handlers as needed
        }

        public async Task DisconnectButtplug()
        {
            // Disconnect from the buttplug.io server
            await client.DisconnectAsync();
        }

        private void HandleDeviceAdded(object sender, DeviceAddedEventArgs e)
        {
            Console.WriteLine($"Device connected: {e.Device.Name}");
        }
        public async Task VibrateDevice(float level)
        {
            
            float intensity = MathHelper.Clamp(level, 0f, 100f) / 100f;
            if (this.client.Devices.Any())
            {
                var device = this.client.Devices.First();
                await device.VibrateAsync(intensity);
            }
        }
        
        public async Task VibrateDevicePulse(float level)
        {
            
            float intensity = MathHelper.Clamp(level, 0f, 100f) / 100f;
            if (this.client.Devices.Any())
            {
                var device = this.client.Devices.First();
                await device.VibrateAsync(intensity);
                await Task.Delay(380);
                await device.VibrateAsync(0);
            }
        }
        public async Task VibrateDevicePulse(float level, int duration)
        {
            
            float intensity = MathHelper.Clamp(level, 0f, 100f) / 100f;
            if (this.client.Devices.Any())
            {
                var device = this.client.Devices.First();
                await device.VibrateAsync(intensity);
                await Task.Delay(duration);
                await device.VibrateAsync(0);
            }
        }
        

        // Add other methods and event handlers as needed
    }
}
using Buttplug.Client;
using Buttplug.Client.Connectors.WebsocketConnector;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace ButtplugValley
{
    internal class BPManager
    {
        private ButtplugClient client = new ButtplugClient("ButtplugValley");
        private string _intifaceIP;
        private IMonitor monitor;
        public ModConfig config;

        private CancellationTokenSource _currentVibrationCts;
        private readonly object _vibrationLock = new object();
        private DateTime _lastVibrationTime;

        private Timer safetyTimer;

        public void InitSafetyTimer()
        {
            safetyTimer?.Dispose();
            safetyTimer = new Timer(CheckAndStopIfNeeded, null, 0, 4000);
            monitor.Log("Safety timer initialized", LogLevel.Debug);
        }

        public async Task ScanForDevices()
        {
            if (!client.Connected)
            {
                monitor.Log("Buttplug not connected, cannot scan for devices", LogLevel.Debug);
                return;
            }
            monitor.Log("Scanning for devices", LogLevel.Info);
            await client.StartScanningAsync();
            await Task.Delay(30000);
            monitor.Log("Stopping scanning for devices", LogLevel.Info);
            await client.StopScanningAsync();
        }

        public async Task ConnectButtplug(IMonitor meMonitor, string meIntifaceIP)
        {
            monitor = meMonitor;
            _intifaceIP = meIntifaceIP;
            if (client.Connected)
            {
                monitor.Log("Buttplug already connected, skipping", LogLevel.Debug);
                return;
            }
            monitor.Log("Buttplug Client Connecting", LogLevel.Info);
            client.Dispose();
            client = new ButtplugClient("ButtplugValley");
            await client.ConnectAsync(new ButtplugWebsocketConnector(new Uri($"ws://{_intifaceIP}")));
            monitor.Log($"Connecting to {_intifaceIP}", LogLevel.Debug);
            monitor.Log($"{client.Devices.Count()} devices found on startup.", LogLevel.Info);
            foreach (var device in client.Devices)
            {
                monitor.Log($"- {device.Name} ({device.DisplayName} : {device.Index})", LogLevel.Info);
            }
            client.DeviceAdded += HandleDeviceAdded;
            client.DeviceRemoved += HandleDeviceRemoved;
            client.ServerDisconnect += (object o, EventArgs e) => monitor.Log("Intiface Server disconnected.", LogLevel.Warn);
            monitor.Log("Buttplug Client Connected", LogLevel.Info);

            InitSafetyTimer();
        }

        public async Task DisconnectButtplug()
        {
            if (!client.Connected)
            {
                monitor.Log("Buttplug not connected, skipping", LogLevel.Debug);
                return;
            }

            safetyTimer?.Dispose();
            safetyTimer = null;

            monitor.Log("Disconnecting Buttplug Client", LogLevel.Info);
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
            if (!HasVibrators())
            {
                return;
            }
            float intensity = MathHelper.Clamp(level, 0f, 100f) / 100f;
            foreach (var device in client.Devices)
            {
                if (device.VibrateAttributes.Count > 0)
                {
                    monitor.Log($"Vibrating at {intensity}", LogLevel.Trace);
                    await device.VibrateAsync(intensity);
                }
                else
                {
                    monitor.Log($"No vibrators on device {device.Name}", LogLevel.Trace);
                }
            }
        }

        public async Task VibrateDevicePulse(float level)
        {
            await VibrateDevicePulse(level, 400);
        }

        public async Task VibrateDevicePulse(float level, int duration)
        {
            if (!HasVibrators())
            {
                return;
            }

            lock (_vibrationLock)
            {
                _currentVibrationCts?.Cancel();
                _currentVibrationCts = new CancellationTokenSource();
            }

            var token = _currentVibrationCts.Token;
            float intensity = MathHelper.Clamp(level, 0f, 100f);
            monitor.Log($"VibrateDevicePulse {intensity}", LogLevel.Trace);

            try
            {
                await VibrateDevice(intensity);
                _lastVibrationTime = DateTime.Now;

                try
                {
                    await Task.Delay(duration, token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                if (!token.IsCancellationRequested)
                {
                    await VibrateDevice(0);
                }
            }
            catch (Exception ex)
            {
                monitor.Log($"Error during vibration: {ex.Message}", LogLevel.Error);
                await VibrateDevice(0);
            }
        }

        public async Task StopDevices()
        {
            await VibrateDevice(0);
        }

        private void CheckAndStopIfNeeded(object state)
        {
            try
            {
                if (!Game1.hasLoadedGame || !Context.IsWorldReady)
                {
                    StopDevices();
                    return;
                }

                if (_lastVibrationTime != default &&
                    (DateTime.Now - _lastVibrationTime).TotalSeconds > 30)
                {
                    monitor.Log("Vibration has been on too long, stopping as safety measure", LogLevel.Warn);
                    StopDevices();
                }
            }
            catch (Exception ex)
            {
                StopDevices();
            }
        }
    }
}
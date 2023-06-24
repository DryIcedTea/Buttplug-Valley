using System;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ButtplugValley
{
    internal sealed class ModEntry : Mod
    {
        private BPManager buttplugManager;
        //private FishingMinigame fishingMinigame;
        private bool isVibrating = false;
        private int previousHealth;

        public override void Entry(IModHelper helper)
        {
            buttplugManager = new BPManager();
            Task.Run(async () =>
            {
                await buttplugManager.ConnectButtplug();
                await buttplugManager.ScanForDevices();
                //fishingMinigame = new FishingMinigame(helper);
            });

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.DayEnding += OnDayEnding;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.World.TerrainFeatureListChanged += OnTerrainFeatureListChanged;
            helper.Events.Player.InventoryChanged += OnInventoryChanged;
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            Task.Run(async () =>
            {
                this.Monitor.Log($"{Game1.player.Name} VIBRATING AT {50} then 100.", LogLevel.Debug);
                await buttplugManager.VibrateDevice(50);
                await Task.Delay(800);
                await buttplugManager.VibrateDevice(80);
                await Task.Delay(400);
                await buttplugManager.VibrateDevice(100);
                await Task.Delay(200);
                await buttplugManager.VibrateDevice(0);
            });
            
        }

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            // Check if any items were removed from the inventory
            
            foreach (Item item in e.Added)
            {
                // Check if the removed item is a crop
                if (item is StardewValley.Object obj)
                {
                    if (obj.Category == StardewValley.Object.FishCategory)
                    {
                        if (obj.Quality == StardewValley.Object.medQuality) buttplugManager.VibrateDevicePulse(55);
                        else if (obj.Quality == StardewValley.Object.highQuality) buttplugManager.VibrateDevicePulse(85, 650);
                        else if (obj.Quality == StardewValley.Object.bestQuality) buttplugManager.VibrateDevicePulse(100, 1000);
                        // Vibrate the device
                        else buttplugManager.VibrateDevicePulse(30); // Adjust the power level as desired
                        break; // Exit the loop after the first harvested crop is found
                    }
                    if (obj.Category == StardewValley.Object.VegetableCategory ||
                        obj.Category == StardewValley.Object.FruitsCategory || obj.Category == StardewValley.Object.MilkCategory )
                    {
                        if (obj.Quality == StardewValley.Object.medQuality) buttplugManager.VibrateDevicePulse(55);
                        else if (obj.Quality == StardewValley.Object.highQuality) buttplugManager.VibrateDevicePulse(85, 650);
                        else if (obj.Quality == StardewValley.Object.bestQuality) buttplugManager.VibrateDevicePulse(100, 1200);
                        // Vibrate the device
                        else buttplugManager.VibrateDevicePulse(30); // Adjust the power level as desired
                        break; // Exit the loop after the first harvested crop is found
                    }
                }
            }
            foreach (ItemStackSizeChange change in e.QuantityChanged)
            {
                // Check if the changed item is a crop
                if (change.Item is StardewValley.Object obj)
                {
                    if (obj.Category == StardewValley.Object.FishCategory)
                    {
                        if (obj.Quality == StardewValley.Object.medQuality)
                            buttplugManager.VibrateDevicePulse(55);
                        else if (obj.Quality == StardewValley.Object.highQuality)
                            buttplugManager.VibrateDevicePulse(85, 650);
                        else if (obj.Quality == StardewValley.Object.bestQuality)
                            buttplugManager.VibrateDevicePulse(100, 1000);
                        else
                            buttplugManager.VibrateDevicePulse(30); // Adjust the power level as desired
                        break; // Exit the loop after the first harvested crop is found
                    }
                    if (obj.Category == StardewValley.Object.VegetableCategory ||
                        obj.Category == StardewValley.Object.FruitsCategory)
                    {
                        if (obj.Quality == StardewValley.Object.medQuality)
                            buttplugManager.VibrateDevicePulse(55);
                        else if (obj.Quality == StardewValley.Object.highQuality)
                            buttplugManager.VibrateDevicePulse(85, 650);
                        else if (obj.Quality == StardewValley.Object.bestQuality)
                            buttplugManager.VibrateDevicePulse(100, 1200);
                        else
                            buttplugManager.VibrateDevicePulse(30); // Adjust the power level as desired
                        break; // Exit the loop after the first harvested crop is found
                    }
                }
            }
        }

        private void OnTerrainFeatureListChanged(object sender, TerrainFeatureListChangedEventArgs e)
        {
            if (e.IsCurrentLocation)
            {
                foreach (var feature in e.Removed)
                {
                    if (feature.Value is Tree tree)
                    {
                        if (tree.stump.Value)
                        {
                            // Tree is fully chopped
                            Task.Run(async () =>
                            {
                                this.Monitor.Log($"{Game1.player.Name} VIBRATING AT {80}.", LogLevel.Debug);
                                await buttplugManager.VibrateDevice(80);
                                await Task.Delay(420);
                                await buttplugManager.VibrateDevice(0);
                            });
                        }
                        else
                        {
                            // Tree is being chopped. This does not work. If someone knows how to fix, then please do.
                            Task.Run(async () =>
                            {
                                this.Monitor.Log($"{Game1.player.Name} VIBRATING AT {50}.", LogLevel.Debug);
                                await buttplugManager.VibrateDevice(50);
                                await Task.Delay(380);
                                await buttplugManager.VibrateDevice(0);
                            });
                        }
                    }
                }
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {

            // print button presses to the console window
            //this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

            // Check if the button pressed is the desired button
            /*if (e.Button == SButton.A)
            {
                // Trigger the device vibration
                Task.Run(async () =>
                {
                    await buttplugManager.VibrateDevice(50);
                    await Task.Delay(200);
                    await buttplugManager.VibrateDevice(0);
                });
            }
            if (e.Button == SButton.Space)
            {
                // Toggle the vibration state
                isVibrating = !isVibrating;
                buttplugManager.VibrateDevice(isVibrating ? 100 : 0);
            }*/
            if (e.Button == SButton.P)
            {
                // Stop Vibrations
                Task.Run(async () => await buttplugManager.StopDevices());
            }
            if (e.Button == SButton.I)
            {
                Task.Run(async () =>
                {
                    await buttplugManager.StopDevices();
                    await buttplugManager.DisconnectButtplug();
                });

            }
            if (e.Button == SButton.K)
            {
                // Reconnect
                Task.Run(async () =>
                {
                    await buttplugManager.ConnectButtplug();
                });
            }
        }
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            //fishingMinigame.previousCaptureLevel = 0f;
            Task.Run(async () =>
            {
                await buttplugManager.VibrateDevice(50);
                await Task.Delay(150);
                await buttplugManager.VibrateDevice(0);

                await Task.Delay(350);

                await buttplugManager.VibrateDevice(50);
                await Task.Delay(150);
                await buttplugManager.VibrateDevice(0);
            });
        }
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // Check if the player's health has decreased since the last tick
            if (Game1.player.health < previousHealth)
            {
                float intensity = 100f * (1f - (float)Game1.player.health / (float)Game1.player.maxHealth);
                intensity = Math.Min(intensity, 100f);
                Task.Run(async () =>
                {
                    this.Monitor.Log($"{Game1.player.Name} VIBRATING AT {intensity}.", LogLevel.Debug);
                    await buttplugManager.VibrateDevice(intensity);
                    await Task.Delay(380);
                    await buttplugManager.VibrateDevice(0);
                });
            }
            // Update the previous health value for the next tick
            previousHealth = Game1.player.health;
        }

        public void Unload()
        {
            buttplugManager.DisconnectButtplug();
        }
    }
}

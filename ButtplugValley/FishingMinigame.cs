using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ButtplugValley
{
    internal class FishingMinigame
    {
        private IModHelper helper;
        private IReflectionHelper reflectionHelper;
        public float previousCaptureLevel;
        private OldBPManager _bpManager;
        private IMonitor monitor;

        public FishingMinigame(IModHelper modHelper, IMonitor MeMonitor, OldBPManager MEbpManager)
        {
            helper = modHelper;
            monitor = MeMonitor;
            _bpManager = MEbpManager;
            reflectionHelper = helper.Reflection;
            previousCaptureLevel = 0f;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            FishingCheck();
        }

        private void FishingCheck()
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.activeClickableMenu is StardewValley.Menus.BobberBar menu)
            {
                monitor.Log("FishingMinigameIsActive", LogLevel.Debug);
        
                // Get the distanceFromCatching field using reflection
                IReflectedField<float> distanceFromCatchingField = this.reflectionHelper.GetField<float>(menu, "distanceFromCatching");

                if (distanceFromCatchingField == null)
                {
                    monitor.Log("distanceFromCatching field not found", LogLevel.Debug);
                    return;
                }

                float captureLevel = distanceFromCatchingField.GetValue();
                monitor.Log($"distancefrom {captureLevel}", LogLevel.Debug);

                // Convert capture level to percentage
                float capturePercentage = captureLevel * 100f;

                // Vibrate the device based on the capture percentage if it has changed
                if (capturePercentage != previousCaptureLevel)
                {
                    monitor.Log($"Vibrating at {capturePercentage}", LogLevel.Debug);
                    _bpManager.VibrateDevice(capturePercentage);
                    previousCaptureLevel = capturePercentage;
                }
            }
        }


        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Reset previous capture level when a new day starts
            previousCaptureLevel = 0f;
        }
        
    }
}
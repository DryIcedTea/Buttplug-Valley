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
        private BPManager _bpManager;

        public FishingMinigame(IModHelper modHelper)
        {
            helper = modHelper;
            reflectionHelper = helper.Reflection;
            previousCaptureLevel = 0f;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // Your code for starting/stopping vibrations based on the fishing minigame button presses
            // Use the ButtplugManager instance or pass it as a parameter to access the Buttplug functionality
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            FishingCheck();
        }

        private void FishingCheck()
        {
            if (!Context.IsWorldReady)
                return;
            if (Game1.activeClickableMenu is StardewValley.Menus.IClickableMenu menu && menu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
            {
                if (Game1.currentMinigame == null)
                    return;
                // Get the distanceFromCatching field using reflection
                IReflectedField<float> distanceFromCatchingField = this.reflectionHelper.GetField<float>(Game1.currentMinigame, "distanceFromCatching");
                float captureLevel = distanceFromCatchingField.GetValue();

                // Convert capture level to percentage
                float capturePercentage = captureLevel * 100f;

                // Vibrate the device based on the capture percentage if it has changed
                if (capturePercentage != previousCaptureLevel)
                {
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

        // Add other methods and event handlers as needed
    }
}
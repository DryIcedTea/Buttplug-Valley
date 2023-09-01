using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ButtplugValley
{
    internal class FishingRod
    {
        private BPManager _bpManager;
        private ModConfig modConfig;
        private IModHelper helper;
        private IMonitor monitor;
        private bool isActive = true;
        private float maxVibration = 100f; // Adjust as desired

        public FishingRod(IModHelper modHelper, IMonitor MeMonitor, BPManager MEbpManager, ModConfig ModConfig)
        {
            helper = modHelper;
            monitor = MeMonitor;
            modConfig = ModConfig;
            _bpManager = MEbpManager;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            isActive = modConfig.VibrateOnFishingRodUsage;
            if (!isActive) return;
            if (Game1.player.CurrentTool is StardewValley.Tools.FishingRod rod)
            {
                if (!rod.isFishing) return;
                monitor.Log("FishingRodIsActive", LogLevel.Debug);
                maxVibration = modConfig.MaxFishingVibration;
                Casting(rod);
                Nibbling(rod);
                Hit(rod);
                Miss(rod);
            }
            
        }

        /// <summary>
        /// Vibrate on casting and holding for power
        /// </summary>
        private void Casting(StardewValley.Tools.FishingRod rod)
        {

        }

        /// <summary>
        /// Tell user that fish is nibbling
        /// </summary>
        private void Nibbling(StardewValley.Tools.FishingRod rod)
        {

        }


        /// <summary>
        /// Inform user of successful hit
        /// </summary>
        private void Hit(StardewValley.Tools.FishingRod rod)
        {

        }

        /// <summary>
        /// Inform user of failed hit
        /// </summary>
        private void Miss(StardewValley.Tools.FishingRod rod)
        {

        }
    }
}

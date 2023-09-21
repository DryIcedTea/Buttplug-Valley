using System;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ButtplugValley
{
    internal class FishingRod
    {
        private BPManager _bpManager;
        private ModConfig _modConfig;
        private IModHelper _helper;
        private IMonitor _monitor;
        private bool isActive = true;
        private float maxVibration = 100f; // Adjust as desired

        private bool wasNibbling = false;
        private bool wasHit = false;

        public FishingRod(IModHelper modHelper, IMonitor MeMonitor, BPManager MEbpManager, ModConfig ModConfig)
        {
            _helper = modHelper;
            _monitor = MeMonitor;
            _modConfig = ModConfig;
            _bpManager = MEbpManager;
            _helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            isActive = _modConfig.VibrateOnFishingRodUsage;
            if (!isActive) return;
            if (Game1.player.CurrentTool is StardewValley.Tools.FishingRod rod)
            {
                if (!rod.inUse())
                {
                    _monitor.Log("reset fishing rod", LogLevel.Trace);
                    wasNibbling = false;
                    wasHit = false;
                    return;
                }
                if (hasFailedCatch())
                {
                    _monitor.Log("Fail to catch fish", LogLevel.Debug);
                    Miss(rod);
                    wasNibbling = false;
                }    
                
                _monitor.Log("FishingRodIsActive", LogLevel.Debug);
                maxVibration = _modConfig.MaxFishingVibration;

                Casting(rod);
                Nibbling(rod, e.Ticks);
                Hit(rod);

            }

            bool hasFailedCatch()
            {
                return rod.timeUntilFishingNibbleDone < 0 && wasNibbling && !wasHit;
            }
            
        }

        /// <summary>
        /// Tell user current casting power level
        /// </summary>
        /// <param name="rod"></param>
        private void Casting(StardewValley.Tools.FishingRod rod)
        {
            if (rod.isTimingCast)
            {
                _ = _bpManager.VibrateDevice(maxVibration * 0.5f * rod.castingPower);
            }
            else if (rod.castedButBobberStillInAir)
            {
                _ = _bpManager.VibrateDevice(0f);
            }
            
        }

        /// <summary>
        /// Tell player the fish is bitting
        /// </summary>
        /// <param name="rod"></param>
        /// <param name="ticks">time since start of game</param>
        private void Nibbling(StardewValley.Tools.FishingRod rod, uint ticks)
        {
            if (!wasNibbling)
            {
                if (rod.isNibbling)
                {
                    _ = _bpManager.VibrateDevicePulse(maxVibration * .8f);
                    wasNibbling = true;
                }
                else if (ticks % 110 == 0)
                {
                    _ = _bpManager.VibrateDevicePulse(maxVibration * 0.2f, 100);
                }
            }
        }

        /// <summary>
        /// Tell player fish is on the line
        /// </summary>
        /// <param name="rod"></param>
        private void Hit(StardewValley.Tools.FishingRod rod)
        {
            if (rod.hit && !wasHit)
            {
                _ =_bpManager.VibrateDevicePulse(maxVibration);
                wasHit = true;
            }
        }

        /// <summary>
        /// Give a woop woop to the player that the fish is gone
        /// </summary>
        /// <param name="rod"></param>
        private void Miss(StardewValley.Tools.FishingRod rod)
        {
            Task.Run(async () =>
            {
                await _bpManager.VibrateDevicePulse(maxVibration * .3f, 200);
                await Task.Delay(400);
                await _bpManager.VibrateDevicePulse(maxVibration * .1f, 500);
            });
        }
    }
}

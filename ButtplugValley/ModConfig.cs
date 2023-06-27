using StardewModdingAPI;

namespace ButtplugValley
{
    public sealed class ModConfig
    {
        public bool VibrateOnStoneBroken { get; set; } = true;
        public bool VibrateOnDamageTaken { get; set; } = true;
        
        public bool VibrateOnEnemyKilled { get; set; } = true;
        public bool VibrateOnFishCollected { get; set; } = true;
        public bool VibrateOnCropAndMilkCollected { get; set; } = true;
        public bool VibrateOnTreeBroken { get; set; } = true;
        public bool VibrateOnDayStart { get; set; } = true;
        public bool VibrateOnDayEnd { get; set; } = true;
        
        public bool VibrateOnFishingMinigame { get; set; } = true;
        
        
        public int StoneBrokenLevel { get; set; } = 35;
        public int DamageTakenMax { get; set; } = 100;
        
        public int EnemyKilledLevel { get; set; } = 35;
        public int FishCollectedBasic { get; set; } = 30;
        public int CropAndMilkBasic { get; set; } = 30;
        public int SilverLevel { get; set; } = 55;
        public int GoldLevel { get; set; } = 85;
        public int IridiumLevel { get; set; } = 100;
        public int TreeBrokenLevel { get; set; } = 80;
        public int DayStartLevel { get; set; } = 50;
        public int DayEndMax { get; set; } = 100;
        public int MaxFishingVibration { get; set; } = 100;
        
        public SButton StopVibrations { get; set; } = SButton.P;
        public SButton DisconnectButtplug { get; set; } = SButton.I;
        public SButton ReconnectButtplug { get; set; } = SButton.K;

        public string IntifaceIP { get; set; } = "localhost:12345";
    }
}
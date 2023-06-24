namespace ButtplugValley
{
    public sealed class ModConfig
    {
        public bool VibrateOnStoneBroken { get; set; } = true;
        public bool VibrateOnDamageTaken { get; set; } = true;
        public bool VibrateOnFishCollected { get; set; } = true;
        public bool VibrateOnCropAndMilkCollected { get; set; } = true;
        public bool VibrateOnTreeBroken { get; set; } = true;
        public bool VibrateOnDayStart { get; set; } = true;
        public bool VibrateOnDayEnd { get; set; } = true;
    }
}
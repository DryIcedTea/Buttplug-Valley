using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ButtplugValley;

internal class ConfigMenu
{
    private IModHelper Helper;
    private IReflectionHelper reflectionHelper;
    public float previousCaptureLevel;
    private BPManager _bpManager;
    private IMonitor monitor;
    private IManifest ModManifest;
    private ModConfig Config;
    
    public ConfigMenu(IModHelper modHelper, IMonitor MeMonitor, BPManager MEbpManager, ModConfig MEConfig, IManifest modManifest)
    {
        Helper = modHelper;
        monitor = MeMonitor;
        _bpManager = MEbpManager;
        reflectionHelper = Helper.Reflection;
        Config = MEConfig;
        ModManifest = modManifest;
    }

    public void LoadConfigMenu()
    {
        // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );
            
            configMenu.AddPageLink(
                mod: this.ModManifest,
                pageId: "ButtplugValley.VibrationEvents",
                text: () => "Vibration Events"
            );
            
            configMenu.AddPageLink(
                mod: this.ModManifest,
                pageId: "ButtplugValley.VibrationLevels",
                text: () => "Vibration Levels"
            );
            configMenu.AddPageLink(
                mod: this.ModManifest,
                pageId: "ButtplugValley.Keybinds",
                text: () => "Keybinds"
            );
            configMenu.AddPageLink(
                mod: this.ModManifest,
                pageId: "ButtplugValley.OtherSettings",
                text: () => "Other Settings"
            ); 
            
            configMenu.AddPage(
                mod: this.ModManifest,
                pageId: "ButtplugValley.VibrationEvents",
                pageTitle: () => "Vibration Events"
            );
            
            configMenu.AddSectionTitle(mod:this.ModManifest, text: () => "Vibration Events");

            // add some config options
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Crop and Milk Pickup",
                tooltip: () => "Should the device vibrate on collecting crops and milk?",
                getValue: () => this.Config.VibrateOnCropAndMilkCollected,
                setValue: value => this.Config.VibrateOnCropAndMilkCollected = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Fish Pickup",
                tooltip: () => "Should the device vibrate on collecting fish?",
                getValue: () => this.Config.VibrateOnFishCollected,
                setValue: value => this.Config.VibrateOnFishCollected = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Flower Pickup",
                tooltip: () => "Should the device vibrate on collecting flowers?",
                getValue: () => this.Config.VibrateOnFlowersCollected,
                setValue: value => this.Config.VibrateOnFlowersCollected = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Foraging Pickup",
                tooltip: () => "Should the device vibrate on collecting foraging?",
                getValue: () => this.Config.VibrateOnForagingCollected,
                setValue: value => this.Config.VibrateOnForagingCollected = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Stone Broken",
                tooltip: () => "Should the device vibrate on breaking stone and ores?",
                getValue: () => this.Config.VibrateOnStoneBroken,
                setValue: value => this.Config.VibrateOnStoneBroken = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Tree Broken",
                tooltip: () => "Should the device vibrate on fully chopping down a tree?",
                getValue: () => this.Config.VibrateOnTreeBroken,
                setValue: value => this.Config.VibrateOnTreeBroken = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Damage Taken",
                tooltip: () => "Should the device vibrate on taking damage? Scales with health",
                getValue: () => this.Config.VibrateOnDamageTaken,
                setValue: value => this.Config.VibrateOnDamageTaken = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enemy Killed",
                tooltip: () => "Should the device vibrate on killing an enemy? Scales with enemies killed at once.",
                getValue: () => this.Config.VibrateOnEnemyKilled,
                setValue: value => this.Config.VibrateOnEnemyKilled = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Day Start",
                tooltip: () => "Should the device vibrate when the day starts?",
                getValue: () => this.Config.VibrateOnDayStart,
                setValue: value => this.Config.VibrateOnDayStart = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Day Ending",
                tooltip: () => "Should the device vibrate when the day ends?",
                getValue: () => this.Config.VibrateOnDayEnd,
                setValue: value => this.Config.VibrateOnDayEnd = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Fishing Minigame",
                tooltip: () => "Should the device vibrate in the fishing minigame? Scales with the capture bar",
                getValue: () => this.Config.VibrateOnFishingMinigame,
                setValue: value => this.Config.VibrateOnFishingMinigame = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Fishing Rod",
                tooltip: () => "Should the device vibrate when using fishing rod",
                getValue: () => this.Config.VibrateOnFishingRodUsage,
                setValue: value => this.Config.VibrateOnFishingRodUsage = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Arcade Minigames",
                tooltip: () => "Should the device vibrate on certain events in the arcade minigames?",
                getValue: () => this.Config.VibrateOnArcade,
                setValue: value => this.Config.VibrateOnArcade = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Dialogue Boxes",
                tooltip: () => "Should the device vibrate on opening a dialogue box?",
                getValue: () => this.Config.VibrateOnDialogue,
                setValue: value => this.Config.VibrateOnDialogue = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Horse Riding",
                tooltip: () => "Should the device vibrate while riding a horse?",
                getValue: () => this.Config.VibrateOnHorse,
                setValue: value => this.Config.VibrateOnHorse = value
            );
            
            // configMenu.AddBoolOption(
            //     mod: this.ModManifest,
            //     name: () => "STONE PICK UP (Test version only)",
            //     tooltip: () => "Should the device vibrate on picking up stone and ore?",
            //     getValue: () => this.Config.StonePickedUpDebug,
            //     setValue: value => this.Config.StonePickedUpDebug = value
            // );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Keep Alive Pulse",
                tooltip: () => "Vibrate every 30s to keep connection alive?",
                getValue: () => this.Config.KeepAlive,
                setValue: value => this.Config.KeepAlive = value
            );
            /*
             * 
             * VIBRATION LEVELS
             * 
             */
            
            configMenu.AddPage(
                mod: this.ModManifest,
                pageId: "ButtplugValley.VibrationLevels",
                pageTitle: () => "Vibration Levels"
            );
            
            configMenu.AddSectionTitle(mod:this.ModManifest, text: () => "Vibration Levels (0-100)");
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Basic Crops and Milk",
                tooltip: () => "How Strong should the vibration be for normal milk and crops?",
                getValue: () => this.Config.CropAndMilkBasic,
                setValue: value => this.Config.CropAndMilkBasic = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Basic Fish Pickup",
                tooltip: () => "How Strong should the vibration be for picking up normal fish?",
                getValue: () => this.Config.FishCollectedBasic,
                setValue: value => this.Config.FishCollectedBasic = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Basic Flower Pickup",
                tooltip: () => "How Strong should the vibration be for picking up normal flowers?",
                getValue: () => this.Config.FlowerBasic,
                setValue: value => this.Config.FlowerBasic = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Basic Foraging Pickup",
                tooltip: () => "How Strong should the vibration be for picking up normal foraging?",
                getValue: () => this.Config.ForagingBasic,
                setValue: value => this.Config.ForagingBasic = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Silver Fish, Crops and Milk",
                tooltip: () => "How Strong should the vibration be for ALL silver fish, crops and milk?",
                getValue: () => this.Config.SilverLevel,
                setValue: value => this.Config.SilverLevel = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Gold Fish, Crops and Milk",
                tooltip: () => "How Strong should the vibration be for ALL Gold fish, crops and milk?",
                getValue: () => this.Config.GoldLevel,
                setValue: value => this.Config.GoldLevel = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Iridium Fish, Crops and Milk",
                tooltip: () => "How Strong should the vibration be for ALL Iridium fish, crops and milk?",
                getValue: () => this.Config.IridiumLevel,
                setValue: value => this.Config.IridiumLevel = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Stone Broken",
                tooltip: () => "How Strong should the vibration be?",
                getValue: () => this.Config.StoneBrokenLevel,
                setValue: value => this.Config.StoneBrokenLevel = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Tree Broken",
                tooltip: () => "How Strong should the vibration be for breaking a tree?",
                getValue: () => this.Config.TreeBrokenLevel,
                setValue: value => this.Config.TreeBrokenLevel = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Damage Taken Max",
                tooltip: () => "How Strong should the MAX vibration be when taking damage?",
                getValue: () => this.Config.DamageTakenMax,
                setValue: value => this.Config.DamageTakenMax = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Enemy Killed",
                tooltip: () => "How Strong should the vibration be when killing an enemy?",
                getValue: () => this.Config.EnemyKilledLevel,
                setValue: value => this.Config.EnemyKilledLevel = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Day Start",
                tooltip: () => "How Strong should the vibration be when the day starts?",
                getValue: () => this.Config.DayStartLevel,
                setValue: value => this.Config.DayStartLevel = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Day End",
                tooltip: () => "How Strong should the MAX vibration be when the day ends? Min 50",
                getValue: () => this.Config.DayEndMax,
                setValue: value => this.Config.DayEndMax = value,
                min: 50,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Fishing Minigame",
                tooltip: () => "How Strong should the MAX vibration be in the fishing minigame?",
                getValue: () => this.Config.MaxFishingVibration,
                setValue: value => this.Config.MaxFishingVibration = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Arcade Minigames",
                tooltip: () => "How Strong should the vibration be in the arcade minigames?",
                getValue: () => this.Config.ArcadeLevel,
                setValue: value => this.Config.ArcadeLevel = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Dialogue Box",
                tooltip: () => "How Strong should the vibration be when opening a dialogue box?",
                getValue: () => this.Config.DialogueLevel,
                setValue: value => this.Config.DialogueLevel = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Horse Riding",
                tooltip: () => "How Strong should the vibration be when riding a horse?",
                getValue: () => this.Config.HorseLevel,
                setValue: value => this.Config.HorseLevel = value,
                min: 0,
                max: 100
            );
            
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Keep Alive Interval",
                tooltip: () => "How frequently should the Keep alive signal be sent (in seconds)",
                getValue: () => this.Config.KeepAliveInterval,
                setValue: value => this.Config.KeepAliveInterval = value,
                min: 5,
                max: 300,
                interval: 5
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Keep Alive Intensity",
                tooltip: () => "How strong should the keep alive vibration be?",
                getValue: () => this.Config.KeepAliveLevel,
                setValue: value => this.Config.KeepAliveLevel = value,
                min: 0,
                max: 100
            );
            /*
             * 
             * Keybinds
             * 
             */
            
            configMenu.AddPage(
                mod: this.ModManifest,
                pageId: "ButtplugValley.Keybinds",
                pageTitle: () => "Keybinds"
            );
            
            configMenu.AddSectionTitle(mod:this.ModManifest, text: () => "Keybinds");
            configMenu.AddKeybind(
                mod: this.ModManifest,
                name: () => "Stop Vibrations",
                tooltip: () => "Stops all ongoing vibrations",
                getValue: () => this.Config.StopVibrations,
                setValue: value => this.Config.StopVibrations = value
            );
            configMenu.AddKeybind(
                mod: this.ModManifest,
                name: () => "Disconnect",
                tooltip: () => "Disconnects the game from intiface",
                getValue: () => this.Config.DisconnectButtplug,
                setValue: value => this.Config.DisconnectButtplug = value
            );
            configMenu.AddKeybind(
                mod: this.ModManifest,
                name: () => "Reconnect",
                tooltip: () => "Reconnects the game to intiface",
                getValue: () => this.Config.ReconnectButtplug,
                setValue: value => this.Config.ReconnectButtplug = value
            );
            
            /*
             * 
             * Intiface Connection
             * 
             */
            
            configMenu.AddPage(
                mod: this.ModManifest,
                pageId: "ButtplugValley.OtherSettings",
                pageTitle: () => "Other Settings"
            );
            configMenu.AddSectionTitle(mod:this.ModManifest, text: () => "Edit IP");
            configMenu.AddParagraph(mod:this.ModManifest, text: () => "Press the Reconnect keybind after saving to reconnect. Ignore this if you don't know what this is.");
            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "Intiface IP",
                tooltip: () => "The address used to connect to intiface. Leave default if you don't know what this is",
                getValue: () => this.Config.IntifaceIP,
                setValue: value => this.Config.IntifaceIP = value
            );
            configMenu.AddSectionTitle(mod:this.ModManifest, text: () => "Max Queued Vibrations");
            configMenu.AddParagraph(mod:this.ModManifest, text: () => "How many vibrations can be queued up at once. Might be useful to limit this to prevent an extremely long vibration.");
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Queue Length",
                tooltip: () => "Max amount of queued vibrations",
                getValue: () => this.Config.QueueLength,
                setValue: value => this.Config.QueueLength = value,
                min: 1,
                max: 100
            );
        
            configMenu.AddSectionTitle(mod:this.ModManifest, text: () => "Grass feedback duration (debug)");
            configMenu.AddParagraph(mod:this.ModManifest, text: () => "The duration (in milliseconds) of feedback when cutting grass");
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Grass Duration",
                tooltip: () => "Duration in milliseconds for cutting grass",
                getValue: () => this.Config.GrassLength,
                setValue: value => this.Config.GrassLength = value,
                min: 50,
                max: 3000
            );
            
            configMenu.AddSectionTitle(mod:this.ModManifest, text: () => "Tree feedback duration (debug)");
            configMenu.AddParagraph(mod:this.ModManifest, text: () => "Feedback strength for hitting and felling trees");
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Tree Hit Level",
                tooltip: () => "Feedback strength when hitting trees",
                getValue: () => this.Config.TreeChopLevel,
                setValue: value => this.Config.TreeChopLevel = value,
                min: 0,
                max: 100
            );
            
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Tree Fell Level",
                tooltip: () => "Feedback strength when felling trees",
                getValue: () => this.Config.TreeFellLevel,
                setValue: value => this.Config.TreeFellLevel = value,
                min: 0,
                max: 100
            );
            configMenu.AddSectionTitle(mod:this.ModManifest, text: () => "Tool feedback");
            configMenu.AddParagraph(mod:this.ModManifest, text: () => "Feedback strength for using various tools. Strength is multiplied by tool power up to a max of 100 (recommended is 25)");
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Watering can level",
                tooltip: () => "Feedback strength when using the watering can",
                getValue: () => this.Config.WateringCanLevel,
                setValue: value => this.Config.WateringCanLevel = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Hoe level",
                tooltip: () => "Feedback strength when using the hoe",
                getValue: () => this.Config.HoeLevel,
                setValue: value => this.Config.HoeLevel = value,
                min: 0,
                max: 100
            );
    }
}
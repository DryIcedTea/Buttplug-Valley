using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Minigames;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace ButtplugValley
{
    internal sealed class ModEntry : Mod
    {
        private BPManager buttplugManager;
        private ModConfig Config;
        private FishingMinigame fishingMinigame;
        private bool isVibrating = false;
        private int previousHealth;
        private int previousCoins;
        private int _levelUps;
        
        //Arcade Machines
        private int previousMinekartHealth;
        private int previousAbigailHealth;
        private int previousPowerupCount;
        private bool isGameOverAbigail = false;

        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            buttplugManager = new BPManager();
            fishingMinigame = new FishingMinigame(helper, Monitor, buttplugManager);
            Task.Run(async () =>
            {
                await buttplugManager.ConnectButtplug(Monitor, Config.IntifaceIP);
                await buttplugManager.ScanForDevices();
                
            });

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.DayEnding += OnDayEnding;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.World.TerrainFeatureListChanged += OnTerrainFeatureListChanged;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            helper.Events.Player.InventoryChanged += OnInventoryChanged;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.World.NpcListChanged += OnNpcListChanged;
            helper.Events.Player.LevelChanged += OnLevelChanged;
        }

        private void OnLevelChanged(object sender, LevelChangedEventArgs e)
        {
            if (e.IsLocalPlayer)
            {
                if (e.NewLevel > e.OldLevel)
                {
                    _levelUps++;
                }
            }
        }

        private void OnNpcListChanged(object sender, NpcListChangedEventArgs e)
        {
            // Get the defeated monsters from the removed NPCs
            var defeatedMonsters = e.Removed.Where(npc => npc.IsMonster).ToList();
            var defeatedEnemyCount = 0;
            
            if (defeatedMonsters.Any() && Config.VibrateOnEnemyKilled)
            {
                // Increment the defeated enemy count
                defeatedEnemyCount += defeatedMonsters.Count;
                Monitor.Log($"Defeated {defeatedEnemyCount} enemies", LogLevel.Debug);
                

                // Vibrate the device
                buttplugManager.VibrateDevicePulse(Config.EnemyKilledLevel, 400*defeatedEnemyCount);
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
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
                name: () => "Arcade Minigames",
                tooltip: () => "Should the device vibrate on certain events in the arcade minigames?",
                getValue: () => this.Config.VibrateOnArcade,
                setValue: value => this.Config.VibrateOnArcade = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Keep Alive Pulse",
                tooltip: () => "Vibrate every 30s to keep connection alive?",
                getValue: () => this.Config.KeepAlive,
                setValue: value => this.Config.KeepAlive = value
            );
            /*
             * VIBRATION LEVELS
             */
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
            /*
             * Keybinds
             */
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
             * Intiface Connection
             */
            configMenu.AddSectionTitle(mod:this.ModManifest, text: () => "Edit IP");
            configMenu.AddParagraph(mod:this.ModManifest, text: () => "Press the Reconnect keybind after saving to reconnect. Ignore this if you don't know what this is.");
            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "Intiface IP",
                tooltip: () => "The address used to connect to intiface. Leave default if you don't know what this is",
                getValue: () => this.Config.IntifaceIP,
                setValue: value => this.Config.IntifaceIP = value
            );
        }

        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            // Get the destroyed stones
            var destroyedStones = e.Removed.Where(pair =>
            {
                if (pair.Value is StardewValley.Object obj)
                {
                    return obj.Name == "Stone" || obj.Name.Contains("Ore");
                }
                return false;
            }).ToList();
            var destroyedStoneCount = 0;
            
            if (destroyedStones.Any())
            {
                // Increment the destroyed stone count
                destroyedStoneCount += destroyedStones.Count;
                //print the count
                this.Monitor.Log($"DESTROYED {destroyedStoneCount} STONES", LogLevel.Debug);
                double durationmath = 3920 / (1 + (10 * Math.Exp(-0.16 * destroyedStoneCount)));
                int duration = Convert.ToInt32(durationmath);

                // Vibrate the device
                this.Monitor.Log($"VIBRATING FOR {duration} milliseconds", LogLevel.Debug);
                buttplugManager.VibrateDevicePulse(Config.StoneBrokenLevel, duration);
            }
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            Task.Run(async () =>
            {
                if (!Config.VibrateOnDayEnd) return;
                
                var level = Config.DayEndMax;
                this.Monitor.Log($"{Game1.player.Name} VIBRATING AT {50} then 100.", LogLevel.Trace);
                await buttplugManager.VibrateDevice(level-50);
                await Task.Delay(800 + (500*_levelUps));
                await buttplugManager.VibrateDevice(level-20);
                await Task.Delay(400 + (250*_levelUps));
                await buttplugManager.VibrateDevice(level);
                await Task.Delay(200 + (125*_levelUps));
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
                    
                    if (obj.Category == StardewValley.Object.FishCategory && Config.VibrateOnFishCollected)
                    {
                        this.Monitor.Log("Fish", LogLevel.Debug);
                        if (obj.Quality == StardewValley.Object.medQuality) buttplugManager.VibrateDevicePulse(Config.SilverLevel);
                        else if (obj.Quality == StardewValley.Object.highQuality) buttplugManager.VibrateDevicePulse(Config.GoldLevel, 650);
                        else if (obj.Quality == StardewValley.Object.bestQuality) buttplugManager.VibrateDevicePulse(Config.IridiumLevel, 1000);
                        // Vibrate the device
                        else buttplugManager.VibrateDevicePulse(Config.FishCollectedBasic); // Adjust the power level as desired
                        break; // Exit the loop after the first harvested crop is found
                    }
                    if (obj.Category == StardewValley.Object.VegetableCategory ||
                        obj.Category == StardewValley.Object.FruitsCategory || obj.Category == StardewValley.Object.MilkCategory )
                    {
                        
                        if (!Config.VibrateOnCropAndMilkCollected) return;
                        this.Monitor.Log("Vegetable", LogLevel.Debug);
                        if (obj.Quality == StardewValley.Object.medQuality) buttplugManager.VibrateDevicePulse(Config.SilverLevel);
                        else if (obj.Quality == StardewValley.Object.highQuality) buttplugManager.VibrateDevicePulse(Config.GoldLevel, 650);
                        else if (obj.Quality == StardewValley.Object.bestQuality) buttplugManager.VibrateDevicePulse(Config.IridiumLevel, 1200);
                        // Vibrate the device
                        else buttplugManager.VibrateDevicePulse(Config.CropAndMilkBasic); // Adjust the power level as desired
                        break; // Exit the loop after the first harvested crop is found
                    }
                }
            }
            foreach (ItemStackSizeChange change in e.QuantityChanged)
            {
                // Check if the changed item is a fish
                if (change.Item is StardewValley.Object obj)
                {
                    if (obj.Category == StardewValley.Object.FishCategory && Config.VibrateOnFishCollected)
                    {
                        if (obj.Quality == StardewValley.Object.medQuality)
                            buttplugManager.VibrateDevicePulse(Config.SilverLevel);
                        else if (obj.Quality == StardewValley.Object.highQuality)
                            buttplugManager.VibrateDevicePulse(Config.GoldLevel, 650);
                        else if (obj.Quality == StardewValley.Object.bestQuality)
                            buttplugManager.VibrateDevicePulse(Config.IridiumLevel, 1000);
                        else
                            buttplugManager.VibrateDevicePulse(Config.FishCollectedBasic); // Adjust the power level as desired
                        break; // Exit the loop after the first harvested crop is found
                    }
                    if (obj.Category == StardewValley.Object.VegetableCategory ||
                        obj.Category == StardewValley.Object.FruitsCategory)
                    {
                        if (!Config.VibrateOnCropAndMilkCollected) return;
                        if (obj.Quality == StardewValley.Object.medQuality)
                            buttplugManager.VibrateDevicePulse(Config.SilverLevel);
                        else if (obj.Quality == StardewValley.Object.highQuality)
                            buttplugManager.VibrateDevicePulse(Config.GoldLevel, 650);
                        else if (obj.Quality == StardewValley.Object.bestQuality)
                            buttplugManager.VibrateDevicePulse(Config.IridiumLevel, 1200);
                        else
                            buttplugManager.VibrateDevicePulse(Config.CropAndMilkBasic); // Adjust the power level as desired
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
                    if (feature.Value is Tree tree && Config.VibrateOnTreeBroken)
                    {
                        // Tree is fully chopped
                            Task.Run(async () =>
                            {
                                this.Monitor.Log($"{Game1.player.Name} VIBRATING AT {80}.", LogLevel.Debug);
                                await buttplugManager.VibrateDevice(Config.TreeBrokenLevel);
                                await Task.Delay(420);
                                await buttplugManager.VibrateDevice(0);
                            });
                    }
                    if (feature.Value is ResourceClump resourceClump && Config.VibrateOnStoneBroken)
                    {
                        // Large rock or stub i think
                        Task.Run(async () =>
                        {
                            this.Monitor.Log($"{Game1.player.Name} VIBRATING AT {80}. for 1.2 seconds", LogLevel.Debug);
                            await buttplugManager.VibrateDevicePulse(80, 1200);
                        });
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
            if (e.Button == Config.StopVibrations)
            {
                // Stop Vibrations
                Task.Run(async () => await buttplugManager.StopDevices());
            }
            if (e.Button == Config.DisconnectButtplug)
            {
                Task.Run(async () =>
                {
                    //await buttplugManager.StopDevices();
                    await buttplugManager.DisconnectButtplug();
                });

            }
            if (e.Button == Config.ReconnectButtplug)
            {
                // Reconnect
                Task.Run(async () =>
                {
                    await buttplugManager.DisconnectButtplug();
                    await buttplugManager.ConnectButtplug(Monitor, Config.IntifaceIP);
                    await buttplugManager.ScanForDevices();
                });
            }
        }
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            _levelUps = 0;
            if (!Config.VibrateOnDayStart) return;
            //fishingMinigame.previousCaptureLevel = 0f;
            Task.Run(async () =>
            {
                await buttplugManager.VibrateDevice(Config.DayStartLevel);
                await Task.Delay(150);
                await buttplugManager.VibrateDevice(0);

                await Task.Delay(350);

                await buttplugManager.VibrateDevice(Config.DayStartLevel);
                await Task.Delay(150);
                await buttplugManager.VibrateDevice(0);
            });
        }
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            ArcadeMinigames(sender, e);
            fishingMinigame.isActive = Config.VibrateOnFishingMinigame;
            fishingMinigame.maxVibration = Config.MaxFishingVibration;
            // Check if the player's health has decreased since the last tick
            if (Game1.player.health < previousHealth)
            {
                if (!Config.VibrateOnDamageTaken) return;
                float intensity = (Config.DamageTakenMax * (1f - (float)Game1.player.health / (float)Game1.player.maxHealth));
                intensity = Math.Min(intensity, 100f);
                Task.Run(async () =>
                {
                    this.Monitor.Log($"{Game1.player.Name} VIBRATING AT {intensity}.", LogLevel.Trace);
                    await buttplugManager.VibrateDevice(intensity);
                    await Task.Delay(380);
                    await buttplugManager.VibrateDevice(0);
                });
            }
            // Update the previous health value for the next tick
            previousHealth = Game1.player.health;


            // Vibrate the plug for keepalive
            if (e.IsMultipleOf(1800))
            {
                if (!Config.KeepAlive) return;
                int intensity = 25;
                int duration = 250;
                buttplugManager.VibrateDevicePulse(intensity, duration);
            }
        }
 
        private void ArcadeMinigames(object sender, UpdateTickedEventArgs e)
        {
            //junimo kart lives trigger
            if (!Context.IsWorldReady || Game1.currentMinigame == null) return;

            if (Game1.currentMinigame is MineCart game && e.IsMultipleOf(5))
            {
                IReflectedField<int> minekartLives = Helper.Reflection.GetField<int>(game, "livesLeft");
                if (minekartLives.GetValue() < previousMinekartHealth)
                {
                    buttplugManager.VibrateDevicePulse(Config.ArcadeLevel);
                    this.Monitor.Log($"{Game1.player.Name} Life lost. Vibrating at {Config.ArcadeLevel}.", LogLevel.Debug);
                }
                previousMinekartHealth = minekartLives.GetValue();
                

                IReflectedField<int> livesLeft = Helper.Reflection.GetField<int>(game, "livesLeft");
            }
            //ABIGAIL GAME TRIGGER
            if (Game1.currentMinigame is AbigailGame abigailGame && e.IsMultipleOf(5))
            {
                IReflectedField<int> abigailLives = Helper.Reflection.GetField<int>(abigailGame, "lives");
                if (abigailLives.GetValue() != previousAbigailHealth)
                {
                    buttplugManager.VibrateDevicePulse(Config.ArcadeLevel, 600);
                    this.Monitor.Log($"{Game1.player.Name} Life lost. Vibrating at {Config.ArcadeLevel}.", LogLevel.Debug);
                }
                previousAbigailHealth = abigailLives.GetValue();
                
                IReflectedField<int> coinsField = Helper.Reflection.GetField<int>(abigailGame, "coins");
                int currentCoins = coinsField.GetValue();
                if (currentCoins > previousCoins)
                {
                    // Coin collected, trigger vibration
                    buttplugManager.VibrateDevicePulse(Config.ArcadeLevel, 600);
                    Monitor.Log($"{Game1.player.Name} Coin collected. Vibrating at {Config.ArcadeLevel}.", LogLevel.Debug);
                }
                previousCoins = currentCoins;
                
            }
        }
        public void Unload()
        {
            buttplugManager.DisconnectButtplug();
        }
    }
}

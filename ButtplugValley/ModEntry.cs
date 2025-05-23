using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace ButtplugValley
{
    internal sealed class ModEntry : Mod
    {
        private static BPManager buttplugManager;
        private static ModConfig SConfig;
        private ModConfig Config;
        private FishingMinigame fishingMinigame;
        private ConfigMenu configMenu;
        private bool isVibrating = false;
        private int previousHealth;
        private int previousCoins;
        private int _levelUps;
        private bool wasRidingHorse = false;

        //private const int CoffeeBeansID = 433;
        //private const int WoolID = 440;

        private const string CoffeeBeansID = "(O)433";
        private const string WoolID = "(O)440";

        //Arcade Machines
        private int previousMinekartHealth;
        private int previousAbigailHealth;
        private int previousPowerupCount;
        private bool isGameOverAbigail = false;
        
        public static IMonitor StaticMonitor { get; private set; }
        public static BPManager StaticButtplugManager { get; private set; }
        
        public static ModConfig StaticConfig { get; private set; }

        private static IMonitor ModMonitor;

        private readonly static string[] darkClubSounds = {"badend", "fellatio01", "fellatio02", "fellatio03", "fellatio04", "fellatio05", "fuck01", "fuck02", "fuck03"};
        private readonly static string[] darkClubSoundsOther = {"moan01", "moan02", "moan03", "moan04", "moan05", "moan06", "moan07", "moan08", "moan09", "moan10", "moan11", "moan12", "moan13", "moan14", "moan15", "pant01", "pant02", "pant03", "pant04", "pant05"};

        
        private static bool isVibratingS = false;

        private static async Task VibrateDevicePulseSafe(float intensity, int duration)
        {
            if (isVibratingS) return;
            isVibratingS = true;
            try
            {
                StaticMonitor.Log($"Queuing vibration at {intensity}% intensity for {duration} milliseconds", LogLevel.Debug);
                await StaticButtplugManager.VibrateDevicePulse(intensity, duration);
                
            }
            catch (Exception ex)
            {
                StaticMonitor.Log($"Error during vibration: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                isVibratingS = false;
            }
        }

        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            SConfig = this.Config;
            buttplugManager = new BPManager();
            fishingMinigame = new FishingMinigame(helper, Monitor, buttplugManager);
            configMenu = new ConfigMenu(helper, Monitor, buttplugManager, Config, this.ModManifest);
            new FishingRod(helper, Monitor, buttplugManager, Config);
            Task.Run(async () =>
            {
                await buttplugManager.ConnectButtplug(Monitor, Config.IntifaceIP);
                await buttplugManager.ScanForDevices();

            });

            StaticMonitor = Monitor;
            StaticButtplugManager = buttplugManager;
            StaticConfig = Config;
            
            ModMonitor = this.Monitor;
            

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
            helper.Events.Display.MenuChanged += OnMenuChanged;
            

            var harmony = new Harmony(this.ModManifest.UniqueID);
            
            
            // Tree hit harmony patch
            harmony.Patch(
                original: AccessTools.Method(typeof(Tree), nameof(Tree.performToolAction)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(TreeHit_Postfix))
            );

            // Tree fell harmony patch
            harmony.Patch(
                original: AccessTools.Method(typeof(Tree), "performTreeFall"),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(TreeFall_Postfix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(WateringCan), nameof(WateringCan.DoFunction)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(WateringCan_Postfix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(Hoe), nameof(Hoe.DoFunction)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(Hoe_Postfix))
            );
            //Harmony kiss detect
            MethodInfo originalCheckKiss = AccessTools.Method(typeof(Farmer), nameof(Farmer.PerformKiss));           
            harmony.Patch(original: originalCheckKiss, new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Kissing_Postfix)));


            //SV has multiple ways of playing sound, so I need to capture all the sounds
            MethodInfo prefixCheckLocal2 = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.localSound));
            MethodInfo prefixCheckLocal = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.playSound));
            harmony.Patch(prefixCheckLocal, new HarmonyMethod(typeof(ModEntry), nameof(PlaySoundPrefix)));
            harmony.Patch(prefixCheckLocal2, new HarmonyMethod(typeof(ModEntry), nameof(PlaySoundPrefix)));
            var methodList = typeof(Game1).GetMethods();
            foreach (var method in methodList)
            {
                if (method.Name == "playSound" && method.GetParameters().Length > 0)
                {                  
                    harmony.Patch(method, new HarmonyMethod(typeof(ModEntry), nameof(PlaySoundPrefix)));
                }
            }
        }
        
        public static void TreeHit_Postfix(Tree __instance)
        {
            if (!StaticConfig.VibrateOnTreeHit) { return; }
            StaticMonitor.Log("Tree hit", LogLevel.Info);
            VibrateDevicePulseSafe(StaticConfig.TreeChopLevel, 300);
            // Use StaticButtplugManager as needed
        }

        // This method will be called after a tree falls
        public static void TreeFall_Postfix(Tree __instance)
        {
            if (!StaticConfig.VibrateOnTreeFell) { return; }
            StaticMonitor.Log("Tree fell", LogLevel.Info);
            VibrateDevicePulseSafe(StaticConfig.TreeFellLevel, 2000);
        }
        
        public static void WateringCan_Postfix(WateringCan __instance, GameLocation location, int x, int y, int power, Farmer who)
        {
            if (StaticConfig.VibrateOnToolUse == false) { return; }
            
            int intensity = StaticConfig.ToolUseLevel * (power+1);

            
            StaticMonitor.Log($"Watering can used with power {power}, intensity {intensity}", LogLevel.Info);
            
            VibrateDevicePulseSafe(intensity, 300);
        }

        public static void Hoe_Postfix(Hoe __instance, GameLocation location, int x, int y, int power, Farmer who)
        { 
            if (StaticConfig.VibrateOnToolUse == false) { return; }
            
            int intensity = StaticConfig.ToolUseLevel * (power + 1);

            StaticMonitor.Log($"Hoe used with power {power}, intensity {intensity}", LogLevel.Info);

            VibrateDevicePulseSafe(intensity, 300);
        }

        static async void OnSoundPlayed(string cueName)
        {                                  
            if (SConfig.VibrateOnSexScene) {
                // Almost all sex scenes in mods use these sounds
                if ((cueName == "slimeHit" || cueName == "fishSlap" || cueName == "gulp") && Game1.eventUp)
                {                 
                    buttplugManager.VibrateDevicePulse(SConfig.SexSceneLevel, 150);
                    return;
                }

                //Spesificlly for cumming sex scene
                if (cueName == "swordswipe" && Game1.eventUp)
                {
                    buttplugManager.VibrateDevicePulse(SConfig.SexSceneLevel, 600);
                    return;
                }
            }
            // VibrateOnRainsInteractionMod sex sound
            if (SConfig.VibrateOnRainsInteractionMod && cueName == "ButtHit")
            {                
                buttplugManager.VibrateDevicePulse(SConfig.RainsInteractionModLevel, 100);
                return;
            }
            if (SConfig.VibrateOnHorse)
            {
                bool isRidingHorse = Game1.player.isRidingHorse();
                if (isRidingHorse)
                {
                    // All step sounds while riding will be procesed as movement
                    // Don't add this to the queue, so if it's a little laggy, it will create a different pattern
                    if (cueName.Contains("Step"))
                    {                       
                        buttplugManager.VibrateDevicePulse(SConfig.HorseLevel, 100);
                        return;
                    }
                }
            }
            if (SConfig.VibrateOnDarkClubMoans)
            {
                // Sounds of machines, slaves, etc.
                foreach (string soundName in darkClubSoundsOther)
                {
                    if (soundName.Contains(cueName))
                    {                        
                        buttplugManager.VibrateDevicePulse(SConfig.DarkClubMoanLevel, 150);
                    }
                }
            }

            if(SConfig.VibrateOnDarkClubSex)
            {
                foreach (string soundName in darkClubSounds)
                {
                    if (soundName.Contains(cueName))
                    {
                        // There are multiple intensities during a sex scene separated by a number in the sound name
                        ICue testC = Game1.soundBank.GetCue(cueName);
                        testC.Play();
                        string numString = Regex.Match(cueName, @"\d+\.*\d*").Value;
                        int num = 0;
                        if (numString != "")
                        {
                            num = int.Parse(numString);
                        }
                        double power = SConfig.MaxDarkClubSexLevel;
                        if (num == 1)
                        {
                            power = Math.Round(power * 0.3);
                        }
                        if (num == 2)
                        {
                            power = Math.Round(power * 0.6);
                        }

                        // Sex sounds in this mod have usually have multiple seconds, so I need a loop with a delay
                        // Might be interesting to put a random deley in there
                        while (testC.IsPlaying)
                        {
                            if (!Game1.eventUp)
                            {
                                testC.Stop(AudioStopOptions.Immediate);
                                return;
                            }                            
                            buttplugManager.VibrateDevicePulse((float)power, 100);
                            await Task.Delay(300);
                        }

                    }
                }
            }
        }
        


        // Just to merge and ignore other arguments besides the sound name
        public static void PlaySoundPrefix(object __0)
        {            
            if (__0 is string soundName)
            {
                OnSoundPlayed(soundName);
            }           
        }

        private static async void Kissing_Postfix()
        {
            try
            {   if (SConfig.VibrateOnKiss)
                {                   
                    await buttplugManager.VibrateDevicePulse(100, 1000);
                }

            }
            catch (Exception ex)
            {
                ModMonitor.Log($"Failed in {nameof(Kissing_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        private async void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is DialogueBox && Config.VibrateOnDialogue)
            {
                Monitor.Log("Dialogue Box Triggered", LogLevel.Trace);
                await VibrateDevicePulseSafe(Config.DialogueLevel, 550);
            }
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
                Monitor.Log($"Defeated {defeatedEnemyCount} enemies", LogLevel.Trace);
                

                // Vibrate the device
                VibrateDevicePulseSafe(Config.EnemyKilledLevel, 400*defeatedEnemyCount);
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            buttplugManager.config = this.Config;
            this.configMenu.LoadConfigMenu();
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
                this.Monitor.Log($"DESTROYED {destroyedStoneCount} STONES", LogLevel.Trace);
                double durationmath = 3920 / (1 + (10 * Math.Exp(-0.16 * destroyedStoneCount)));
                int duration = Convert.ToInt32(durationmath);

                if (!StaticConfig.VibrateOnStoneBroken) {return;}

                // Vibrate the device
                this.Monitor.Log($"VIBRATING FOR {duration} milliseconds", LogLevel.Trace);
                VibrateDevicePulseSafe(Config.StoneBrokenLevel, duration);
            }
            GameLocation location = Game1.currentLocation;
            
            bool hasBrokenBranches = e.Removed.Any(pair =>
            {
                
                if (pair.Value is StardewValley.Object obj)
                {
                    return obj.Name == "Twig";
                }
                return false;
            });

            if (hasBrokenBranches)
            {
                // Vibrate the device when a broken branch is found
                this.Monitor.Log("Branch Broken", LogLevel.Trace);
                VibrateDevicePulseSafe(Config.TreeBrokenLevel, 400);
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
            
            this.Monitor.Log("Inventory Changed", LogLevel.Trace);

            // Check if any items were added from the inventory
            foreach (Item item in e.Added)
            {
                if (item != null)
                {
                    this.Monitor.Log($"Added Item: {item.Name}, Category: {item.getCategoryName()}, Category Id: {item.Category}", LogLevel.Trace);
                    if (item.Category == StardewValley.Object.FishCategory)
                    {
                        if (!Config.VibrateOnFishCollected) return;
                        this.Monitor.Log("Fish", LogLevel.Trace);
                        VibrateBasedOnQuality(item, Config.FishCollectedBasic);
                        break; // Exit the loop after the first harvested crop is found
                    }
                    if (item.Category == StardewValley.Object.VegetableCategory ||
                        item.Category == StardewValley.Object.FruitsCategory || 
                        item.Category == StardewValley.Object.MilkCategory || 
                        item.Category == StardewValley.Object.EggCategory || 
                        item.QualifiedItemId is CoffeeBeansID or WoolID)
                    {
                        
                        if (!Config.VibrateOnCropAndMilkCollected) return;
                        this.Monitor.Log("Crop or Milk Added", LogLevel.Trace);
                        VibrateBasedOnQuality(item, Config.CropAndMilkBasic);
                        break; // Exit the loop after the first harvested crop is found
                    }
                    if (item.Category == StardewValley.Object.flowersCategory)
                    {
                        if (!Config.VibrateOnFlowersCollected) return;
                        this.Monitor.Log("Flower Added", LogLevel.Trace);
                        VibrateBasedOnQuality(item, Config.FlowerBasic);
                        break; // Exit the loop after the first harvested crop is found
                    }
                    if (item.Category == StardewValley.Object.GreensCategory ||
                        item.Category == StardewValley.Object.sellAtFishShopCategory ||
                        item.QualifiedItemId == "(O)771") //771 is fiber i think
                    {
                        if (!Config.VibrateOnForagingCollected) return;
                        this.Monitor.Log("Foraging Added", LogLevel.Trace);
                        VibrateBasedOnQuality(item, Config.ForagingBasic);
                        break; // Exit the loop after the first harvested crop is found
                    }
                    if (item.Category == StardewValley.Object.metalResources || item.QualifiedItemId == "(O)390") //390 is stone
                    {
                        if (Config.StonePickedUpDebug)
                        {
                            //THIS IS TEMPORARY CODE FOR TESTING PURPOSES ==============================================================================================================
                            double durationmath = 3920 / (1 + (10 * Math.Exp(-0.16 * 1)));
                            VibrateDevicePulseSafe(Config.StoneBrokenLevel, Convert.ToInt32(durationmath));
                            break;
                        }
                    }
                }
            }
            foreach (ItemStackSizeChange change in e.QuantityChanged)
            {
                // Check if the changed item is a fish
                if (change.NewSize > change.OldSize)
                {
                    //this.Monitor.Log($"Changed Item: {obj.Name}, Category: {obj.getCategoryName()}, Category Id: {obj.Category}", LogLevel.Debug);
                    if (change.Item.Category == StardewValley.Object.FishCategory)
                    {
                        if (!Config.VibrateOnFishCollected) return;
                        VibrateBasedOnQuality(change.Item, Config.FishCollectedBasic);
                        break; // Exit the loop after the first harvested crop is found
                    }

                    if (change.Item.Category == StardewValley.Object.metalResources || change.Item.QualifiedItemId == "(O)390") //390 is stone
                    {
                        if (Config.StonePickedUpDebug)
                        {
                            //THIS IS TEMPORARY CODE FOR TESTING PURPOSES ===================================================================================================================================
                            double durationmath = 3920 / (1 + (10 * Math.Exp(-0.16 * 1)));
                            VibrateDevicePulseSafe(Config.StoneBrokenLevel, Convert.ToInt32(durationmath));
                            break;
                        }
                    }
                    
                    if (change.Item.Category == StardewValley.Object.VegetableCategory ||
                        change.Item.Category == StardewValley.Object.FruitsCategory ||
                        change.Item.Category == StardewValley.Object.MilkCategory ||
                        change.Item.Category == StardewValley.Object.EggCategory ||
                        change.Item.QualifiedItemId is CoffeeBeansID or WoolID)
                    {
                        if (!Config.VibrateOnCropAndMilkCollected) return;
                        this.Monitor.Log("Crop or Milk Changed", LogLevel.Trace);
                        VibrateBasedOnQuality(change.Item, Config.CropAndMilkBasic);
                        break; // Exit the loop after the first harvested crop is found
                    }
                    if (change.Item.Category == StardewValley.Object.flowersCategory)
                    {
                        if (!Config.VibrateOnFlowersCollected) return;
                        this.Monitor.Log("Flower Changed", LogLevel.Trace);
                        VibrateBasedOnQuality(change.Item, Config.FlowerBasic);
                        break; // Exit the loop after the first harvested crop is found
                    }
                    if (change.Item.Category == StardewValley.Object.GreensCategory ||
                        change.Item.Category == StardewValley.Object.sellAtFishShopCategory)
                    {
                        if (!Config.VibrateOnForagingCollected) return;
                        this.Monitor.Log("Foraging Changed", LogLevel.Trace);
                        VibrateBasedOnQuality(change.Item, Config.ForagingBasic);
                        break; // Exit the loop after the first harvested crop is found
                    }
                }
            }
        }

        private void VibrateBasedOnQuality(Item obj, int basicLevel)
        {
            this.Monitor.Log("Vibrating based on quality", LogLevel.Trace);
            switch (obj.Quality)
            {
                case StardewValley.Object.medQuality:
                    _ = VibrateDevicePulseSafe(Config.SilverLevel, 400);
                    break;
                case StardewValley.Object.highQuality:
                    _ = VibrateDevicePulseSafe(Config.GoldLevel, 650);
                    break;
                case StardewValley.Object.bestQuality:
                    _ = VibrateDevicePulseSafe(Config.IridiumLevel, 1200);
                    break;
                default:
                    _ = VibrateDevicePulseSafe(basicLevel, 400); // Adjust the power level as desired
                    break;
            }
        }

        private void OnTerrainFeatureListChanged(object sender, TerrainFeatureListChangedEventArgs e)
        {
            if (e.IsCurrentLocation)
            {
                foreach (var feature in e.Removed)
                {
                    
                    //if feature is grass
                    if (feature.Value is Grass grass && Config.VibrateOnGrass)
                    {
                        Monitor.Log($"Removed {feature.Value.GetType().Name}", LogLevel.Trace);
                        Task.Run(async () =>
                        {
                            this.Monitor.Log($"{Game1.player.Name} VIBRATING AT {Config.GrassLevel}.", LogLevel.Trace);
                            await VibrateDevicePulseSafe(Config.GrassLevel, 300);
                        });
                    }
                    
                    if (feature.Value is Tree tree && Config.VibrateOnTreeBroken)
                    {
                        // Tree is fully chopped
                            Task.Run(async () =>
                            {
                                this.Monitor.Log($"{Game1.player.Name} VIBRATING AT {80}.", LogLevel.Trace);
                                await buttplugManager.VibrateDevicePulse(Config.TreeBrokenLevel, 420);
                            });
                    }
                    if (feature.Value is ResourceClump resourceClump && Config.VibrateOnStoneBroken)
                    {
                        // Large rock or stub i think
                        Task.Run(async () =>
                        {
                            await VibrateDevicePulseSafe(StaticConfig.StoneBrokenLevel, 1200);
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
                    await buttplugManager.VibrateDevicePulse(intensity, 380);
                });
            }
            // Update the previous health value for the next tick
            previousHealth = Game1.player.health;


            // Vibrate the plug for keepalive
            if (e.IsMultipleOf((uint)Config.KeepAliveInterval*60))
            {
                if (!Config.KeepAlive) return;
                int duration = 250;
                VibrateDevicePulseSafe(Config.KeepAliveLevel, duration);
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
                    VibrateDevicePulseSafe(Config.ArcadeLevel, 400);
                    this.Monitor.Log($"{Game1.player.Name} Life lost. Vibrating at {Config.ArcadeLevel}.", LogLevel.Trace);
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
                    VibrateDevicePulseSafe(Config.ArcadeLevel, 600);
                    this.Monitor.Log($"{Game1.player.Name} Life lost. Vibrating at {Config.ArcadeLevel}.", LogLevel.Trace);
                }
                previousAbigailHealth = abigailLives.GetValue();
                
                IReflectedField<int> coinsField = Helper.Reflection.GetField<int>(abigailGame, "coins");
                int currentCoins = coinsField.GetValue();
                if (currentCoins > previousCoins)
                {
                    // Coin collected, trigger vibration
                    VibrateDevicePulseSafe(Config.ArcadeLevel, 600);
                    Monitor.Log($"{Game1.player.Name} Coin collected. Vibrating at {Config.ArcadeLevel}.", LogLevel.Trace);
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

using MelonLoader;
using UniverseLib.Input;
using MoreRushes.Rush;
using MoreRushes.UI;

[assembly: MelonInfo(typeof(MoreRushes.MoreRushes), "MoreRushes", "1.1.0", "joeyexists", null)]
[assembly: MelonGame("Little Flag Software, LLC", "Neon White")]
[assembly: MelonColor(214, 111, 49, 255)]

namespace MoreRushes
{
    public class MoreRushes : MelonMod
    {
        public static bool IsActive { get; private set; }

        internal static MoreRushes ModInstance { get; private set; }
        internal static new HarmonyLib.Harmony HarmonyInstance { get; private set; }
        
        private static bool _isAntiCheatRegistered;
        private static bool _isWaitingForLevelRushToEnd;

        public override void OnLateInitializeMelon()
        {
            ModInstance = this;
            HarmonyInstance = new("joeyexists.MoreRushes");
            
            Settings.Initialize();

            Singleton<Game>.Instance.OnInitializationComplete += 
                OnGameInitialized;
        }

        private static void OnGameInitialized() =>
            SetModActive(Settings.modEnabledEntry.Value);

        public static void SetModActive(bool active)
        {
            if (active != IsActive)
            {
                if (active) EnableMod();
                else DisableMod();
            }
        }

        private static void EnableMod()
        {
            if (LevelRush.IsLevelRush())
            {
                _isWaitingForLevelRushToEnd = true;
                return;
            }

            RegisterAntiCheat();
            Patching.ApplyPatches();

            if (Settings.rushTextEnabledEntry.Value) 
                RushText.EnsureCreated();

            RushManager.ReplaceAllPickupsAndVendors();

            if (Settings.rushModeEntry.Value == RushMode.Random && Settings.rushSeedEntry.Value != 0)
                RushManager.SetSeed(Settings.rushSeedEntry.Value);

            Singleton<Game>.Instance.OnLevelLoadComplete += 
                GhostManager.OnLevelLoadComplete;

            var numPatches = HarmonyInstance.GetPatchedMethods().Count();
            MelonLogger.Msg($"Enabled (v{ModInstance.Info.Version}) - " +
                $"Ran {numPatches} patch{(numPatches == 1 ? "" : "es")}.");

            IsActive = true;
        }

        private static void DisableMod()
        {
            Patching.RemovePatches();

            RushText.DestroyInstance();

            Singleton<Game>.Instance.OnLevelLoadComplete -=
                GhostManager.OnLevelLoadComplete;
            GhostManager.ClearGhost();

            MelonLogger.Msg("Disabled.");

            IsActive = false;
        }

        public override void OnUpdate()
        {
            if (!IsActive) return;

            if (InputManager.GetKeyDown(Settings.incrementRushSeedHotkeyEntry.Value) &&
                Settings.rushModeEntry.Value == RushMode.Random &&
                !LevelRush.IsLevelRush())
            {
                uint seed = RushManager.CurrentSeed + 1;
                if (seed == 0) seed++;

                RushManager.SetSeed(seed);
            }    
        }

        internal static void DebugLog(string message)
        {
            if (Settings.debugModeEntry.Value)
                MelonLogger.Msg($"[DEBUG] {message}");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if(_isAntiCheatRegistered && !IsActive && sceneName == "HUB_HEAVEN")
                UnregisterAntiCheat();

            if (_isWaitingForLevelRushToEnd && !LevelRush.IsLevelRush())
            {
                _isWaitingForLevelRushToEnd = false;
                if (Settings.modEnabledEntry.Value && !IsActive)
                    EnableMod();
            }
        }

        private static void RegisterAntiCheat()
        {
            NeonLite.Modules.Anticheat.Register(ModInstance.MelonAssembly);
            _isAntiCheatRegistered = true;
        }
        private static void UnregisterAntiCheat()
        {
            NeonLite.Modules.Anticheat.Unregister(ModInstance.MelonAssembly);
            _isAntiCheatRegistered = false;
        }
    }
}
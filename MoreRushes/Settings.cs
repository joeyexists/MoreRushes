using MelonLoader;
using UnityEngine;
using MoreRushes.Rush;
using MoreRushes.UI;

namespace MoreRushes
{
    public static class Settings
    {
        public static MelonPreferences_Category category;

        public static MelonPreferences_Entry<bool> modEnabledEntry;
        public static MelonPreferences_Entry<RushMode> rushModeEntry;

        public static MelonPreferences_Entry<bool> useCustomGhostsEntry;

        public static MelonPreferences_Entry<uint> rushSeedEntry;
        public static MelonPreferences_Entry<bool> rushTextEnabledEntry;

        public static MelonPreferences_Entry<int> rushTextSizeEntry;

        public static MelonPreferences_Entry<RushTextAnchor> rushTextAnchorEntry;

        public static MelonPreferences_Entry<bool> showSeedTextOnlyEntry;

        public static MelonPreferences_Entry<KeyCode> incrementRushSeedHotkeyEntry;

        public static MelonPreferences_Entry<Color32> seedTextColorEntry;
        public static MelonPreferences_Entry<Color32> purifyColorEntry;
        public static MelonPreferences_Entry<Color32> elevateColorEntry;
        public static MelonPreferences_Entry<Color32> godspeedColorEntry;
        public static MelonPreferences_Entry<Color32> stompColorEntry;
        public static MelonPreferences_Entry<Color32> fireballColorEntry;
        public static MelonPreferences_Entry<Color32> dominionColorEntry;
        public static MelonPreferences_Entry<Color32> randomRushColorEntry;

        public static MelonPreferences_Entry<bool> debugModeEntry;

        public static void Initialize()
        {
            category = MelonPreferences.CreateCategory(MoreRushes.ModInstance.Info.Name);

            modEnabledEntry = category.CreateEntry("Enabled", false,
                description: "Triggers anti-cheat. To reset it, return to the hub.");

            rushModeEntry = category.CreateEntry("Rush Mode", RushMode.Purify);

            useCustomGhostsEntry = category.CreateEntry("Enable Rush Ghosts", false,
                description: "Creates and displays ghosts for the selected rush mode when running individual levels.\n" +
                "Note: each completed random seed creates a new ghost.");

            rushSeedEntry = category.CreateEntry("Rush Seed", (uint)0,
                description: "The seed used for Random Rush.\n" +
                "Leave at 0 to generate a new seed at the start of each run.");

            rushTextEnabledEntry = category.CreateEntry("Display Rush", true,
                description: "Shows your current rush mode (and seed if running Random Rush.)");

            rushTextSizeEntry = category.CreateEntry("Rush Text Font Size", 32,
                validator: new MelonLoader.Preferences.ValueRange<int>(24, 52));

            rushTextAnchorEntry = category.CreateEntry("Rush Text Anchor", RushTextAnchor.BottomRight);

            showSeedTextOnlyEntry = category.CreateEntry("Only Show Seed Text", false,
                description: "Hides the main rush text, only showing the seed text for random rushes.");

            incrementRushSeedHotkeyEntry = category.CreateEntry("Increment Rush Seed By 1 Hotkey",
                KeyCode.None, is_hidden: true);

            seedTextColorEntry = category.CreateEntry("Seed Text Color Entry", new Color32(242, 242, 255, 255), is_hidden: true);
            purifyColorEntry = category.CreateEntry("Purify Rush Text Color Entry", new Color32(159, 0, 255, 255), is_hidden: true);
            elevateColorEntry = category.CreateEntry("Elevate Rush Text Color Entry", new Color32(255, 216, 0, 255), is_hidden: true);
            godspeedColorEntry = category.CreateEntry("Godspeed Rush Text Color Entry", new Color32(0, 114, 255, 255), is_hidden: true);
            stompColorEntry = category.CreateEntry("Stomp Rush Text Color Entry", new Color32(0, 178, 0, 255), is_hidden: true);
            fireballColorEntry = category.CreateEntry("Fireball Rush Text Color Entry", new Color32(226, 0, 0, 255), is_hidden: true);
            dominionColorEntry = category.CreateEntry("Dominion Rush Text Color Entry", new Color32(0, 216, 205, 255), is_hidden: true);
            randomRushColorEntry = category.CreateEntry("Random Rush Text Color Entry", new Color32(203, 0, 255, 255), is_hidden: true);

            debugModeEntry = category.CreateEntry("Debug Mode", false,
                description: "Enables debug logging.", is_hidden: true);

            modEnabledEntry.OnEntryValueChanged.Subscribe((_, enable) =>
               MoreRushes.SetModActive(enable));

            rushModeEntry.OnEntryValueChanged.Subscribe((_, newRush) => RushManager.SetRush(newRush));

            useCustomGhostsEntry.OnEntryValueChanged.Subscribe((_, _) =>
            {
                if (MoreRushes.IsActive)
                    GhostManager.ClearGhost();
            });

            rushSeedEntry.OnEntryValueChanged.Subscribe((_, newSeed) =>
            {
            if (MoreRushes.IsActive && newSeed != 0 && rushModeEntry.Value == RushMode.Random && !LevelRush.IsLevelRush())
                    RushManager.SetSeed(newSeed);
            });

            rushTextEnabledEntry.OnEntryValueChanged.Subscribe((_, enable) =>
            {
                if (MoreRushes.IsActive)
                {
                    if (enable) RushText.EnsureCreated();
                    else RushText.DestroyInstance();
                }
            });

            showSeedTextOnlyEntry.OnEntryValueChanged.Subscribe((_, _) => RushText.ApplyOnlyShowSeedTextSetting());

            rushTextSizeEntry.OnEntryValueChanged.Subscribe((_, _) => RushText.ApplyFontSizeSetting());
            rushTextAnchorEntry.OnEntryValueChanged.Subscribe((_, _) => RushText.ApplyAnchorSetting());

            purifyColorEntry.OnEntryValueChanged.Subscribe((_, _) => RushText.UpdateRush());
            elevateColorEntry.OnEntryValueChanged.Subscribe((_, _) => RushText.UpdateRush());
            godspeedColorEntry.OnEntryValueChanged.Subscribe((_, _) => RushText.UpdateRush());
            stompColorEntry.OnEntryValueChanged.Subscribe((_, _) => RushText.UpdateRush());
            fireballColorEntry.OnEntryValueChanged.Subscribe((_, _) => RushText.UpdateRush());
            dominionColorEntry.OnEntryValueChanged.Subscribe((_, _) => RushText.UpdateRush());
            randomRushColorEntry.OnEntryValueChanged.Subscribe((_, _) => RushText.UpdateRush());
            seedTextColorEntry.OnEntryValueChanged.Subscribe((_, _) => RushText.UpdateRush());
        }
    }
}

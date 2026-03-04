using System.Reflection;
using HarmonyLib;
using UnityEngine;
using MoreRushes.Rush;

namespace MoreRushes
{
    internal static class Patching
    {
        public static bool IsPatched { get; private set; }

        // CardPickup.Spawn
        private static readonly MethodInfo CardPickup_Spawn_Method =
            AccessTools.Method(typeof(CardPickup), "Spawn");
        private static readonly HarmonyMethod CardPickup_Spawn_PrefixPatch =
            new(AccessTools.Method(typeof(Patching), nameof(CardPickup_Spawn_Prefix)));

        // CardVendor.Setup
        private static readonly MethodInfo CardVendor_Setup_Method =
            AccessTools.Method(typeof(CardVendor), "Setup");
        private static readonly HarmonyMethod CardVendor_Setup_PrefixPatch =
            new(AccessTools.Method(typeof(Patching), nameof(CardVendor_Setup_Prefix)));

        // Game.LevelSetupRoutine
        private static readonly MethodInfo Game_LevelSetupRoutine_Method =
            AccessTools.Method(typeof(Game), "LevelSetupRoutine");
        private static readonly HarmonyMethod Game_LevelSetupRoutine_PrefixPatch =
            new(AccessTools.Method(typeof(Patching), nameof(Game_LevelSetupRoutine_Prefix)));

        // EnemyJumper.DropCard
        private static readonly MethodInfo EnemyJumper_DropCard_Method =
            AccessTools.Method(typeof(EnemyJumper), "DropCard");
        private static readonly HarmonyMethod EnemyJumper_DropCard_PrefixPatch =
            new(AccessTools.Method(typeof(Patching), nameof(EnemyJumper_DropCard_Prefix)));
        private static readonly AccessTools.FieldRef<EnemyJumper, float> EnemyJumper_HeightRef =
            AccessTools.FieldRefAccess<EnemyJumper, float>("_height");
        private static readonly AccessTools.FieldRef<EnemyJumper, EnemyWaypoint> EnemyJumper_CurrentWaypointRef =
            AccessTools.FieldRefAccess<EnemyJumper, EnemyWaypoint>("currentWaypoint");

        // CardVendor.VendCard
        private static readonly MethodInfo CardVendor_VendCard_Method =
            AccessTools.Method(typeof(CardVendor), "VendCard");
        private static readonly HarmonyMethod CardVendor_VendCard_PrefixPatch =
            new(AccessTools.Method(typeof(Patching), nameof(CardVendor_VendCard_Prefix)));
        private static readonly AccessTools.FieldRef<CardVendor, bool> CardVendor_IsVendingRef =
            AccessTools.FieldRefAccess<CardVendor, bool>("_isVending");
        private static readonly AccessTools.FieldRef<CardVendor, bool> CardVendor_IsCardSpawnedAndNotCollectedRef =
            AccessTools.FieldRefAccess<CardVendor, bool>("_isCardSpawnedAndNotCollected");
        private static readonly Action<CardVendor> CardVendor_OnCardPickup =
            AccessTools.MethodDelegate<Action<CardVendor>>(AccessTools.Method(typeof(CardVendor), "OnCardPickup"));
        public static readonly AccessTools.FieldRef<CardVendor, Vector3> CardVendor_StartPositionRef =
            AccessTools.FieldRefAccess<CardVendor, Vector3>("_startPosition");

        // MenuScreenLevelRushComplete.OnSetVisible
        private static readonly MethodInfo MenuScreenLevelRushComplete_OnSetVisible_Method =
            AccessTools.Method(typeof(MenuScreenLevelRushComplete), "OnSetVisible");
        private static readonly HarmonyMethod MenuScreenLevelRushComplete_OnSetVisible_PostfixPatch =
            new(AccessTools.Method(typeof(Patching), nameof(MenuScreenLevelRushComplete_OnSetVisible_Postfix)));

        // LevelRush.OnQuitLevelRush
        private static readonly MethodInfo LevelRush_OnQuitLevelRush_Method =
            AccessTools.Method(typeof(LevelRush), "OnQuitLevelRush");
        // LevelRush.OnLevelRushComplete
        private static readonly MethodInfo LevelRush_OnLevelRushComplete_Method =
            AccessTools.Method(typeof(LevelRush), "OnLevelRushComplete");
        // LevelRush.ClearLevelRushStats
        private static readonly MethodInfo LevelRush_ClearLevelRushStats_Method =
            AccessTools.Method(typeof(LevelRush), "ClearLevelRushStats");

        private static readonly HarmonyMethod OnLevelRushEndPrefixPatch = 
            new(AccessTools.Method(typeof(Patching), nameof(OnLevelRushEndPrefix)));

        public static void ApplyPatches()
        {
            if (IsPatched)
                return;

            MoreRushes.HarmonyInstance.Patch(CardPickup_Spawn_Method, prefix: CardPickup_Spawn_PrefixPatch);
            MoreRushes.HarmonyInstance.Patch(CardVendor_Setup_Method, prefix: CardVendor_Setup_PrefixPatch);
            MoreRushes.HarmonyInstance.Patch(Game_LevelSetupRoutine_Method, prefix: Game_LevelSetupRoutine_PrefixPatch);
            MoreRushes.HarmonyInstance.Patch(EnemyJumper_DropCard_Method, prefix: EnemyJumper_DropCard_PrefixPatch);
            MoreRushes.HarmonyInstance.Patch(CardVendor_VendCard_Method, prefix: CardVendor_VendCard_PrefixPatch);
            MoreRushes.HarmonyInstance.Patch(LevelRush_ClearLevelRushStats_Method, prefix: OnLevelRushEndPrefixPatch);
            MoreRushes.HarmonyInstance.Patch(LevelRush_OnQuitLevelRush_Method, prefix: OnLevelRushEndPrefixPatch);
            MoreRushes.HarmonyInstance.Patch(LevelRush_OnLevelRushComplete_Method, prefix: OnLevelRushEndPrefixPatch);
            MoreRushes.HarmonyInstance.Patch(MenuScreenLevelRushComplete_OnSetVisible_Method, postfix: MenuScreenLevelRushComplete_OnSetVisible_PostfixPatch);
            IsPatched = true;
        }

        public static void RemovePatches()
        {
            if (!IsPatched)
                return;

            MoreRushes.HarmonyInstance.Unpatch(CardPickup_Spawn_Method, CardPickup_Spawn_PrefixPatch.method);
            MoreRushes.HarmonyInstance.Unpatch(CardVendor_Setup_Method, CardVendor_Setup_PrefixPatch.method);
            MoreRushes.HarmonyInstance.Unpatch(Game_LevelSetupRoutine_Method, Game_LevelSetupRoutine_PrefixPatch.method);
            MoreRushes.HarmonyInstance.Unpatch(EnemyJumper_DropCard_Method, EnemyJumper_DropCard_PrefixPatch.method);
            MoreRushes.HarmonyInstance.Unpatch(CardVendor_VendCard_Method, CardVendor_VendCard_PrefixPatch.method);
            MoreRushes.HarmonyInstance.Unpatch(LevelRush_ClearLevelRushStats_Method, OnLevelRushEndPrefixPatch.method);
            MoreRushes.HarmonyInstance.Unpatch(LevelRush_OnQuitLevelRush_Method, OnLevelRushEndPrefixPatch.method);
            MoreRushes.HarmonyInstance.Unpatch(LevelRush_OnLevelRushComplete_Method, OnLevelRushEndPrefixPatch.method);
            MoreRushes.HarmonyInstance.Unpatch(MenuScreenLevelRushComplete_OnSetVisible_Method, MenuScreenLevelRushComplete_OnSetVisible_PostfixPatch.method);
            IsPatched = false;
        }

        private static void CardPickup_Spawn_Prefix(ref PlayerCardData card, Vector3 position, bool autoPickup = false)
        {
            RushManager.TryReplaceCardOnPickupSpawn(ref card, position);
        }

        private static void CardVendor_Setup_Prefix(CardVendor __instance)
        {
            RushManager.ReplaceVendorCardOnSetup(__instance);
        }

        private static void Game_LevelSetupRoutine_Prefix(LevelData newLevel)
        {
            RushManager.SetupRushForLevelStart(newLevel);
            GhostManager.UpdateGhostForLevelStart(newLevel);
        }

        private static void OnLevelRushEndPrefix()
        {
            RushManager.ResetLevelRushFlags();
        }

        private static bool CardVendor_VendCard_Prefix(CardVendor __instance)
        {
            var startPosition = CardVendor_StartPositionRef(__instance);

            __instance.SetReloadShaderFXAmount(1f);
            CardVendor_IsVendingRef(__instance) = false;

            CardPickup pickup;
            bool success;

            RushPositionContext.OverrideHashPosition = startPosition;
            try
            {
                success = CardPickup.Spawn(
                    __instance._cardData,
                    __instance.transform.TransformPoint(__instance._cardSpawnPosition),
                    out pickup, 
                    delegate { CardVendor_OnCardPickup(__instance); }
                );
            }
            finally
            {
                RushPositionContext.OverrideHashPosition = null;
            }

            CardVendor_IsCardSpawnedAndNotCollectedRef(__instance) = true;
            __instance.transform.position = startPosition;
            __instance.stock--;
            __instance.stockText.text = __instance.stock.ToString();

            if (success)
                __instance.OnVendAction?.Invoke(pickup);

            if (__instance.stock == 0)
                __instance._uiCard.UICards[0].FadeOutBackground();

            return false;
        }

        private static bool EnemyJumper_DropCard_Prefix(EnemyJumper __instance, bool autoPickup = false)
        {
            if (__instance.dropsCard == null) return false;

            var jumperPos = __instance.transform.position;
            var hashPos = new Vector3(jumperPos.x, 0, jumperPos.z);
            RushPositionContext.OverrideHashPosition = hashPos;

            try
            {
                if (CardPickup.Spawn(__instance.dropsCard, __instance.transform.position, out var pickup, __instance.dropsCardCollectAction, autoPickup))
                {
                    var currentWaypoint = EnemyJumper_CurrentWaypointRef(__instance);
                    if (currentWaypoint != null)
                    {
                        RushManager.RegisterCardDroppedByJumper(pickup);

                        var height = EnemyJumper_HeightRef(__instance);
                        
                        pickup.transform.EasePositionTo(
                            currentWaypoint.transform.position + Vector3.up * height,
                            __instance.cardPickupFallTime, AxKEasing.EaseType.EaseOutQuad
                        );
                    }
                    else
                    {
                        Debug.LogWarning("Jumpers card drop is null?");
                    }
                }
            }
            finally
            {
                RushPositionContext.OverrideHashPosition = null;
            }

            return false;
        }

        private static void MenuScreenLevelRushComplete_OnSetVisible_Postfix(MenuScreenLevelRushComplete __instance)
        {
            var moddedRush = RushManager.ActiveRush.ToString();
            var rushType = LevelRush.GetCurrentLevelRushType();

            string rushOwner = rushType switch
            {
                LevelRush.LevelRushType.WhiteRush => "White",
                LevelRush.LevelRushType.RedRush => "Red",
                LevelRush.LevelRushType.VioletRush => "Violet",
                LevelRush.LevelRushType.YellowRush => "Yellow",
                LevelRush.LevelRushType.MikeyRush => "Mikey",
                _ => null
            };

            if (rushOwner == null)
                return;

            var realm = !LevelRush.IsHellRush() ? "Heaven" : "Hell";

            // force english name
            __instance._rushName.textMeshProUGUI.text =
                $"{rushOwner}'s {moddedRush} {realm} Rush";
        }
    }
}

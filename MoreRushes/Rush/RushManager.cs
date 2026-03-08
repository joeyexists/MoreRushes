using UnityEngine;
using Object = UnityEngine.Object;
using MoreRushes.UI;

namespace MoreRushes.Rush
{
    internal static class RushManager
    {
        public static RushMode ActiveRush => Settings.rushModeEntry.Value;
        public static uint CurrentSeed { get; private set; }

        private static readonly Dictionary<RushMode, string> RushToCardId = new()
        {
            { RushMode.Purify, "MACHINEGUN" },
            { RushMode.Elevate, "PISTOL" },
            { RushMode.Godspeed, "RIFLE" },
            { RushMode.Stomp, "UZI" },
            { RushMode.Fireball, "SHOTGUN" },
            { RushMode.Mikey, "ROCKETLAUNCHER" }
        };

        private static readonly Dictionary<string, uint> RandomRushCardWeights = new()
        {
            { "MACHINEGUN", 2000 },
            { "PISTOL", 1900 },
            { "RIFLE", 1900 },
            { "UZI", 1300 },
            { "SHOTGUN", 1900 },
            { "ROCKETLAUNCHER", 999 },
            { "RAPTURE", 1 }
        };

        private static readonly Dictionary<RushMode, PlayerCardData> _rushToCardData = [];
        private static readonly WeightedPool<PlayerCardData> _randomCardPool = new();

        private static readonly HashSet<uint> _replacedCardHashes = [];
        private static readonly HashSet<int> _jumperCardInstanceIds = [];

        private static string _lastLevelId;
        private static bool _rushSeedIsFixed = false;
        private static bool _levelRushStarted = true;

        public static void RegisterCardDroppedByJumper(CardPickup pickup) =>
            _jumperCardInstanceIds.Add(pickup.GetInstanceID());

        public static bool EnsureCardCache()
        {
            if (_rushToCardData.Count > 0)
                return true;

            var gameData = Singleton<Game>.Instance.GetGameData();
            if (gameData == null) return false;

            foreach (var kvp in RushToCardId)
            {
                // cache card data for fixed rushes
                var card = gameData.GetCard(kvp.Value);
                if (card != null)
                    _rushToCardData[kvp.Key] = card;
            }

            foreach (var kvp in RandomRushCardWeights)
            {
                // cache card data for random rush
                var card = gameData.GetCard(kvp.Key);
                if (card != null)
                    _randomCardPool.Add(card, kvp.Value);
            }

            return true;
        }

        public static void SetupRushForLevelStart(LevelData level)
        {
            if (ActiveRush != RushMode.Random) 
                return;

            _jumperCardInstanceIds.Clear();

            if (IsLevelChanged(level))
            {
                ClearReplacedCardHashes();
                _lastLevelId = level.levelID;
            }

            if (LevelRush.IsLevelRush())
            {
                if (!_levelRushStarted)
                {
                    // first time starting this level rush
                    _levelRushStarted = true;

                    if (Settings.rushSeedEntry.Value != 0)
                    {
                        _rushSeedIsFixed = true;
                        if (CurrentSeed != Settings.rushSeedEntry.Value)
                            SetSeed(Settings.rushSeedEntry.Value, restarting: true);
                    }
                    else 
                        _rushSeedIsFixed = false;
                }

                if (!_rushSeedIsFixed) 
                    RandomizeSeed(restarting: true);

                return;
            }

            ResetLevelRushFlags();

            if (Settings.rushSeedEntry.Value != 0)
            {
                if (CurrentSeed != Settings.rushSeedEntry.Value)
                    SetSeed(Settings.rushSeedEntry.Value, restarting: true);
                return;
            }
            
            RandomizeSeed(restarting: true);
        }

        public static void ResetLevelRushFlags()
        {
            _levelRushStarted = false;
            _rushSeedIsFixed = false;
        }

        private static void ClearReplacedCardHashes()
        {
            if (_replacedCardHashes.Count > 0)
            {
                MoreRushes.DebugLog($"Cleared {_replacedCardHashes.Count} cached card hashes.");
                _replacedCardHashes.Clear();
            }
        }

        private static bool IsLevelChanged(LevelData level) =>
            level != null && level.levelID != _lastLevelId && level.type == LevelData.LevelType.Level;

        public static void SetRush(RushMode rush)
        {
            if (!MoreRushes.IsActive || LevelRush.IsLevelRush())
                return;

            if (rush == RushMode.Random)
            {
                if (Settings.rushSeedEntry.Value != 0)
                    SetSeed(Settings.rushSeedEntry.Value);
                else RandomizeSeed();
                return;
            }

            ReplaceAllPickupsAndVendors();
            GhostManager.ClearGhost();
            RushText.UpdateRush();
        }

        public static void SetSeed(uint seed, bool restarting = false)
        {
            CurrentSeed = seed;
            RushText.UpdateRush();
            ReplaceAllPickupsAndVendors();

            if (!restarting)
                GhostManager.ClearGhost();
        }

        public static void RandomizeSeed(bool restarting = false)
        {
            uint seed;
            do seed = RushSeedUtility.RandomNonZeroUInt();
            while (seed == CurrentSeed);

            SetSeed(seed, restarting);
        }

        public static PlayerCardData GetCardForActiveRush(Vector3 position)
        {
            if (!EnsureCardCache())
                return null;

            var rush = Settings.rushModeEntry.Value;

            if (rush != RushMode.Random)
                return _rushToCardData.TryGetValue(rush, out var card) ? card : null;

            var posToHash = RushPositionContext.OverrideHashPosition ?? position;

            uint posHash = RushSeedUtility.HashPosition(posToHash);
            uint combined = RushSeedUtility.CombineSeeds(CurrentSeed, posHash);
            var randomCard = _randomCardPool.GetDeterministic(combined);

            if (randomCard.cardID == "RAPTURE" && !_replacedCardHashes.Contains(posHash))
                MoreRushes.DebugLog($"Boof rolled at {position} on seed '{CurrentSeed}'!");

            _replacedCardHashes.Add(posHash);

            return randomCard;
        }

        public static void ReplaceAllPickupsAndVendors()
        {
            foreach (var pickup in Object.FindObjectsOfType<CardPickup>())
            {
                var card = pickup.GetPlayerCardData();

                var hashPos = _jumperCardInstanceIds.Contains(pickup.GetInstanceID())
                    ? new Vector3(pickup.transform.position.x, 0, pickup.transform.position.z)
                    : pickup.transform.position;

                if (!ShouldReplaceCard(card, hashPos))
                    continue;

                var newCard = GetCardForActiveRush(hashPos);
                if (newCard == null) continue;

                // maintain ammo overrides
                var playerCard = pickup.uiCard.GetCurrentPlayerCard();
                int ammo = playerCard.GetCurrentAmmo();
                int ammoOverride = (ammo == playerCard.GetMaxAmmo()) ? -1 : ammo;

                pickup.SetCard(newCard, ammoOverride);
            }

            foreach (var vendor in Object.FindObjectsOfType<CardVendor>())
            {
                var vendorStartPos = Patching.CardVendor_StartPositionRef(vendor);

                if (!ShouldReplaceCard(vendor._cardData, vendorStartPos))
                    continue;

                var newCard = GetCardForActiveRush(vendorStartPos);
                vendor.SetCard(newCard);
                if (vendor.stock == 0)
                    vendor._uiCard.UICards[0].FadeOutBackground();
            }
        }

        public static void TryReplaceCardOnPickupSpawn(ref PlayerCardData card, Vector3 position)
        {
            var vendorPos = RushPositionContext.OverrideHashPosition ?? position;

            if (!ShouldReplaceCard(card, vendorPos)) 
                return;

            var newCard = GetCardForActiveRush(vendorPos);
            if (newCard != null) 
                card = newCard;
        }

        public static void ReplaceVendorCardOnSetup(CardVendor vendor)
        {
            var hashPos = vendor.transform.position;
            if (!ShouldReplaceCard(vendor._cardData, hashPos))
                return;

            var newCard = GetCardForActiveRush(hashPos);
            if (newCard != null) 
                vendor._cardData = newCard;
        }

        private static bool ShouldReplaceCard(PlayerCardData card, Vector3 position)
        {
            if (!NeonLite.Modules.Anticheat.Active)
                return false;

            if (card == null)
                return false;

            if (card.cardType == PlayerCardData.Type.SpecialConsumableAutomatic)
                return false;

            if (card.consumableType is
                PlayerCardData.ConsumableType.Tutorial or
                PlayerCardData.ConsumableType.GiftCollectible or
                PlayerCardData.ConsumableType.LoreCollectible)
                return false;

            if (card.cardID == "RAPTURE")
                return CanReplaceBoofAt(position);

            return true;
        }

        public static bool CanReplaceBoofAt(Vector3 position)
        {
            if (ActiveRush != RushMode.Random)
                return false;

            uint posHash = RushSeedUtility.HashPosition(position);
            return _replacedCardHashes.Contains(posHash);
        }
    }
}

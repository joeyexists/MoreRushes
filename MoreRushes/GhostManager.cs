using MoreRushes.Rush;

namespace MoreRushes
{
    internal static class GhostManager
    {
        private static string _currentGhostName;
        private static bool _previousLevelWasRush;

        private static void SetGhost(string ghostName) =>
            NeonLite.Modules.Anticheat.SetGhostName(MoreRushes.ModInstance.MelonAssembly, ghostName);

        public static void OnLevelLoadComplete() =>
            _previousLevelWasRush = LevelRush.IsLevelRush();

        public static void UpdateGhostForLevelStart(LevelData level)
        {
            if (!CanUseCustomGhostForLevel(level))
            {
                ClearGhost();
                return;
            }

            var newGhostName = BuildGhostName();
            if (_currentGhostName != newGhostName)
            {
                SetGhost(newGhostName);
                _currentGhostName = newGhostName;

                MoreRushes.DebugLog($"Ghost name updated to '{newGhostName}'.");
            }
        }

        public static void ClearGhost()
        {
            if (_currentGhostName == null)
                return;

            SetGhost(null);
            _currentGhostName = null;
            MoreRushes.DebugLog("Ghost name cleared.");
        }

        private static string BuildGhostName()
        {
            var name = $"{Settings.rushModeEntry.Value}Rush";

            if (Settings.rushModeEntry.Value == RushMode.Random)
                name += $"_{RushManager.CurrentSeed}";

            return Path.Combine("MoreRushes", name);
        }

        private static bool TryEnsureGhostDirForLevel(LevelData level)
        {
            if (level?.type != LevelData.LevelType.Level)
                return false;

            var levelGhostDir = string.Empty;
            GhostUtils.GetPath(level.levelID, GhostUtils.GhostType.PersonalGhost, ref levelGhostDir);
            var customGhostDir = Path.Combine(levelGhostDir, "MoreRushes");

            if (!Directory.Exists(customGhostDir))
                MoreRushes.DebugLog($"Created custom ghost directory for level '{level.levelID}' at '{customGhostDir}'.");

            Directory.CreateDirectory(customGhostDir);
            return true;
        }

        private static bool CanUseCustomGhostForLevel(LevelData level) =>
            MoreRushes.IsActive &&
            Settings.useCustomGhostsEntry.Value &&
            !LevelRush.IsLevelRush() &&
            !_previousLevelWasRush &&
            TryEnsureGhostDirForLevel(level);
    }
}

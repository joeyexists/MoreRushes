using UnityEngine;

namespace MoreRushes.Rush
{
    internal static class RushPositionContext
    {
        [ThreadStatic] public static Vector3? OverrideHashPosition;
    }
}

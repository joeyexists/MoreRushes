using System.Security.Cryptography;
using UnityEngine;

namespace MoreRushes.Rush
{
    internal static class RushSeedUtility
    {
        private static readonly RandomNumberGenerator _rng =
            RandomNumberGenerator.Create();

        private static readonly byte[] _buffer = new byte[4];

        public static uint RandomNonZeroUInt()
        {
            uint value;
            do
            {
                _rng.GetBytes(_buffer);
                value = BitConverter.ToUInt32(_buffer, 0);
            } while (value == 0);

            return value;
        }

        public static uint HashPosition(Vector3 pos)
        {
            unchecked
            {
                int x = Quantize(pos.x, 0.1f);
                int y = Quantize(pos.y, 0.1f);
                int z = Quantize(pos.z, 0.1f);

                uint hash = 2166136261u;

                hash = (hash ^ (uint)x) * 16777619u;
                hash = (hash ^ (uint)y) * 16777619u;
                hash = (hash ^ (uint)z) * 16777619u;
                return hash;
            }
        }

        private static int Quantize(float v, float precision) =>
            Mathf.RoundToInt(v / precision);

        public static uint CombineSeeds(uint a, uint b) =>
            unchecked(a ^ (b + 0x9e3779b9 + (a << 6) + (a >> 2)));

        public static uint NextUInt(ref uint state)
        {
            unchecked
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                return state;
            }
        }
    }
}

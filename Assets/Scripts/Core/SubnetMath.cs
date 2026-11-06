using System;

namespace RuoJingo
{
    /// <summary>IPv4 subnetting helpers. Usable hosts = 2^(32 - prefix) - 2.</summary>
    public static class SubnetMath
    {
        public const int MinPrefix = 0;
        public const int MaxUsablePrefix = 30; // /31 and /32 are special cases outside this course's formula

        public static long UsableHosts(int prefix)
        {
            if (prefix < MinPrefix || prefix > MaxUsablePrefix)
                throw new ArgumentOutOfRangeException(nameof(prefix), "Prefix must be between /0 and /30.");
            return (1L << (32 - prefix)) - 2;
        }

        public static string MaskForPrefix(int prefix)
        {
            if (prefix < 0 || prefix > 32) throw new ArgumentOutOfRangeException(nameof(prefix));
            uint mask = prefix == 0 ? 0u : uint.MaxValue << (32 - prefix);
            return $"{(mask >> 24) & 255}.{(mask >> 16) & 255}.{(mask >> 8) & 255}.{mask & 255}";
        }

        /// <summary>The longest prefix (smallest subnet) that still has at least <paramref name="hosts"/> usable hosts.</summary>
        public static int SmallestPrefixForHosts(long hosts)
        {
            if (hosts < 1) throw new ArgumentOutOfRangeException(nameof(hosts));
            for (int p = MaxUsablePrefix; p >= MinPrefix; p--)
                if (UsableHosts(p) >= hosts) return p;
            throw new ArgumentOutOfRangeException(nameof(hosts), "Too many hosts for IPv4.");
        }

        /// <summary>Number of subnets created when a /originalPrefix network is split into /newPrefix subnets.</summary>
        public static long SubnetCount(int originalPrefix, int newPrefix)
        {
            if (newPrefix < originalPrefix || originalPrefix < 0 || newPrefix > 32)
                throw new ArgumentOutOfRangeException(nameof(newPrefix));
            return 1L << (newPrefix - originalPrefix);
        }
    }
}

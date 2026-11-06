using System;
using System.Collections.Generic;

namespace RuoJingo
{
    public static class ShuffleUtil
    {
        /// <summary>In-place Fisher-Yates shuffle.</summary>
        public static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}

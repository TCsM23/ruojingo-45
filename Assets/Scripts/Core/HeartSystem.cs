using System;

namespace RuoJingo
{
    /// <summary>Hearts for one lesson session: start full, lose one per wrong answer, lesson fails at 0.</summary>
    public class HeartSystem
    {
        public const int DefaultMaxHearts = 5;

        public int MaxHearts { get; }
        public int Current { get; private set; }
        public bool IsEmpty => Current <= 0;

        public event Action<int> Changed;

        public HeartSystem(int maxHearts = DefaultMaxHearts)
        {
            if (maxHearts < 1) throw new ArgumentOutOfRangeException(nameof(maxHearts), "Need at least one heart.");
            MaxHearts = maxHearts;
            Current = maxHearts;
        }

        /// <summary>Removes one heart (never below zero). Returns true if the player is now out of hearts.</summary>
        public bool LoseHeart()
        {
            if (Current > 0)
            {
                Current--;
                Changed?.Invoke(Current);
            }
            return IsEmpty;
        }

        public void Refill()
        {
            Current = MaxHearts;
            Changed?.Invoke(Current);
        }
    }
}

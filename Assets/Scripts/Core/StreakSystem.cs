using System;

namespace RuoJingo
{
    /// <summary>
    /// Daily streak. Completing at least one lesson on a calendar day keeps the streak alive;
    /// a full calendar day with no completed lesson resets it.
    /// </summary>
    public static class StreakSystem
    {
        /// <summary>Streak to display today: the stored value, or 0 if a day has been missed.</summary>
        public static int GetCurrentStreak(int storedStreak, DateTime? lastCompletedDay, DateTime today)
        {
            if (lastCompletedDay == null) return 0;
            int days = (today.Date - lastCompletedDay.Value.Date).Days;
            if (days < 0) return storedStreak; // clock moved backwards: don't punish the player
            return days <= 1 ? storedStreak : 0;
        }

        /// <summary>New streak value after a lesson is completed today.</summary>
        public static int RegisterLessonCompleted(int storedStreak, DateTime? lastCompletedDay, DateTime today)
        {
            if (lastCompletedDay == null) return 1;
            int days = (today.Date - lastCompletedDay.Value.Date).Days;
            if (days <= 0) return Math.Max(storedStreak, 1); // already counted today (or clock moved back)
            if (days == 1) return storedStreak + 1;          // consecutive day
            return 1;                                         // missed at least one day: start over
        }
    }
}

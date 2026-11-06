using System;
using System.Collections.Generic;

namespace RuoJingo
{
    /// <summary>Everything persisted to disk. Serialised with JsonUtility, so only public fields.</summary>
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public string playerName = "You";

        // Progress
        public int highestUnlockedUnit = 1;
        public List<int> passedUnits = new List<int>();

        // XP
        public int totalXp;
        public int weeklyXp;
        public string weekStartDate; // yyyy-MM-dd (Monday) the weeklyXp belongs to

        // Streak. lastPlayedDate = calendar day of the last *completed* lesson (yyyy-MM-dd).
        public int streak;
        public string lastPlayedDate;

        // Hearts left at the end of the last lesson (hearts refill to 5 at the start of every lesson).
        public int lastHearts = HeartSystem.DefaultMaxHearts;

        // Spaced repetition. lessonCounter increments every time a lesson starts and is the Leitner "clock".
        public int lessonCounter;
        public List<LeitnerEntry> leitner = new List<LeitnerEntry>();

        /// <summary>Repairs nulls / out-of-range values after loading an old or hand-edited file.</summary>
        public void Sanitize()
        {
            if (passedUnits == null) passedUnits = new List<int>();
            if (leitner == null) leitner = new List<LeitnerEntry>();
            if (string.IsNullOrEmpty(playerName)) playerName = "You";
            if (highestUnlockedUnit < 1) highestUnlockedUnit = 1;
            if (totalXp < 0) totalXp = 0;
            if (weeklyXp < 0) weeklyXp = 0;
            if (streak < 0) streak = 0;
            if (lessonCounter < 0) lessonCounter = 0;
            if (lastHearts < 0 || lastHearts > HeartSystem.DefaultMaxHearts) lastHearts = HeartSystem.DefaultMaxHearts;
            leitner.RemoveAll(e => e == null || string.IsNullOrEmpty(e.questionId));
            foreach (var e in leitner)
                e.box = Math.Max(1, Math.Min(SpacedRepetition.BoxCount, e.box));
        }
    }
}

using System;
using System.Linq;

namespace RuoJingo
{
    /// <summary>Applies a finished (passed or failed) lesson to the save data. Pure, so it can be unit tested.</summary>
    public static class ProgressService
    {
        public static LessonResult ApplyLessonResult(SaveData save, LessonRunner runner, DateTime today, int totalUnits)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));
            if (runner == null) throw new ArgumentNullException(nameof(runner));

            bool passed = runner.IsComplete; // finished every exercise with at least 1 heart left
            int xp = XpSystem.CalculateLessonXp(runner.CorrectCount, runner.WrongCount, passed);

            RollWeek(save, today);
            save.totalXp += xp;
            save.weeklyXp += xp;
            save.lastHearts = runner.Hearts.Current;

            var result = new LessonResult
            {
                UnitId = runner.UnitId,
                Passed = passed,
                CorrectCount = runner.CorrectCount,
                WrongCount = runner.WrongCount,
                TotalExercises = runner.Items.Count,
                HeartsLeft = runner.Hearts.Current,
                XpEarned = xp,
                Perfect = passed && runner.WrongCount == 0,
                Accuracy = runner.Accuracy,
                ReviewCount = runner.Items.Count(i => i.IsReview),
                Missed = runner.Missed.ToList(),
            };

            if (passed)
            {
                save.streak = StreakSystem.RegisterLessonCompleted(save.streak, DateUtil.ParseDay(save.lastPlayedDate), today);
                save.lastPlayedDate = DateUtil.ToDayString(today);

                if (!save.passedUnits.Contains(runner.UnitId)) save.passedUnits.Add(runner.UnitId);
                int next = runner.UnitId + 1;
                if (next <= totalUnits && next > save.highestUnlockedUnit)
                {
                    save.highestUnlockedUnit = next;
                    result.UnlockedUnitId = next;
                }
            }

            result.Streak = StreakSystem.GetCurrentStreak(save.streak, DateUtil.ParseDay(save.lastPlayedDate), today);
            result.TotalXp = save.totalXp;
            return result;
        }

        public static bool IsUnitUnlocked(SaveData save, int unitId) => unitId >= 1 && unitId <= save.highestUnlockedUnit;
        public static bool IsUnitPassed(SaveData save, int unitId) => save.passedUnits.Contains(unitId);

        /// <summary>Resets weekly XP when a new week (Monday) starts.</summary>
        public static void RollWeek(SaveData save, DateTime today)
        {
            string week = DateUtil.ToDayString(DateUtil.WeekStart(today));
            if (save.weekStartDate != week)
            {
                save.weekStartDate = week;
                save.weeklyXp = 0;
            }
        }
    }
}
